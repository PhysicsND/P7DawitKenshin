# Free Starter Pack — Zawartość paczki (checklista wydawnicza)

> Dokument roboczy Serwus Studio (2026-07-06). Cel: **darmowa** paczka-wizytówka
> serii Cursed Cozy Village na Unity Asset Store + Fab (lejek do paczek premium).
> Wymagania: Unity 6000.3+, URP 17.

---

## 1. Zawartość (KOMPLETNA w ~90% ✅)

- **Shadery**: `CCA_WallLite.shader` (Wall Free — brick+plaster+damage),
  `CCA_PropLite.shader` (Prop Free) — standalone, bez include'ów ✅
- **Modele**: 22 FBX (beczka, ławka, drzwi, latarnie, skrzynie, płot/deski,
  dachy, ściany, studnia, skrzynka pocztowa, doniczka, krawężnik…)
- **Prefaby**: 21 (komplet do złożenia małej uliczki)
- **Materiały**: 16 MI_*
- **Tekstury**: pełne sety BaseMap/MaskMap/Normal (+ Emissive dla latarni)
- **Demo**: `Demo.unity` + terrain + skybox CC0
- **Doc**: `Shaders_Documentation.md`

## 2. PORZĄDKI przed publikacją 🟠

- [ ] Literówki i nazwy PL → EN: `Irionbound` → `Ironbound`, `kraweznik` → `Curb`,
  `studnia` → `Well`, `MI_Kraweznik`, `MI_studnia`, `props_ glass.PNG` (spacja),
  `propos texture_*` → `props_texture_*` (UWAGA: zmiana nazw FBX/tekstur nie psuje
  GUID-ów, ale robić w Unity, nie w Explorerze!)
- [ ] Usunąć duplikaty: `MI_Lantern 1.mat`, `floor_demo 1.mat`, `New Material.mat`,
  `New Terrain 1.asset`, `skyubox.mat`
- [ ] Brak README paczki (jest tylko doc shaderów) — dodać krótki README:
  co jest w paczce + linki do paczek premium serii (cross-sell!)
- [ ] Dokumentacja PDF
- [ ] Prefaby: sprawdzić kolizje (czy każdy prop ma collider) i LOD-y (opcjonalnie)
- [ ] Screenshoty / key image — pokazać spójność z paczkami premium

## 3. Zawartość finalnego .unitypackage

```
Free Starter Pack/
  Doc/          README + Shaders_Documentation + PDF
  Demo/         Demo.unity + terrain + skybox (CC0)
  Materials/    po deduplikacji
  Models/       22 FBX
  Prefabs/      21 prefabów
  Shaders/      CCA_WallLite + CCA_PropLite (standalone!)
  Texture/      sety tekstur (docelowo: "Textures" — ujednolicić nazwę folderu)
```

## 4. Zależności / trzecie strony

- Skybox `night no moon` (Jettelly) — **CC0** ✅, zostawić License.txt, zgłosić przy submicie.
- Tylko URP, brak skryptów runtime — najprostsza paczka do review. ✅
- Rola w bundlu: darmowy teaser; w opisie linkować Alley / Vegetation / Gothic /
  Graveyard / Alchemist.
