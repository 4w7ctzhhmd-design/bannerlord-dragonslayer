# Custom sword: editor import and client publishing

Status: original sources and FBXs generated; Bannerlord import/publishing not yet completed. A `.blend` or `.fbx` alone is **not** a finished client asset. Do not rename prototype labels until a Custom build actually has published resources.

Use Modding Kit **v1.4.8**, matching game **v1.4.8.119303**. Run it from Steam → Library → Tools → Mount & Blade II Bannerlord – Modding Kit. Launching the managed launcher EXE directly without its expected environment can show “There is no target file”; use Steam instead.

## Short procedure

1. Build/install the prototype, restart the Modding Kit launcher, enable Dragonslayer with its official dependencies, and enter the editor. Work in the **Dragonslayer** module. If the editor does not offer it as editable, use File → Create New Module with the ID `Dragonslayer` in a separate development installation first; do not overwrite a populated folder. Keep the repository's SubModule/ModuleData definitions.
2. In the Resource Browser, select the Dragonslayer module's Assets area. Import the four `assets/source/dragonslayer_*.fbx` files and three `dragonslayer_*.png` textures. Preserve resource names exactly. Each FBX contains one centred crafting part plus names such as `.lod1` for grouping. Use metres, +Z along the blade, without extra scale or axis rotation; compare the unscaled lengths below with a native crafting piece in the Model Viewer before proceeding.
3. Create material **`dragonslayer_iron`**, shader **`pbr_metallic`**. Assign `dragonslayer_albedo` to diffuse/albedo, `dragonslayer_normal` to normal, and `dragonslayer_specular` to specular. Specular packs R=metallic, G=glossiness (1−roughness), B=AO, A=0. Treat normal/specular as linear data. Assign this material to all four meshes and their LODs; check both faces and grip in the Model Viewer.
4. Check names `dragonslayer_blade`, `dragonslayer_guard`, `dragonslayer_grip`, `dragonslayer_pommel`. Check that LODs are grouped, tangents/normals are valid and the atlas shows a dark body, bright bevels and brown grip. Save assets. Use File → **Publish Module**, target **Client**, into a new output directory. This is local asset compilation, not a Workshop upload. The result must include `Dragonslayer/AssetPackages/*.tpac`.
5. Run `./scripts/build.ps1 -Visual Custom -PublishedModule 'X:/Published/Dragonslayer'`. The build copies client AssetPackages and selects the custom piece mesh names. Install the result, launch the normal client and run the [in-game checklist](VERIFICATION.md). Verify inventory, unsheathed and holstered visuals and distant LODs before treating the custom visual as finished.

The exact placement of resource-import controls can vary with editor builds. The key outcome is imported named resources and a **Client** publish; copying source files into AssetSources does not replace these steps.

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
