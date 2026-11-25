namespace Cthangover.Mods.Game.StoryRoutes
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Cthangover.Core.Games;
    using Cthangover.Core.Factories.Impls;
    using Cthangover.Core.Mods;
    using Cthangover.Core.Mods.Testing;
    using Cthangover.Core.Scenes;
    using Cthangover.Core.Settings;
    using Cthangover.Mods.Dialog;
    using Cthangover.Mods.Interface.View;
    using Cthangover.Validator;
    using Godot;

    /// <summary>Checks references under the selected collection and its actual runtime route registry.</summary>
    public static class StoryRouteTests
    {
        [ModTest("cthangover/story_references", TimeoutSeconds = 180)]
        public static async Task SelectedCollection(ModTestContext context)
        {
            ModAssert.Equal("cthangover", ActiveGame.Id);
            var tree = (SceneTree)Engine.GetMainLoop();
            var scenes = tree.Root.GetNode<SceneManager>("SceneManager");
            foreach (var scene in new[] { "home_livingroom", "prolog_graveyard", "prolog_villa", "town_outskirts", "forest", "Battle" })
            {
                ModAssert.True(scenes.HasScene(scene), "The active game can route " + scene);
            }

            var mods = new List<IModInfo>();
            var registry = ModRegistry.Instance;
            foreach (var id in registry.GetOrderedModIds())
            {
                if (registry.Mods.TryGetValue(id, out var mod) && mod != null) mods.Add(mod);
            }

            ValidatorService.WarmUp();
            var report = new ValidationReport();
            ValidatorService.Analyze(mods, report, context.CancellationToken);
            File.WriteAllText(Path.Combine(context.SandboxDir, "story-validation.json"),
                JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
            var broken = new List<string>();
            foreach (var issue in report.Issues)
            {
                if (issue.Code == "REF_NOT_FOUND" && issue.Mod is "cthangover_game" or "cthangover_murakami")
                    broken.Add(issue.File + ":" + issue.Line + " " + issue.Message);
            }

            ModAssert.Equal(0, broken.Count, string.Join("\n", broken));
            await ForestExits(context);
        }

        private static async Task ForestExits(ModTestContext context)
        {
            var tree = (SceneTree)Engine.GetMainLoop();
            var original = tree.CurrentScene;
            var originalView = original as CanvasItem;
            var originalVisible = originalView?.Visible == true;
            var before = SaveService.BuildPackage("__story_routes_before");
            var scenes = tree.Root.GetNode<SceneManager>("SceneManager");
            originalView?.Hide();
            tree.CurrentScene = null;
            try
            {
                scenes.PendingSceneName = "forest";
                tree.Root.GetNode<GodotSceneService>("GodotSceneService").LoadShell();
                await WaitScene(tree, scenes, "forest", context);
                await Choose(tree, 0, context);
                await WaitScene(tree, scenes, "home_outside", context);
                SceneContextNode.FindNode<DialogBox>("DialogBox")?.Abort();
                scenes.SwitchScene("forest");
                await WaitScene(tree, scenes, "forest", context);
                await Choose(tree, 1, context);
                await WaitScene(tree, scenes, "town_outskirts", context);
                ModAssert.NotNull(BackgroundFactory.Instance.Get("town/town_outskirts"), "the outskirts has its own background");
                GameData.Instance.Runtime.Time.SetTime(16 * 60);
                foreach (var scene in new[] { "town_outskirts", "home_livingroom", "prolog_graveyard", "prolog_villa" })
                {
                    SceneContextNode.FindNode<DialogBox>("DialogBox")?.Abort();
                    scenes.SwitchScene(scene);
                    await WaitScene(tree, scenes, scene, context);
                    if (DisplayServer.GetName() != "headless")
                    {
                        await tree.ToSignal(tree.CreateTimer(.35), SceneTreeTimer.SignalName.Timeout);
                        await tree.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                        ModAssert.True(ViewportCapture.SavePng(tree.Root, Path.Combine(context.SandboxDir, scene + ".png")));
                    }
                }
            }
            finally
            {
                var shell = tree.CurrentScene;
                tree.CurrentScene = original;
                if (GodotObject.IsInstanceValid(shell) && shell != original) shell.Free();
                if (originalView != null) originalView.Visible = originalVisible;
                SaveService.ApplyPackage(before, "__story_routes_before");
                SceneContextNode.Instance?.ClearData();
            }
        }

        private static async Task Choose(SceneTree tree, int index, ModTestContext context)
        {
            for (var frame = 0; frame < 180; frame++)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
                var box = SceneContextNode.FindNode<DialogBox>("DialogBox");
                if (box?.TrySelectVariant(index) == true) return;
            }

            throw new System.InvalidOperationException("The real forest dialog did not offer exit " + index);
        }

        private static async Task WaitScene(SceneTree tree, SceneManager scenes, string scene, ModTestContext context)
        {
            for (var frame = 0; frame < 180; frame++)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                await tree.ToSignal(tree, SceneTree.SignalName.ProcessFrame);
                if (scenes.CurrentSceneName == scene && SceneContextNode.FindNode<DialogBox>("DialogBox") != null) return;
            }

            throw new System.InvalidOperationException("The standard story shell did not reach " + scene);
        }
    }
}
