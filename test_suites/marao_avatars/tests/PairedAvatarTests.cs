using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Cthangover.Core.Mods.Resolvers;
using Cthangover.Core.Mods.Testing;
using Cthangover.Core.Utils;
using Cthangover.Mods.Avatars;
using Godot;

namespace Cthangover.Mods.MaraoAvatars.Tests
{
    /// <summary>Both content avatars share the same named emotions while retaining independent view/pose state.</summary>
    public sealed class PairedAvatarTests
    {
        private const string Marao = "cthangover/marao_test";
        private const string Book = "cthangover/al_azif_test";
        private const string Output = "res://gen/marao_azif_avatar_update_20261030/";

        private static string OutputFile(string name)
        {
            // The picture folder may be missing on a fresh checkout (the host has no gen/ subfolder).
            var dir = ProjectSettings.GlobalizePath(Output);
            DirAccess.MakeDirRecursiveAbsolute(dir);
            return dir + name;
        }

        [ModTest("paired_avatars/common_emotions_render_and_restore", TimeoutSeconds = 60)]
        public static async Task CommonEmotions(ModTestContext ctx)
        {
            ModAssert.True(GameEnvironment.IsRuntime);
            var definitions = Entities.Collect<AvatarDefinition>();
            var states = definitions[Marao].Groups["emotion"].Presets.Keys.OrderBy(Identity, StringComparer.Ordinal).ToArray();
            var bookStates = definitions[Book].Groups["emotion"].Presets.Keys.OrderBy(Identity, StringComparer.Ordinal).ToArray();
            ModAssert.Equal(13, states.Length, "twelve shared states plus auto");
            ModAssert.Equal(string.Join(",", states), string.Join(",", bookStates), "same public emotion names");
            var tree = (SceneTree)Engine.GetMainLoop();
            var viewport = new SubViewport { Size = new Vector2I(512, 768), TransparentBg = true,
                Disable3D = true, RenderTargetUpdateMode = SubViewport.UpdateMode.Always };
            try
            {
                tree.Root.AddChild(viewport);
                foreach (var id in new[] { Marao, Book })
                {
                    var avatar = AvatarCatalog.Create(id);
                    using var renderer = AvatarRenderers.Create(avatar);
                    viewport.AddChild(renderer.View);
                    foreach (var state in states)
                    {
                        avatar.ApplyPresets(new Dictionary<string, string> { ["emotion"] = state });
                        renderer.Apply(avatar.Resolve());
                        await tree.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                        using var image = viewport.GetTexture().GetImage();
                        var visible = 0; var transparent = 0;
                        for (var y = 0; y < 768; y += 24)
                            for (var x = 0; x < 512; x += 24)
                            {
                                var alpha = image.GetPixel(x, y).A;
                                if (alpha > 0.9f) visible++;
                                if (alpha < 0.01f) transparent++;
                            }
                        ModAssert.True(visible > 40 && transparent > 40, id + "/" + state + ": rendered with alpha");
                        var saved = JsonSerializer.Deserialize<AvatarState>(JsonSerializer.Serialize(avatar.CaptureState()));
                        var restored = AvatarCatalog.Create(id, saved);
                        ModAssert.Equal(state, restored.CaptureState().Presets["emotion"]);
                    }
                }
                var book = AvatarCatalog.Create(Book);
                foreach (var view in new[] { "front", "angled", "back", "open" })
                {
                    book.ApplyPresets(new Dictionary<string, string> { ["view"] = view, ["emotion"] = "angry_scream" });
                    using var renderer = AvatarRenderers.Create(book);
                    viewport.AddChild(renderer.View);
                    await tree.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                    using var image = viewport.GetTexture().GetImage();
                    ModAssert.True(image.SavePng(OutputFile("al_azif_" + view + ".render.png")) == Error.Ok);
                    var state = JsonSerializer.Deserialize<AvatarState>(JsonSerializer.Serialize(book.CaptureState()));
                    ModAssert.Equal(view, AvatarCatalog.Create(Book, state).CaptureState().Presets["view"]);
                }
            }
            finally { viewport.GetParent()?.RemoveChild(viewport); viewport.Free(); }
        }

        [ModTest("paired_avatars/emotion_comparison_sheet", TimeoutSeconds = 60)]
        public static async Task Comparison(ModTestContext ctx)
        {
            ModAssert.True(GameEnvironment.IsRuntime);
            var tree = (SceneTree)Engine.GetMainLoop();
            var viewport = new SubViewport { Size = new Vector2I(1920, 1920), TransparentBg = false,
                Disable3D = true, RenderTargetUpdateMode = SubViewport.UpdateMode.Always };
            var renderers = new List<IAvatarRenderer>();
            try
            {
                tree.Root.AddChild(viewport);
                viewport.AddChild(new ColorRect { Size = new Vector2(1920, 1920), Color = new Color("e4ded4") });
                var index = 0;
                foreach (var emotion in Entities.Collect<AvatarDefinition>()[Marao].Groups["emotion"].Presets.Keys)
                {
                    if (emotion == "auto") continue;
                    var x = (index % 3) * 640; var y = (index / 3) * 480;
                    viewport.AddChild(new ColorRect { Position = new Vector2(x, y), Size = new Vector2(640, 480),
                        Color = new Color(index % 2 == 0 ? "e4ded4" : "c6c7c5") });
                    var side = 0;
                    foreach (var id in new[] { Marao, Book })
                    {
                        var avatar = AvatarCatalog.Create(id);
                        avatar.ApplyPresets(new Dictionary<string, string> { ["emotion"] = emotion });
                        var renderer = AvatarRenderers.Create(avatar); renderers.Add(renderer);
                        viewport.AddChild(renderer.View);
                        renderer.View.Scale = new Vector2(0.5f, 0.5f);
                        renderer.View.Position = new Vector2(x + 24 + side * 320, y + 24);
                        side++;
                    }
                    var label = new Label { Text = emotion, Position = new Vector2(x + 12, y + 428),
                        Size = new Vector2(616, 36), HorizontalAlignment = HorizontalAlignment.Center };
                    label.AddThemeColorOverride("font_color", new Color("24232b"));
                    label.AddThemeFontSizeOverride("font_size", 22);
                    viewport.AddChild(label); index++;
                }
                await tree.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                using var image = viewport.GetTexture().GetImage();
                ModAssert.True(image.SavePng(OutputFile("emotion_comparison.png")) == Error.Ok);
            }
            finally
            {
                foreach (var renderer in renderers) renderer.Dispose();
                viewport.GetParent()?.RemoveChild(viewport); viewport.Free();
            }
        }

        private static string Identity(string value) => value;
    }
}
