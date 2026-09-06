# Implementation and evidence

1. Inspect local game, schemas, public assembly signatures, native crafting definitions and build tools.
2. Introduce a new `dragonslayer` crafted item and four mod-owned crafting pieces. Use a private template and weapon description with verified native two-handed usage. Native visuals are a separately labelled prototype.
3. Use a small DLL for an explicit one-sword console grant, deterministic item-only stats and diagnostics. No Harmony, global patches, campaign behaviours, save data, automatic grants or multiplayer support.
4. Generate original centred crafting meshes, PBR texture atlas, LODs and FBX exports. Import and publish through the matching TaleWorlds editor before calling the custom visual installable.
5. Validate locally with installed schemas, compile with local references, test installer ownership, package only whitelisted mod files, and open a draft PR.

## Inspected installation

Initial game: v1.3.10, Steam build 21128576; editor v1.4.8. User updated the game during implementation. Final target is **v1.4.8.119303**, Steam build **24573425**, editor **v1.4.8**. `package_info.txt` reports compile changeset 119303 and asset-pack changeset 118999. The launcher independently displays v1.4.8.119303. Steam StateFlags became 4 after the update completed. No game paths or Steam account identifiers are committed.

Runtime DLLs target .NET Standard 2.0. This mod compiles as .NET Framework 4.7.2 using the game's bundled Mono reference assemblies and netstandard facade; all game references have `Private=false`. .NET SDK 9.0.100 was used. Blender portable 4.3.2 was downloaded from the official mirror; SHA256 `933a62a770c941e316535f893629ff106ddd2aa5a956f0081ec557439073d4f6` matched the official checksum. Tools stay in ignored `.tools`.

## Installed evidence (read, never redistributed)

- `bin/Win64_Shipping_Client/Version.xml`, `package_info.txt`, official modules' `SubModule.xml`.
- `XmlSchemas/Items.xsd`, `CraftingPieces.xsd`, `CraftingTemplates.xsd`, `WeaponDescriptions.xsd`, `GameText.xsd`.
- `Modules/Native/ModuleData/crafting_pieces.xml`: `battania_blade_1`, `battania_guard_9`, `khuzait_grip_14`, `battania_pommel_9`; exact mesh names, unscaled lengths, physics and crafting material IDs.
- `Modules/SandBoxCore/ModuleData/items/weapons.xml`: `early_retirement_2hsword_t3` demonstrates the selected native combination.
- `Modules/Native/ModuleData/crafting_templates.xml`: piece assembly order and item type.
- `weapon_descriptions.xml`: `TwoHandedSword`, `twohanded:block:swing:thrust`, `MeleeWeapon`, `NotUsableWithOneHand`.
- `item_usage_sets.xml` and `item_holsters.xml`: native combat and `sword_back` references.
- Reflection on installed assemblies confirmed the public `MBSubModuleBase.OnGameInitializationFinished(Game)`, `CommandLineArgumentFunction(string name, string groupname)`, `MBObjectManager.GetObject<T>(string)`, `MobileParty.MainParty.ItemRoster.AddToCounts(ItemObject,int)` and all 27 parameters of `WeaponComponentData.Init`. Compilation verifies these bindings again.
- Local IL inspection of crafting initialization established that the weapon description must explicitly allow every custom piece. A private description and regression check enforce this. The template name also needs a registered GameText entry. `WeaponComponentData.Init` resets the weapon frame, so the stat update explicitly restores it.
- The first editor launch reported a missing `Win64_Shipping_wEditor/Dragonslayer.dll`. The build now compiles against both installed configurations and packages both mod DLLs when matching editor references exist. A subsequent editor launch loaded Dragonslayer successfully, wrote its diagnostic log and reached Edit Mode.

## Official documentation consulted

- [Module structure](https://moddocs.bannerlord.com/asset-management/quickguide_create_a_mod/)
- [Crafting pieces and assembly](https://moddocs.bannerlord.com/asset-management/weapon_smithing/)
- [Asset import and client packages](https://moddocs.bannerlord.com/asset-management/asset-types/overriding_assets/)
- [Mesh naming and LOD grouping](https://moddocs.bannerlord.com/asset-management/asset-types/asset_naming_conventions/)
- [PBR material and packed channels](https://moddocs.bannerlord.com/editor/resource-editors/material_editor/)
- [Editor Publish Module](https://moddocs.bannerlord.com/steam-workshop/uploading_updating_mod/)

The current installed XML takes precedence where older docs differ (for example `modifier_group` and `item_type` in CraftingTemplate). No native XML schemas or assemblies are committed to make CI pass.
