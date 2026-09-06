# Verification record

Target: **v1.4.8.119303**, Steam build **24573425**, editor **v1.4.8**. See `target-game.json` and [implementation evidence](IMPLEMENTATION.md).

## Completed checks

- Compiled client and editor DLLs against the updated installed assemblies with .NET SDK 9.0.100: zero warnings/errors.
- Validated all five registered XMLs against installed TaleWorlds XSDs.
- Checked module registration, exact dependency ID casing, distinct item ID, native mesh references, template/piece relationships and supported configuration ranges.
- Generated original four-part FBX exports, LODs, UVs, packed Blender source and three PBR PNG textures; rendered and visually inspected the source sword.
- Reimported all four FBXs in Blender and checked dimensions, names, LODs, pivots, materials, UVs and nondegenerate faces.
- Built a ZIP containing only mod DLL/XML and installation manifest; no proprietary assemblies required in CI.
- Exercised installation/removal in an isolated fixture; existing-folder refusal, edited-file preservation, unrelated-file preservation, traversal rejection and invalid stats/missing-piece cases passed.
- Installed the native-visual prototype into only `Modules/Dragonslayer` and saw its registration in the Modding Kit launcher. Corrected the `Sandbox` dependency ID after local inspection.
- Launched the matching editor with Dragonslayer enabled, reached its main menu and Edit Mode. The engine log confirmed successful assembly loading; Dragonslayer's log confirmed its load callback. Fixed the initial missing editor DLL and retested startup.

## Pending runtime verification

During the first editor session, exiting the empty default scene triggered a native assertion at `rglIntrusive_ptr.h:151`, `px != nullptr`, after scene finalization. The process was stopped and restarted. The trace does not establish a mod-specific cause; successful assembly loading is not evidence that editor shutdown or gameplay passed.

The game has **not** been played to verify this mod. Launcher visibility is not a gameplay test. Custom assets have **not** been imported/published. The prototype is an installable development package, not a claim that the requested custom sword is finished.

## In-game checklist

Use a new/separate single-player test save and only official modules plus Dragonslayer initially.

- [ ] Startup: launcher shows Dragonslayer, no dependency/XML/assembly error, main menu and campaign load.
- [ ] Obtain: `dragonslayer.give` adds one item per call; wrong contexts/arguments produce useful messages. `dragonslayer.status` and log agree.
- [ ] Inventory: correct name, weight/reach, 180 cutting / 65 piercing damage and 58/55 speeds / 45 handling. Check tooltip and thumbnail.
- [ ] Equip: two-handed weapon slot, normal grip and draw/sheath; no one-handed or shield alternative.
- [ ] Combat: all swing directions, thrust, blocks, hit reactions and slower handling; no global changes to a native control sword.
- [ ] Reach/damage: compare against a native two-hander at controlled distances and against the same armour. Check visible blade vs hit reach, close-range hits, walls and mounted use.
- [ ] Save/load: save with sword carried/equipped, restart, reload, verify retained item, stats and no duplicate automatic grant.
- [ ] Clipping: hand spacing, grip/pommel, shoulder/back while sheathed, cloaks, armour, mounted pose and camera. Check both blade faces, bright bevels, material and distant LODs for Custom.
- [ ] Drop/pickup: expected physics, orientation, collision and retained item identity.

For each failure, record game version, enabled modules, variant, reproduction steps, `dragonslayer.status`, and the relevant custom/native log excerpt. Do not upload saves or full personal logs unless needed and explicitly agreed.
