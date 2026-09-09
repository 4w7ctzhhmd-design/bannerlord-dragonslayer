# Verification record

Target: **v1.4.8.119303**, Steam build **24573425**, editor **v1.4.8**. See `target-game.json` and [implementation evidence](IMPLEMENTATION.md).

## Completed checks

- Compiled client and editor DLLs against the updated installed assemblies with .NET SDK 9.0.100: zero warnings/errors.
- Validated all five registered XMLs against installed TaleWorlds XSDs.
- Checked module registration, exact dependency ID casing, distinct item ID, native mesh references, template/piece relationships and supported configuration ranges.
- Generated original four-part FBX exports, LODs, UVs, packed Blender source and three PBR PNG textures; rendered and visually inspected the source sword.
- Reimported all four FBXs in Blender and checked dimensions, names, LODs, pivots, materials, UVs and nondegenerate faces.
- Built the custom-visual ZIP with mod DLL/XML, original Client-published asset package/runtime data, installation manifest and documentation; no proprietary assemblies required in CI.
- Exercised installation/removal in an isolated fixture; existing-folder refusal, edited-file preservation, unrelated-file preservation, traversal rejection and invalid stats/missing-piece cases passed.
- Installed the native-visual prototype into only `Modules/Dragonslayer` and saw its registration in the Modding Kit launcher. Corrected the `Sandbox` dependency ID after local inspection.
- Launched the matching editor with Dragonslayer enabled, reached its main menu and Edit Mode. The engine log confirmed successful assembly loading; Dragonslayer's log confirmed its load callback. Fixed the initial missing editor DLL and retested startup.
- Imported all four FBXs/LODs and three textures into the matching editor, created `dragonslayer_iron` using `pbr_metallic`, corrected Normal/Specular import presets and repaired initial missing-material links by reimporting. Inspected the blade in the Meta Mesh Editor.
- Completed Client publishing: editor reported `All packages created`; original pack0.tpac is 826730 bytes. Recorded published file hashes in `assets/publish-record.json`; CI checks integrity without claiming to decode/render the resources.
- On 2026-09-09, rebuilt the custom package with zero compiler warnings/errors, checked the extracted ZIP against its hash manifest and custom mesh references, and installed it in only the game's Dragonslayer folder. Preserved the authoring workspace under ignored `artifacts/editor-backup-20260906`; the leftover editor DLL was removed only after its backup checksum matched. No game was launched for this deployment.

## Pending runtime verification

### 0.1.1 new-campaign crash fix

The user's 2026-09-09 crash dump for process 56036 contains `System.InvalidOperationException: Sequence contains no matching element` in `CraftingCampaignBehavior.GetWeaponPieces`, called by `CreateTownOrder` during new-game initialization. Local inspection of the installed implementation confirmed every crafting template can be randomly selected; its final fallback requires a non-hidden piece of each represented type. All four Dragonslayer pieces were hidden. Version 0.1.1 sets them visible, and a negative regression test now rejects hiding the only eligible piece. The dump was analyzed locally and is not committed or uploaded. Gameplay confirmation of the fix remains pending user retest.

The user's mod log confirmed successful item registration and configured stats before the crash: weight 13.21 kg, reach 162 cm, swing 180 Cut, thrust 65 Pierce, speeds 58/55, handling 45. No grant or combat success is inferred from that log.

During the first editor session, exiting the empty default scene triggered a native assertion at `rglIntrusive_ptr.h:151`, `px != nullptr`, after scene finalization. The process was stopped and restarted. The trace does not establish a mod-specific cause; successful assembly loading is not evidence that editor shutdown or gameplay passed.

The shutdown assertion recurred after the successful client publish. Its cause remains unresolved. The normal game has **not** been played to verify this mod. Custom assets are imported/published and included in a deployable test package; resource linkage in a campaign, assembled visuals and gameplay are not yet verified. Per the user's request, all in-game testing is left to the user using the README's How to test section.

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
