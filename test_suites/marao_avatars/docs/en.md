# Marao reaction auditions

Opt-in content sample for `avatar_base` + `avatar_layers`, activated by `--itest=marao_avatars`.
The virtual mod `cthangover_game__marao_avatars` supplies `cthangover/marao_test` with ten named
`reaction` presets. Four are full-body images; six are waist-up portraits, fitted without
stretching into a 512x768 canvas and aligned at the bottom. Every PNG has actual alpha.

This is an audition set of complete acting poses, not a modular facial rig. A reaction selects
one complete character layer. Eyes, mouth, clothes and pose cannot be independently combined
in these particular assets. The underlying renderer remains capable of multiple independent layers.

Sources, exact cloud prompts, rejected-request records, story references and the offline preview
are archived outside repositories in `art_sources/cthangover/marao_avatar_test_20261030/`.
Source images are copied byte-for-byte into the test suite. No production dialogue references change.

```csharp
IAvatar avatar = AvatarCatalog.Create("cthangover/marao_test");
avatar.ApplyPresets(new Dictionary<string, string> { ["reaction"] = "deadpan_azif" });
IAvatarRenderer renderer = AvatarRenderers.Create(avatar);
host.AddChild(renderer.View);
// Dispose renderer before destroying host.
```

Presets: neutral, hangover, pants_panic, rag_emperor, deadpan_azif, poetry_reader,
police_panic, suspicious, bad_smell, protective.

Runtime audition and comparison export (windowed, via the workspace Windows job bridge):

```text
godot --path . --rendering-method gl_compatibility -- --frontend --with-mods=cthangover_game --itest=marao_avatars --run-tests --mod=cthangover_game__marao_avatars
```

The two runtime tests apply every preset, verify distinct rendered output and alpha, round-trip
saved selections and export a comparison sheet plus individual normalized PNGs under
`gen/marao_avatar_test_20261030/`. Raw generated images are retained separately.

## Paired Al-Azif update

The suite also supplies `cthangover/al_azif_test`. Both avatars expose `emotion` with
`auto`, `neutral`, `open_mouth`, `eye_roll`, `confused`, `angry_scream`, `fear`, `embarrassed`,
`gloating`, `squeamish`, `disgust`, `talking_a`, `talking_b`. Default `auto` contributes nothing:
Marao keeps the selected story reaction, Al-Azif uses its neutral baseline. An explicit emotion
at priority 20 replaces the complete character image; select auto to return to story reactions.

Al-Azif additionally exposes `view`: auto/front keep emotion control; angled/back/open at
priority 100 select the requested structural view, temporarily overriding the emotional image.
Select view=auto to return to the chosen emotion. No mirroring is applied to book images.
The two talking presets are not an automatically played or pixel-aligned animation.

The four full-body Marao poses now use softer legs and loose white shorts. Both held-book
portraits use the canonical Al-Azif design. Selected source PNGs and the updated offline gallery
live in `art_sources/cthangover/marao_azif_avatar_update_20261030/`. The original generated
versions remain archived in the earlier source batch. Three page-pulling story frames are
standalone illustrations in the updated source batch; they do not replace production scenes.

Additional runtime tests exercise the shared emotion vocabulary, all Al-Azif views, JSON
restore and export `gen/marao_azif_avatar_update_20261030/emotion_comparison.png`.
