using System.Collections.Generic;
using Cthangover.Core.Bootstrap;
using Cthangover.Core.Events;
using Cthangover.Core.Scenes;
using Cthangover.Core.Settings;
using Cthangover.Core.UI;
using Cthangover.Core.Utils;
using Cthangover.Mods.Dialog;
using Cthangover.Mods.Interface.View;
using Godot;

namespace Cthangover.Mods.CthangoverGame.Tests
{
    /// <summary>
    /// Walks a new game of cthangover through the frames the site shows and saves
    /// a picture of each: locations entered with <see cref="SceneManager.SwitchScene"/>
    /// (their own scenarios start the dialog, so every frame is a real novel
    /// screen), the toolbar panels opened with <see cref="UiRegistry.Toggle"/> like
    /// the toolbar buttons do, and Murakami's story at the town entry played by
    /// answering its menu — the wolf scene and the card battle it starts.
    /// <para/>
    /// The prologue's scenarios have no locale keys (Russian only), so its dialog
    /// is aborted at once and its scenes are left out. Before every switch the
    /// clock is set to noon (a new game starts at night and the night lighting
    /// leaves only lamps visible) and the running dialog is aborted the way the
    /// scene editor's "Go to scene" does — otherwise the entered scene's scenario
    /// waits behind it. Before the panels the inventory is seeded and Murakami
    /// joins the party, so they are not empty. Waits are tick counts, not
    /// conditions on the picture; the checks only make sure each scene, panel and
    /// the battle were really reached. Result line:
    /// <c>[TEST_RESULT] name=site_shots passed=N failed=M</c>, exit 0/1, and a
    /// 240 s watchdog.
    /// </summary>
    public partial class SiteShotsDriver : Node
    {
        private const float TickSeconds = 0.1f;
        private const float TimeoutSeconds = 240f;
        private const int SettleTicks = 40;
        private const int PanelTicks = 15;
        private const int LineTicks = 25;
        private const long MinutesPerDay = 24 * 60;
        private const long Noon = 12 * 60;
        private const string Prologue = "prolog_alley";
        private const string StoryScene = "town_entry";
        private const int AlleyOption = 1;

        private static readonly string[] Locations =
        {
            "home_outside", "home_vestibule", "home_livingroom", "home_kitchen",
            "home_bedroom", "home_workroom", "home_basement", "town_outskirts"
        };

        private static readonly string[] Panels = { "inventory", "characters", "map", "journal", "skills" };

        private readonly string shotsDir;
        private readonly List<string> failures = new();
        private int passed;
        private int step;
        private int index;
        private int waitTicks;
        private double elapsed;
        private bool battleStarted;

        /// <summary>Creates the driver; <paramref name="shotsDir"/> null disables screenshots.</summary>
        public SiteShotsDriver(string shotsDir)
        {
            this.shotsDir = shotsDir;
            Name = "SiteShotsDriver";
            var timer = new Timer { WaitTime = TickSeconds, Autostart = true };
            AddChild(timer);
            timer.Timeout += Tick;
        }

        /// <summary>Listens for the battle scene.</summary>
        public override void _EnterTree() => ModEventBus.Subscribe<BattleStartedEvent>(OnBattleStarted);

        /// <summary>Stops listening.</summary>
        public override void _ExitTree() => ModEventBus.Unsubscribe<BattleStartedEvent>(OnBattleStarted);

        private SceneManager Scenes => GetTree().Root.GetNodeOrNull<SceneManager>("SceneManager");

        private static DialogBox Box => SceneContextNode.FindNode<DialogBox>("DialogBox");

        private void OnBattleStarted(BattleStartedEvent e) => battleStarted = true;

        private void Tick()
        {
            if (step < 0)
                return;
            elapsed += TickSeconds;
            if (elapsed > TimeoutSeconds)
            {
                Fail($"timeout at step {step} (scene='{SceneManager.GetCurrentSceneName()}')");
                Finish();
                return;
            }
            if (waitTicks > 0)
            {
                waitTicks--;
                return;
            }

            try
            {
                RunStep();
            }
            catch (System.Exception ex)
            {
                Fail($"step {step}: {ex.GetType().Name}: {ex.Message}");
                Finish();
            }
        }

        private void RunStep()
        {
            switch (step)
            {
                case 0:
                    if (!Boot.IsReady || SceneManager.GetCurrentSceneName() != Prologue)
                        return;
                    Check(true, $"entered '{Prologue}'");
                    index = 0;
                    Next(PanelTicks);
                    return;
                case 1:
                    GoTo(Locations[index]);
                    Next(0);
                    return;
                case 2:
                    if (SceneManager.GetCurrentSceneName() != Locations[index])
                        return;
                    Check(true, $"entered '{Locations[index]}'");
                    Next(SettleTicks);
                    return;
                case 3:
                    Shot("location_" + Locations[index]);
                    index++;
                    if (index < Locations.Length)
                    {
                        step = 1;
                        return;
                    }
                    Seed();
                    index = 0;
                    Next(PanelTicks);
                    return;
                case 4:
                    UiRegistry.Toggle(Panels[index]);
                    Next(PanelTicks);
                    return;
                case 5:
                    Check(UiHost.Instance.TryGetInstance(Panels[index], out var panel) && panel.Visible,
                        $"panel '{Panels[index]}' is open");
                    Shot("panel_" + Panels[index]);
                    UiRegistry.Toggle(Panels[index]);
                    index++;
                    if (index < Panels.Length)
                    {
                        step = 4;
                        waitTicks = PanelTicks;
                        return;
                    }
                    Next(PanelTicks);
                    return;
                case 6:
                    GoTo(StoryScene);
                    Next(0);
                    return;
                case 7:
                    if (SceneManager.GetCurrentSceneName() != StoryScene || Box?.IsWaitingAnswer != true)
                        return;
                    Check(true, $"Murakami's story waits for an answer in '{StoryScene}'");
                    Next(SettleTicks);
                    return;
                case 8:
                    Shot("novel_menu");
                    Check(Box.TrySelectVariant(AlleyOption), "the alley option is taken");
                    Next(SettleTicks);
                    return;
                case 9:
                    Shot("novel_wolf");
                    Box.NextAction();
                    Next(SettleTicks);
                    return;
                case 10:
                    Shot("novel_marao");
                    Next(0);
                    return;
                case 11:
                    if (battleStarted)
                    {
                        Check(true, "the card battle started");
                        Next(SettleTicks);
                        return;
                    }
                    if (Box != null && !Box.IsDialogEnded && !Box.IsWaitingAnswer)
                        Box.NextAction();
                    waitTicks = LineTicks;
                    return;
                case 12:
                    Shot("battle");
                    Next(SettleTicks);
                    return;
                case 13:
                    Shot("battle_later");
                    Finish();
                    return;
            }
        }

        /// <summary>Noon, no dialog left running, then the switch.</summary>
        private void GoTo(string scene)
        {
            var time = GameData.Instance.Runtime.Time;
            time.SetTime(time.Tick - time.Tick % MinutesPerDay + Noon);
            Box?.Abort();
            Scenes.SwitchScene(scene);
        }

        /// <summary>A few stacks for the inventory and a second hero for the party panel.</summary>
        private static void Seed()
        {
            var runtime = GameData.Instance.Runtime;
            runtime.Inventory.Add("food/ration", 3);
            runtime.Inventory.Add("food/wolf_meat", 4);
            runtime.Inventory.Add("resource/branches", 6);
            runtime.Inventory.Add("recipe/wolf_meat_to_ration");
            runtime.CharacterData.AddCharacterToParty("Murakami");
        }

        private void Next(int ticks)
        {
            step++;
            waitTicks = ticks;
        }

        private void Check(bool condition, string what)
        {
            if (condition)
            {
                passed++;
                GameLogger.Log("TEST", $"[PASS] {what}");
            }
            else
            {
                Fail(what);
            }
        }

        private void Fail(string what)
        {
            failures.Add(what);
            GameLogger.Log("TEST", $"[FAIL] {what}", LogLevel.Error);
        }

        private void Shot(string name)
        {
            if (string.IsNullOrEmpty(shotsDir) || DisplayServer.GetName() == "headless")
                return;
            DirAccess.MakeDirRecursiveAbsolute(shotsDir);
            ViewportCapture.SavePng(GetViewport(), shotsDir.TrimEnd('/') + "/" + name + ".png");
            GameLogger.Log("TEST", $"SiteShotsDriver: saved {name}.png");
        }

        private void Finish()
        {
            step = -1;
            GameLogger.Log("TEST", $"[TEST_RESULT] name=site_shots passed={passed} failed={failures.Count}");
            GetTree().Quit(failures.Count == 0 ? 0 : 1);
        }
    }
}
