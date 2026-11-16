using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Cthangover.Core.Mods.Resolvers;
using Cthangover.Core.Mods.Testing;
using Cthangover.Core.Utils;
using Cthangover.Mods.Avatars;
using Godot;

namespace Cthangover.Mods.MaraoAvatars.Tests
{
    /// <summary>Auditions the actual story PNGs through preset state and the installed layered renderer.</summary>
    public sealed class MaraoAvatarTests
    {
        private const string AvatarId = "cthangover/marao_test";
        private const string Output = "res://gen/marao_avatar_test_20261030/";

        private static string OutputFile(string name)
        {
            // The picture folder may be missing on a fresh checkout (the host has no gen/ subfolder).
            var dir = ProjectSettings.GlobalizePath(Output);
            DirAccess.MakeDirRecursiveAbsolute(dir);
            return dir + name;
        }

        [ModTest("marao_avatars/all_ten_reactions_render_and_restore", TimeoutSeconds = 45)]
        public static async Task Reactions(ModTestContext ctx)
        {
            ModAssert.True(GameEnvironment.IsRuntime, "visual audition requires the Godot runtime");
            var tree = (SceneTree)Godot.Engine.GetMainLoop();
            var viewport = new SubViewport { Size = new Vector2I(512, 768), TransparentBg = true,
                Disable3D = true, RenderTargetUpdateMode = SubViewport.UpdateMode.Always };
            var avatar = AvatarCatalog.Create(AvatarId);
            var independent = AvatarCatalog.Create(AvatarId);
            var definition = Entities.Collect<AvatarDefinition>()[AvatarId];
            ModAssert.Equal(10, definition.Groups["reaction"].Presets.Count, "all requested reactions installed");
            using var renderer = AvatarRenderers.Create(avatar);
            var hashes = new HashSet<string>(StringComparer.Ordinal);
            try
            {
                tree.Root.AddChild(viewport);
                viewport.AddChild(renderer.View);
                foreach (var preset in definition.Groups["reaction"].Presets.Keys)
                {
                    avatar.ApplyPresets(new Dictionary<string, string> { ["reaction"] = preset });
                    renderer.Apply(avatar.Resolve());
                    await tree.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                    using var image = viewport.GetTexture().GetImage();
                    var visible = 0; var transparent = 0;
                    for (var y = 0; y < image.GetHeight(); y += 16)
                        for (var x = 0; x < image.GetWidth(); x += 16)
                        {
                            var alpha = image.GetPixel(x, y).A;
                            if (alpha > 0.9f) visible++;
                            if (alpha < 0.01f) transparent++;
                        }
                    ModAssert.True(visible > 100, preset + ": character has visible pixels");
                    ModAssert.True(transparent > 100, preset + ": transparent surroundings preserved");
                    ModAssert.True(hashes.Add(Convert.ToHexString(SHA256.HashData(image.GetData()))),
                        preset + ": reaction produces a distinct rendered image");
                    ModAssert.True(image.SavePng(OutputFile(preset + ".render.png")) == Error.Ok,
                        preset + ": rendered audition exported");
                    var saved = JsonSerializer.Deserialize<AvatarState>(JsonSerializer.Serialize(avatar.CaptureState()));
                    var restored = AvatarCatalog.Create(AvatarId, saved);
                    ModAssert.Equal(preset, restored.CaptureState().Presets["reaction"], "JSON save restores selection");
                }
                ModAssert.Equal("neutral", independent.CaptureState().Presets["reaction"], "another instance stays unchanged");
            }
            finally
            {
                renderer.Dispose(); viewport.GetParent()?.RemoveChild(viewport); viewport.Free();
            }
        }

        [ModTest("marao_avatars/comparison_sheet", TimeoutSeconds = 45)]
        public static async Task Comparison(ModTestContext ctx)
        {
            ModAssert.True(GameEnvironment.IsRuntime, "comparison sheet requires the Godot runtime");
            var tree = (SceneTree)Godot.Engine.GetMainLoop();
            var viewport = new SubViewport { Size = new Vector2I(1440, 960), TransparentBg = false,
                Disable3D = true, RenderTargetUpdateMode = SubViewport.UpdateMode.Always };
            var renderers = new List<IAvatarRenderer>();
            try
            {
                tree.Root.AddChild(viewport);
                viewport.AddChild(new ColorRect { Color = new Color("e4ded4"), Size = new Vector2(1440, 960) });
                var index = 0;
                foreach (var preset in Entities.Collect<AvatarDefinition>()[AvatarId].Groups["reaction"].Presets.Keys)
                {
                    var x = (index % 5) * 288; var y = (index / 5) * 480;
                    viewport.AddChild(new ColorRect { Color = new Color(index % 2 == 0 ? "e4ded4" : "c5c6c5"),
                        Position = new Vector2(x, y), Size = new Vector2(288, 480) });
                    var avatar = AvatarCatalog.Create(AvatarId);
                    avatar.ApplyPresets(new Dictionary<string, string> { ["reaction"] = preset });
                    var renderer = AvatarRenderers.Create(avatar); renderers.Add(renderer);
                    viewport.AddChild(renderer.View);
                    renderer.View.Scale = new Vector2(0.5f, 0.5f);
                    renderer.View.Position = new Vector2(x + 16, y + 24);
                    var label = new Label { Text = (index + 1) + ". " + preset, Position = new Vector2(x + 12, y + 426),
                        Size = new Vector2(264, 36), HorizontalAlignment = HorizontalAlignment.Center };
                    label.AddThemeColorOverride("font_color", new Color("24232b"));
                    label.AddThemeFontSizeOverride("font_size", 18);
                    viewport.AddChild(label);
                    index++;
                }
                await tree.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                using var sheet = viewport.GetTexture().GetImage();
                ModAssert.True(sheet.SavePng(OutputFile("comparison.png")) == Error.Ok,
                    "comparison sheet exported from actual avatar renderer");
            }
            finally
            {
                foreach (var renderer in renderers) renderer.Dispose();
                viewport.GetParent()?.RemoveChild(viewport); viewport.Free();
            }
        }
    }
}
