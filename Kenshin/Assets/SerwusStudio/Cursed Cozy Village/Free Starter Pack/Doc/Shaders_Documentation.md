# Cursed Cozy Village — Free Shaders

**Serwus Studio**
Stylized cel-shaded shaders for the Free Starter Pack.

---

## 1. Overview

This free pack ships **two self-contained shaders** built for the
Universal Render Pipeline. Both use a soft **cel / toon** lighting model so your
scene gets a consistent stylized look out of the box.

| Shader | Menu path | Use for |
|---|---|---|
| **Wall (Free)** | `Serwus Studio/Cursed Cozy Alley/Wall (Free)` | Brick + plaster walls where plaster crumbles away to reveal brick |
| **Prop (Free)** | `Serwus Studio/Cursed Cozy Alley/Prop (Free)` | Any general prop (barrels, crates, lanterns, pots, planks…) |

Both shaders are **standalone** — they have no external file dependencies, so you
can move/export them freely.

---

## 2. Requirements

| | |
|---|---|
| **Unity** | 2022.3 LTS or 6000.x (Unity 6) — tested on 2022.3.62 and 6000.3 |
| **Render Pipeline** | Universal RP — URP 14.x (2022.3) or 17.x (Unity 6), **Forward** or **Forward+** |
| **Color space** | Linear (recommended) |

> If a material appears **magenta/pink**, the project is not using URP, or the
> URP version differs. See *Troubleshooting* below.

---

## 3. Quick Start

1. Create a Material (`Assets ▸ Create ▸ Material`).
2. In the material's **Shader** dropdown, pick
   `Serwus Studio/Cursed Cozy Alley/Wall (Free)` or `…/Prop (Free)`.
3. Assign your textures (see each shader below).
4. Drag the material onto your mesh.

That's it — lighting, shadows and fog all work automatically.

---

## 4. Wall (Free)

A two-layer wall: a **brick** base with a **plaster** layer on top. The plaster
can crumble away (globally and/or with a painted mask) to expose the brick.

### 4.1 Textures

Each layer (Brick and Plaster) takes three maps:

| Slot | Content |
|---|---|
| **Albedo** | Base color texture |
| **Normal** | Normal map (tangent space) |
| **Mask** | Packed map — **R = Ambient Occlusion**, **B = Height** *(G reserved)* |

> **Height (Mask B)** matters: the plaster crumbles from its *thinnest* spots
> first, so a good height map gives natural, irregular damage edges. A flat
> (white) mask still works — damage is then driven purely by the slider/mask.

### 4.2 Damage Mask convention

The **Damage Mask** is an optional painted texture controlling *where* plaster
survives:

- **White (R = 1)** → intact plaster
- **Black (R = 0)** → exposed brick

Unassigned = white = fully plastered. Paint black where you want brick to show.

### 4.3 Properties

**Brick / Plaster (per layer)**

| Property | Description |
|---|---|
| Albedo / Normal / Mask | The three maps above |
| Tint | Multiplies the albedo |
| Normal Strength | Bump intensity (0 = flat) |
| AO Strength | How strongly the Mask's AO darkens the surface |

**Plaster Damage (reveal brick)**

| Property | Description |
|---|---|
| Damage Amount (global) | 0 = full plaster, 1 = full brick |
| Damage Mask | Painted mask (white = plaster, black = brick) |
| Painted Mask Influence | How strongly the painted mask drives the reveal |
| Crumble Edge Softness | Soft ↔ sharp transition between plaster and brick |
| Procedural Crumble Noise | Toggle natural noisy edges |
| Crumble Noise Scale / Strength | Size and intensity of the noisy edge |
| Exposed Brick Recess | Darkens the brick in the gaps (fake depth) |
| Crumble Rim Darkening | Dark line right at the crumble boundary |

**Cel Shading** — see *Section 6*.

**Rendering**

| Property | Description |
|---|---|
| Cull | Back (default) / Front / Off (double-sided) |

---

## 5. Prop (Free)

A general stylized prop shader with optional features you toggle per material.

### 5.1 Properties

**Surface**

| Property | Description |
|---|---|
| Albedo / Tint | Base color texture and tint |
| Use Normal Map | Toggle the normal map on/off |
| Normal Map / Strength | Tangent-space normal map and intensity |
| Mask (R:AO) | Packed mask — **R = Ambient Occlusion** *(G,B reserved)* |
| AO Strength | How strongly AO darkens the surface |

**Emission** (for lamps, windows, embers)

| Property | Description |
|---|---|
| Enable Emission | Toggle the emissive contribution |
| Emission Map | Emissive texture (mask or full color) |
| Emission Color | **HDR** color — push intensity above 1 to glow with Bloom |
| Emission Strength | Multiplier on the emission |

**Alpha Cutout** (for foliage, rope, openwork metal)

| Property | Description |
|---|---|
| Enable Alpha Cutout | Toggle hard-edged transparency (uses Albedo alpha) |
| Cutoff | Alpha threshold — pixels below this are clipped |

**Cel Shading** — see *Section 6*. **Rendering** — Cull mode as above.

> **Glass / true transparency** is intentionally *not* in this shader — use a
> dedicated transparent material for glass.

---

## 6. Cel Shading (both shaders)

The stylized look is controlled by four shared parameters:

| Property | Description |
|---|---|
| **Cel Strength** | Blends between a smooth gradient (**0**) and a crisp toon step (**1+**). Higher = harder, more visible band. *(On Prop this is an unclamped value — push past 1 for a stronger effect.)* |
| **Shadow Tint** | Color of the shaded side (cool tints read best) |
| **Shade Threshold** | Where the light/shadow boundary sits (lower = more lit) |
| **Shade Softness** | Width of the transition — lower = sharper edge |

**Recipe for a stronger cartoon edge:** raise *Cel Strength* (e.g. 1.5–2) **and**
lower *Shade Softness* (e.g. 0.1). For the subtle "almost realistic" look, keep
*Cel Strength* low (~0.3) with a wider *Shade Softness*.

---

## 7. Performance Notes

- Optional features (Normal Map, Emission, Alpha Cutout) are compiled as
  **shader keywords** — disabled features cost nothing.
- Both shaders are **SRP Batcher** compatible and support **GPU instancing**.
- All four render passes are included (Forward, Shadow, Depth, Depth Normals),
  so shadows, depth-based effects and SSAO work correctly.

---

## 8. Troubleshooting

| Symptom | Cause / Fix |
|---|---|
| Material is **magenta/pink** | Project isn't on URP, or URP version mismatch. Confirm URP 14 (Unity 2022.3) or URP 17 (Unity 6) and that a URP asset is set in *Project Settings ▸ Graphics*. |
| Brick shows through plaster at Damage = 0 | The plaster **Albedo** slot is empty (white default). Assign the plaster textures. |
| Wall looks uniformly pale | Same as above — plaster maps not assigned yet. |
| Cutout prop has hard rectangular shadows | Enable **Alpha Cutout** so the shadow/depth passes also clip. |
| Cel banding too strong/weak | Tune **Cel Strength** + **Shade Softness** (Section 6). |
| Emission doesn't glow | Emission only *adds color*; the glow halo comes from a **Bloom** post-process. Use an HDR Emission Color > 1. |

---

## 9. Upgrade — Premium Wall

The premium **Wall (Brick + Plaster)** shader extends the free wall with:

- **Dirt** that pools in cavities and streaks downward
- **Moss / Damp** creeping up from the base (with its own texture)
- **Edge Wear** on raised plaster
- **Parallax-Occlusion** depth in the mortar gaps
- **Triplanar** mapping (no UV stretch on raw geometry)

Property names match the free wall, so a material built on **Wall (Free)**
upgrades by simply switching the shader.

---

*© Serwus Studio. Shaders may be used in your projects per the Unity Asset Store
EULA. Thanks for trying the pack!*
