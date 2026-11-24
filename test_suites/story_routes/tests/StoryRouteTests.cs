namespace Cthangover.Mods.Game.StoryRoutes
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using Cthangover.Core.Games;
    using Cthangover.Core.Mods;
    using Cthangover.Core.Mods.Testing;
    using Cthangover.Core.Scenes;
    using Cthangover.Validator;
    using Godot;

    /// <summary>Checks references under the selected collection and its actual runtime route registry.</summary>
    public static class StoryRouteTests
    {
        [ModTest("cthangover/story_references", TimeoutSeconds = 180)]
        public static void SelectedCollection(ModTestContext context)
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
        }
    }
}
