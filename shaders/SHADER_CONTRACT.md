# Shader contract for gd_cubism (Live2D)

## Overview

gd_cubism uses 10 shaders to render Live2D models. Each drawable (mesh part)
selects one of these shaders based on its blend mode and mask configuration.
A mod can replace any shader by placing a `.gdshader` file with the same name
in its `shaders/` directory.

## Mandatory contract

### Uniforms — DO NOT remove or rename

Every shader below MUST declare the uniforms listed in its section. The C++ renderer
sets these uniforms by name via `ShaderMaterial::set_shader_parameter()`.
Removing or renaming a uniform will break rendering.

You MAY add new uniforms (e.g. `uniform float pixel_size = 4.0;`), but you
must set them manually in code via `model.GetMeshes()`.

### Vertex shader

All shaders MUST include `UV.y = 1.0 - UV.y;` in the vertex function.
This flips the texture vertically to match Live2D's coordinate system.

### blend_mode must match

The `render_mode` line must remain compatible with the blend operation
the C++ code expects. Changing `blend_premul_alpha` to `blend_mix`
will produce incorrect transparency.

---

## Shader catalog

### 1. `2d_cubism_norm_mix.gdshader` — Normal blend (most drawables)

render_mode: `blend_premul_alpha, unshaded`

Mandatory uniforms:
```
uniform vec4 color_base : source_color;
uniform vec4 color_screen : source_color;
uniform vec4 color_multiply : source_color;
uniform vec4 channel : source_color;
uniform sampler2D tex_main : filter_linear_mipmap;
```

Varying: `varying vec4 modulate` (passed from vertex: `modulate = COLOR`)

Fragment output: `COLOR = vec4(color.rgb * color.a, color.a);`

### 2. `2d_cubism_norm_add.gdshader` — Additive blend

Same uniforms and contract as norm_mix. Fragment output:
`COLOR = vec4(color.rgb * color.a, 0.0);` (alpha=0 for additive)

### 3. `2d_cubism_norm_mul.gdshader` — Multiply blend

Same uniforms and contract as norm_mix.

### 4. `2d_cubism_mask.gdshader` — Mask generation (offscreen)

render_mode: `blend_mix, unshaded`

Mandatory uniforms:
```
uniform vec4 channel : source_color;
uniform sampler2D tex_main : filter_linear_mipmap;
```

Fragment output: `COLOR = channel * texture(tex_main, UV).a;`

This shader writes alpha to the mask texture. Not visible on screen.

### 5–10. Masked drawable shaders

`2d_cubism_mask_mix`, `mask_add`, `mask_mul`, `mask_mix_inv`, `mask_add_inv`, `mask_mul_inv`

render_mode: same as their non-mask counterpart

Mandatory uniforms (all of norm_mix PLUS):
```
uniform sampler2D tex_mask : filter_linear_mipmap;
```

Additional varyings (computed in vertex):
```
varying vec2 MASK_UV;
```

Mask UV computation in vertex:
```glsl
MASK_UV = (VERTEX - mesh_offset) / mask_size;
// where mask_size = textureSize(tex_mask, 0) / mask_scale
```

Fragment must sample `tex_mask` and multiply/lerp the main color by
the sum of mask channels: `clip_mask.r + clip_mask.g + clip_mask.b + clip_mask.a`.
The `_inv` variants invert this: `(1.0 - mask_val)`.

---

## How to add custom logic

**Safe additions** (will not break rendering):
- Add new `uniform` declarations (set values via `ShaderMaterial.SetShaderParameter`)
- Modify internal fragment calculations (e.g. pixel-art downscale)
- Add conditional logic (e.g. `if (pixel_size > 1.0) { ... }`)

**Allowed modifications:**
- Change texture filtering: `filter_linear_mipmap` → `filter_nearest`
- Add post-processing in fragment (color grading, dithering, outline)
- Add noise/procedural effects using new uniforms

**Forbidden modifications:**
- Remove or rename mandatory uniforms
- Change `render_mode` blend type
- Remove `UV.y = 1.0 - UV.y` from vertex
- Change the data type of mandatory uniforms (vec4 → vec3, etc.)

---

## Mod override example

To create a pixel-art Live2D shader:

1. Copy `2d_cubism_norm_mix.gdshader` to `mods/your_mod/shaders/`
2. Add a new uniform: `uniform float pixel_size : hint_range(1.0, 16.0) = 4.0;`
3. Modify fragment to downsample:
```glsl
vec2 tex_size = vec2(textureSize(tex_main, 0));
vec2 snapped_uv = floor(UV * tex_size / pixel_size) * pixel_size / tex_size;
vec4 color_tex = texture(tex_main, snapped_uv);
```
4. Keep all mandatory uniforms and the vertex shader intact.
5. In code, find the material and set `pixel_size`:
```csharp
var meshes = model.GetMeshes();
foreach (var kv in meshes) {
    var mesh = (ArrayMesh)kv.Value;
    var mat = (ShaderMaterial)mesh.SurfaceGetMaterial(0);
    mat.SetShaderParameter("pixel_size", 4.0f);
}
```
