# Cursed Cozy Village — Free Starter Pack

A stylized medieval village kit for Unity 2022.3 LTS and Unity 6, built for the
Universal Render Pipeline.
Barrels, benches, lanterns, walls, roofs, a well — plus two hand-written URP
shaders that keep large surfaces from tiling visibly.

This is the free entry point to the **Cursed Cozy Village** series. It is a
complete, standalone pack: nothing here requires another package.

---

## Two things that help more than you'd think

This pack is free and still getting updates. If it is useful to you:

- **★ Rate it** → https://assetstore.unity.com/packages/3d/environments/cursed-cozy-village-free-pack-382598#reviews
  A rating is the single biggest factor in whether anyone else ever finds this
  pack. Honest ones — including what you did not like — are the useful ones.
- **Join the Discord** → https://discord.gg/Cu2HF9xyVm
  New packs get announced there first, bug reports get answered there fastest,
  and it is where I ask what to model next.

---

## Requirements

| | |
|---|---|
| Unity | 2022.3 LTS or newer, including Unity 6 |
| Render pipeline | Universal Render Pipeline — URP 14.x on Unity 2022.3, URP 17.x on Unity 6 |
| Rendering paths | Forward and Forward+ |
| Shader Graph | Not required — all shaders are hand-written HLSL |
| Other packages | None |

Built-in and HDRP are not supported.

---

## What's inside

**23 models · 23 prefabs · 2 shaders · 2 demo scenes**

| Category | Contents |
|---|---|
| Architecture | Wall, stone wall, window, door, full roof, half roof, wall-roof piece, ground plate |
| Props | Barrel, bench, ceramic pot, mailbox on a wooden post, stone well, curb segments, iron fence segments (2 variants), wooden beam / support structures (3 variants) |
| Lighting | Street lantern, hanging lantern — both with emissive maps |
| Signage | Wooden signpost and a separate paper sheet, so you can pin your own poster or notice to any board |
| Shaders | `CCA_WallLite`, `CCA_PropLite` |
| Textures | Full BaseMap / MaskMap / Normal sets, plus emissive for the lanterns |
| Demo | `Demo.unity` — a dressed street scene. `Demo_Inter.unity` — an interactive browser: step through all 23 prefabs on a turntable and drag the wall shader sliders live |

Every prefab is ready to drag into a scene — no setup step, no manager object, no
scripts. The only scripts in the package drive the interactive demo and live in
`Demo/Scripts/`; delete that folder and every prefab, material and shader still
works exactly the same.

---

## Quick start

1. Import the package.
2. Open `Demo/Demo.unity` to see everything assembled, or `Demo/Demo_Inter.unity`
   and press Play to click through every prefab and play with the wall shader.
3. Drag prefabs from `Prefabs/` into your own scene and build from there.

The welcome window that opens after import can be reopened at any time from
*Tools ▸ Serwus Studio ▸ Cursed Cozy Village Free ▸ Welcome*.

That's the whole workflow. If the demo scene looks correct, your project is
configured correctly.

**If materials appear pink:** your project is not using URP, or is using a URP
version older than 14 (Unity 2022.3). Check *Project Settings → Graphics → Render Pipeline Asset*.

---

## The shaders

Both shaders are hand-written HLSL. The source is readable and commented — open
it, change it, learn from it.

**`CCA_WallLite`** — for walls and large surfaces. Brick and plaster sit as two
stacked layers, each with its own albedo, normal and mask (R: AO, G: roughness,
B: height). A single **Damage Amount** slider crumbles the plaster away to expose
the brick underneath. Procedural crumble noise breaks up the edge so a long wall
run never shows an obvious repeat, and exposed brick is recessed with darkened
rims so the break reads as depth rather than a decal. If you want control over
*where* it breaks, paint a damage mask and blend it in with **Painted Mask
Influence**.

No damage textures to author — the wear is generated in the shader, so it costs
no additional texture memory and you tune it live in the Inspector.

**`CCA_PropLite`** — for props. Albedo with tint, optional normal map, mask-map
driven ambient occlusion, optional alpha cutout, and optional HDR emission for
the lanterns.

Both shaders share the same cel shading block — strength, shadow tint, threshold
and softness — so every asset in the pack reads as one consistent look, and you
can push it from nearly-PBR to hard-edged toon with one value.

Full property reference: **`Doc/Shaders_Documentation.md`**

---

## Third-party content and licensing

The demo scene uses the **"night no moon" skybox by Jettelly**, released under
**CC0**. Its `License.txt` is included alongside it. Everything else in this
package is original work by Serwus Studio.

You may use this pack in commercial and non-commercial projects under the Unity
Asset Store EULA. You may not redistribute or resell the assets as-is, or as
part of another asset pack.

---

## More from Cursed Cozy Village

Every pack in the series is standalone — install only what you need. They share
one art direction, so they drop into the same scene without looking mismatched.

**Cursed Cozy Alley — Market Shopfront Kit**
A dressed shop window, service counter, shelving and 12 hand-painted market
props, plus three runtime systems: procedural cloth tearing that costs zero
extra texture memory, a sign system that takes any PNG you drop on it, and one
wind manager driving every banner and sign in the scene.
→ https://assetstore.unity.com/packages/package/390220

**Cursed Cozy Alchemist's Corner — Liquid Shader & Potions**
Potions that behave like liquid: tilt a bottle and the surface stays level,
shake it and it sloshes, tip it past the lip and it pours into the next one,
mixing colour and glow by volume. One material, hand-written HLSL.
→ https://assetstore.unity.com/packages/package/390222

More packs in the series are in development. The signposts in the demo scene
show what is coming.

---

## Support

Questions, bug reports, or a feature you need for your project — write to me
directly, I answer.

- **Email:** dustin.smela@gmail.com
- **Documentation:** https://serwusgamestudio.pl
- **Publisher page:** https://assetstore.unity.com/publishers/140496

- **Discord:** https://discord.gg/Cu2HF9xyVm

If this pack saved you time, **rate it → https://assetstore.unity.com/packages/3d/environments/cursed-cozy-village-free-pack-382598#reviews**.
A review genuinely helps a small publisher get seen, and honest ones — including
the critical parts — are the most useful.

---

## Changelog

**1.3.0**
- Now supports Unity 2022.3 LTS with URP 14, in addition to Unity 6 with URP 17.
  Same content, same shaders — the Asset Store hands you the right build for your
  editor version
- Both shaders pick up additional lights in Forward+ on URP 14 as well
- New interactive demo scene, `Demo/Demo_Inter.unity`. Press Play and step through
  all 23 prefabs one at a time on a turntable, each scaled to fill the frame and
  labelled with its triangle count
- The same scene puts the shaders on live sliders while the model spins — plaster
  damage, brick and plaster tint, crumble edge, cel shading. The panel is built
  from whatever shader the current model uses, so props get their own controls
- Nothing you change in the demo is written to disk. The sliders drive per-instance
  material copies, so the `.mat` files in your project stay untouched
- New welcome window on first import, with a button that opens the interactive
  demo. Shows once per version, closes with Escape, and has a "Don't show again"
  button. Reopen it from *Tools ▸ Serwus Studio ▸ Cursed Cozy Village Free ▸ Welcome*
- The demo is the only part of this package with scripts. They live in
  `Demo/Scripts/` — delete the `Demo` folder and every prefab, material and shader
  still works exactly the same
- Documentation rewritten: the README now opens with the links that matter and
  covers both demo scenes
- Added a Discord server for support, bug reports and pack announcements
- Removed an unused material (`MI_wall 1`) that referenced a shader from outside
  this package

**1.1.0**
- Added a wooden signpost and a separate paper sheet, so you can pin your own
  poster, notice or logo onto any board
- Demo scene updated to show the signposts in place
- Added this README
- Consistent English naming across all prefabs, materials and textures
- Removed duplicate materials
- Demo textures converted from BMP to PNG — smaller download

Existing projects are unaffected: assets were renamed inside Unity, so GUIDs are
preserved and anything already placed in a scene keeps working.

**1.0** — Initial release. Models, prefabs, `CCA_WallLite` and `CCA_PropLite`
shaders, demo scene.

---

*Serwus Studio — Kraków, Poland*
