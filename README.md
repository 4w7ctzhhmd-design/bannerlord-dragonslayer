# Dragonslayer for Bannerlord

An original Berserk-inspired sword mod for **Bannerlord v1.4.8.119303**, Windows Steam build **24573425**. Single-player only; multiplayer and co-op are untested and unsupported.

**Development build:** the installable prototype uses a labelled native sword visual. Original custom meshes, textures, FBXs and Blender source are included under `assets/source`; these are not yet published Bannerlord assets. See [asset import](docs/ASSET_IMPORT.md) and [verification](docs/VERIFICATION.md) for status. Unofficial fan project, not affiliated with TaleWorlds or the owners of Berserk.

![Original source render, not an in-game screenshot](assets/source/preview.png)

## Features

- New item `dragonslayer`, four mod-owned crafting pieces and a private template. No native item/asset replacement.
- Normal native two-handed sword animations, blocking, swings and thrusts. No one-handed usage.
- Long blade: 98.9 cm at 145% scale, about 143.4 cm before grip/assembly offsets. Native crafting determines final reach, weight and inertia; `dragonslayer.status` reports actual values.
- 180 base cutting damage, 65 piercing thrust damage, slow 58/55 swing/thrust speed ratings, 45 handling and substantial mass.
- Small C# module applies only this item's stats after campaign initialization and provides explicit grant/status commands. No powers, global patches, auto-grants, campaign behaviours or additional save data.

## Prerequisites and build

To play: the exact game version above, with Native, SandBox Core and Sandbox enabled. StoryMode is optional. No DLC or third-party mod dependency. Test first with only official modules and Dragonslayer.

To build: **PowerShell 7+**, .NET SDK **8 or 9**, local game client DLLs and the game's bundled `mono/lib/mono/4.7.2-api` reference assemblies. Blender **4.3.2** regenerates the included art. Matching Bannerlord Modding Kit **v1.4.8** is needed for custom asset import/client publishing.

Run from the repository root:

```powershell
./scripts/validate.ps1     # Portable source validation only
./scripts/test.ps1         # Negative cases and installer safety
./scripts/build.ps1        # Detect game, native XSD checks, compile, package
./scripts/install.ps1      # Install the latest build
```

Game detection reads Steam library folders. Override with `-GamePath 'X:/Games/Mount & Blade II Bannerlord'`, environment variable `BANNERLORD_GAME_PATH`, or ignored `local.settings.json` containing `{"gamePath":"X:/Games/Mount & Blade II Bannerlord"}`. Builds and installation check installed module versions against `target-game.json`. Future game updates need an API/schema review and retarget.

Builds go to `artifacts/build-<id>/Modules/Dragonslayer`; the ZIP is `artifacts/Dragonslayer-0.1.0-Prototype-v1.4.8.zip`. Only this mod's DLLs, XML, installation hash manifest and documentation are packaged. Matching editor references trigger a second compilation and include `bin/Win64_Shipping_wEditor/Dragonslayer.dll`; the normal client uses its own `Win64_Shipping_Client` DLL. Game assemblies have `Private=false` and are never copied into source control or ZIPs. The source `Modules/Dragonslayer` folder is not itself compiled.

CI is **Source validation (no game compilation)**: XML relationships, configuration ranges, installer safety and asset script syntax. It has no game references and does not compile the DLL, validate against native XSDs, or run Bannerlord. A green workflow is not a full mod build.

## Installation and removal

Close the game/editor, then run `./scripts/install.ps1`. Alternatively extract the ZIP's `Modules/Dragonslayer` folder into the game's `Modules` directory. Restart the launcher and enable Dragonslayer after Sandbox, preserving the normal official module order. Module/item names say **Native Prototype** until the custom build is imported and published.

The installer refuses an existing Dragonslayer folder. For a managed update:

```powershell
./scripts/uninstall.ps1
./scripts/build.ps1
./scripts/install.ps1
```

Uninstall checks hashes before removing listed files. It refuses edited files and preserves unlisted files, unrelated modules and all saves. Back up configuration edits and restore the originals before scripted removal. Manual uninstall means removing only `Modules/Dragonslayer`. Use a separate test save: removing an item mod while its items remain in a save can cause missing-item behaviour. Scripts never read or change saves.

## Obtain for testing

1. Start/load a single-player campaign and return to the campaign map.
2. Open the built-in console with **Alt + `** (the key above Tab on a US keyboard).
3. Run `dragonslayer.give` with no arguments. Each call deliberately adds **one** sword to the main party inventory.
4. Open inventory (**I**), find **Dragonslayer [Native Prototype]**, and equip it. Run `dragonslayer.status` to inspect registered stats.

This mod-owned command needs no additional console mod or global cheat-mode setting. It does not automatically grant items. Merchant stock is disabled; shops are not the test path. Runtime command discovery and inventory granting still need verification.

## Configuration

Edit source `Modules/Dragonslayer/ModuleData/weapon_stats.xml` and rebuild, or edit the installed file and fully restart/load the campaign. Installed XML edits need no DLL rebuild. These are mod-owned configuration fields, not TaleWorlds item XML attributes.

| Field | Default | Units | Accepted range |
|---|---:|---|---|
| `swingDamage` | 180 | Base cutting points, before armour/skill/speed modifiers | integer 1–500 |
| `thrustDamage` | 65 | Base piercing points, before modifiers | integer 1–500 |
| `swingSpeed` | 58 | Engine rating, not attacks/sec or milliseconds | integer 1–200 |
| `thrustSpeed` | 55 | Engine speed rating | integer 1–200 |
| `handling` | 45 | Engine handling rating; higher is easier | integer 1–200 |

These are this mod's enforced ranges, not theoretical engine limits. Invalid/missing configuration logs an error, shows a warning and retains generated crafting stats; no partial settings are applied.

Geometry settings:

- `dragonslayer_items.xml`, `Piece/@scale_factor`: percent, supported **50–200**. Scales the visual and derived weapon geometry together. Extreme values may clip.
- `dragonslayer_pieces.xml`, `CraftingPiece/@weight`: unscaled kilograms, validator accepts **(0,10]** per piece. Defaults: blade 4.0, guard 0.35, grip 0.35, pommel 0.25. Native crafting applies its scaling calculations to produce inventory mass.
- `CraftingPiece/@length`: unscaled mesh length in centimetres, **98.9 / 4.78 / 25 / 5.44**. Geometry metadata, not a free reach slider; keep it matched to exported meshes. The installed XSD has numeric types without useful gameplay bounds here.
- `BladeData/Swing` and `Thrust/@damage_factor`: dimensionless crafting inputs, not damage points. The DLL overrides the resulting damage/speed/handling only for `dragonslayer`.

Keep the IDs, native damage types and two-handed usage stable. Prototype and custom builds share item IDs.

## Diagnostics and troubleshooting

Custom log: `%LOCALAPPDATA%/Dragonslayer/Logs/Dragonslayer.log` (UTC; rotates at 1 MiB to `.previous`). Records module loading, resolved stats, grants and exceptions. Native logs: `%PROGRAMDATA%/Mount and Blade II Bannerlord/logs/`, including `rgl_log_*`, `launcher_log_*` and error logs when present. Build output is printed to the terminal.

- **Mod absent:** avoid double-nesting the extracted folder; restart launcher.
- **Dependency error:** verify exact game version and official IDs. Sandbox's ID is `Sandbox`, despite the `SandBox` folder name. Disable incompatible third-party mods for smoke testing.
- **Unknown command:** verify enabled module and `bin/Win64_Shipping_Client/Dragonslayer.dll`; check for the module-load log entry. If Windows blocked a downloaded DLL, use its file Properties → Unblock where available.
- **Missing item:** inspect native XML errors; the grant command will not conceal failed XML registration by fabricating an item.
- **Native appearance:** expected for the prototype. Source FBX files are not game TPACs.
- **Invisible/pink custom visual:** verify resource names, material texture slots and published client AssetPackages. The mere presence of a TPAC does not verify its contents.
- **Unexpected damage/speed:** inspect `dragonslayer.status` and the log for configuration failures. Base damage does not guarantee a fixed health loss through every armour/skill combination.
- **Failure after updating:** finish Steam verification and recheck versions/APIs; do not substitute random DLLs.

Report the action, exact version, prototype/custom variant, enabled modules and relevant log excerpt. See the [in-game checklist](docs/VERIFICATION.md).

## Original asset source

```powershell
& 'C:/path/to/blender.exe' --background --factory-startup --python assets/generate_sword.py
& 'C:/path/to/blender.exe' --background --factory-startup --python assets/verify_exports.py
```

Creates four centred crafting-part FBXs with LODs/UVs, packed `.blend`, 512×512 PBR textures and a source-only preview. The `pbr_metallic` specular texture packs R=metallic, G=glossiness, B=AO, A=0. The flat normal map is intentional: bevels and wrapping are geometry. No external artwork or extracted game assets are included.

Follow [editor import and publishing](docs/ASSET_IMPORT.md), then build with actual published client TPACs:

```powershell
./scripts/build.ps1 -Visual Custom -PublishedModule 'X:/EditorPublish/Dragonslayer'
```

Custom mode changes only the four mesh references and labels in the build. It requires AssetPackages but cannot certify resource linkage or in-game visuals automatically. No Workshop upload, GitHub release or merge is included in this work. See [implementation evidence](docs/IMPLEMENTATION.md).
