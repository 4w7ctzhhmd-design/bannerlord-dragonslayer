# Custom sword: editor import and client publishing

Status: all four original FBXs and three textures were imported in Modding Kit v1.4.8, material links were repaired by reimporting after material creation, and Client publishing completed on 2026-09-06. The blade was viewed in the engine's Meta Mesh Editor. `assets/published` contains the original published client package and runtime data; the default build includes them. Assembled appearance and gameplay remain untested and are assigned to the user. No further editor interaction is required to install this test build.

Use Modding Kit **v1.4.8**, matching game **v1.4.8.119303**. Run it from Steam → Library → Tools → Mount & Blade II Bannerlord – Modding Kit. Launching the managed launcher EXE directly without its expected environment can show “There is no target file”; use Steam instead.

## Short procedure

1. For a fresh authoring workspace, build/install with `-Visual Prototype`, create empty `Assets` and `AssetSources` directories inside only `Modules/Dragonslayer`, then restart the Modding Kit launcher. Enable Dragonslayer with its official dependencies and enter Edit Mode. Work in the **Dragonslayer** module. Preserve any existing authoring folder before replacing an installation.
2. In the Resource Browser, select the Dragonslayer module's Assets area. Import the four `assets/source/dragonslayer_*.fbx` files and three `dragonslayer_*.png` textures. Preserve resource names exactly. Each FBX contains one centred crafting part plus names such as `.lod1` for grouping. Use metres, +Z along the blade, without extra scale or axis rotation; compare the unscaled lengths below with a native crafting piece in the Model Viewer before proceeding.
3. Create material **`dragonslayer_iron`**, shader **`pbr_metallic`**. Assign `dragonslayer_albedo` to diffuse/albedo, `dragonslayer_normal` to normal, and `dragonslayer_specular` to specular. Specular packs R=metallic, G=glossiness (1−roughness), B=AO, A=0. Treat normal/specular as linear data. Assign this material to all four meshes and their LODs; check both faces and grip in the Model Viewer.
4. Check names `dragonslayer_blade`, `dragonslayer_guard`, `dragonslayer_grip`, `dragonslayer_pommel`. Check that LODs are grouped, tangents/normals are valid and the atlas shows a dark body, bright bevels and brown grip. Save assets. Use File → **Publish Module**, target **Client**, into a new output directory. This is local asset compilation, not a Workshop upload. The result must include `Dragonslayer/AssetPackages/*.tpac`.
5. Run `./scripts/build.ps1 -Visual Custom -PublishedModule 'X:/Published/Dragonslayer'`. The build copies client AssetPackages and selects the custom piece mesh names. Install the result, launch the normal client and run the [in-game checklist](VERIFICATION.md). Verify inventory, unsheathed and holstered visuals and distant LODs before treating the custom visual as finished.

Observed controls in this editor: Resource Browser → Modules → Dragonslayer → Assets; right-click the empty grid → Import new asset. FBX import reports +Z up, right-handed, metres; keep unit conversion at metres without extra rotation. Create the material through right-click → Create → Material, rename it, and drag textures into its inspector slots. Set the normal texture's Type preset to **Normal** and the packed specular texture's preset to **Specular**, then Save each; automatic import initially assigned Albedo to both. If meshes were imported before their material, right-click each FBX source and choose Reimport after saving the material. Check every LOD's material link. Publish Module → select **Dragonslayer** → **Client** → Publish → choose a new writable folder outside the installed module. Keep the emitted AssetPackages and RuntimeDataCache together. Do not choose Native or upload to Workshop.

The editor repeatedly asserted at `rglIntrusive_ptr.h:151` when closing the empty scene after successful publishing. This has not been diagnosed as mod-specific. The successful client export and checksums are recorded separately from the unresolved shutdown issue.

## Resource contract

| Mesh/resource | Unscaled length | Item scale | Material |
|---|---:|---:|---|
| `dragonslayer_blade` | 98.9 cm | 145% | `dragonslayer_iron` |
| `dragonslayer_guard` | 4.78 cm | 100% | same |
| `dragonslayer_grip` | 25 cm | 120% | same |
| `dragonslayer_pommel` | 5.44 cm | 100% | same |

All exported pivots are centred at world origin, as required for smithing pieces. `assets/source/dragonslayer.blend` preserves that export arrangement; `preview.png` is a separately assembled illustration. Preview positioning is not evidence that Bannerlord's weapon assembly, orientation or collisions have passed testing. The game reuses verified native `bo_sword_one_handed` physics data through crafting; the custom slab's dropped collision shape and reach still need in-game checks.

Do not copy any native TPACs, textures or DLLs into the published folder to make a missing resource disappear. The build refuses Custom without actual TPACs, but cannot verify their resource contents merely from file presence. Keep publisher output isolated to Dragonslayer. No Workshop uploader is used.

Authoring sources: [TaleWorlds crafting](https://moddocs.bannerlord.com/asset-management/weapon_smithing/), [asset management](https://moddocs.bannerlord.com/asset-management/asset-types/overriding_assets/), [material editor](https://moddocs.bannerlord.com/editor/resource-editors/material_editor/) and [module publishing](https://moddocs.bannerlord.com/steam-workshop/uploading_updating_mod/).
