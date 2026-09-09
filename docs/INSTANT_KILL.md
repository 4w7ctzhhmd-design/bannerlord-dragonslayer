# Dragonslayer Instant Kill — optional add-on 0.1.0

Requires the separate **Dragonslayer 0.1.1** sword mod and Bannerlord **v1.4.8.119303**. Single-player campaign only. This add-on does not include or replace the sword's assets or stats. Disable/remove only this add-on to return to the standalone sword.

An unblocked Dragonslayer blade hit sets a living human target's remaining health to zero during the engine's normal hit callback. The installed v1.4.8 `Agent.RegisterBlow` implementation then invokes normal death handling with the original attack, preserving its attacker attribution. No recursive extra blow, global damage patch or increased base damage is used. Armour cannot prevent the effect, including a valid hit whose damage was fully absorbed.

It applies to any wielder, including an NPC holding Dragonslayer, and any human target the game permits the blade to hit. It excludes weapon/shield blocks, shields on the back, kicks/bashes, projectiles, horse charges, fall damage, mounts and other weapons. It does not bypass collision or friendly-fire restrictions. Conversation/cutscene and missions that disable dying retain those protections. This is immediate defeat/death in the mission; the campaign's hero survival/death rules and nonlethal mission outcomes still apply. It does not execute named heroes permanently on the campaign map.

## Install and disable

1. Exit the game completely before installing DLLs.
2. Keep `Modules/Dragonslayer` installed. Extract this ZIP's `Modules/DragonslayerInstantKill` into the game's Modules folder.
3. Restart the launcher. Enable **Dragonslayer Instant Kill (Optional)** after **Dragonslayer** and its official dependencies.
4. Use the same sword already in your inventory, or run `dragonslayer.give` on the campaign map.

To disable, uncheck only **Dragonslayer Instant Kill (Optional)** and restart the game. To uninstall, remove only `Modules/DragonslayerInstantKill`. No saved item IDs, save data, native assets or base sword files are changed by the add-on.

From the source repository (PowerShell 7):

```powershell
./scripts/build-instant-kill.ps1
./scripts/install.ps1 -ModuleId DragonslayerInstantKill
./scripts/uninstall.ps1 -ModuleId DragonslayerInstantKill
```

Build accepts `-GamePath` and the same configurable local game references as the base mod. The archive contains only this add-on's DLLs, registration, ownership manifest and this guide. Do not install two copies of the base sword.

## How to test

Gameplay of the effect is **not yet verified**. Use a separate test save.

1. Confirm the base sword still appears and the game loads with both modules enabled.
2. Strike an unblocked, healthy enemy with a swing and then a thrust: each should immediately defeat the target, including a heavily armoured one.
3. Test a shield block and weapon parry: neither should kill. Test a kick/bash while holding the sword: it should behave normally.
4. Hit an enemy with a normal weapon to confirm normal damage; strike a mount to confirm it receives only normal sword damage.
5. Check attribution/kill feed, battle completion, loot and return to campaign. Test save/reload. Tournament and named-hero outcomes should retain the game's campaign rules.
6. Disable the add-on, restart, and verify the same sword still works with ordinary damage. Re-enable when desired.

Logs: `%LOCALAPPDATA%/Dragonslayer/Logs/InstantKill.log`. Each triggered effect records attacker/victim agent indices and remaining health before the effect. Logs rotate at 1 MiB. Report which test failed, enabled modules, exact error and relevant log lines. Base sword logging remains in `Dragonslayer.log`.

## Preserved standalone version

The user confirmed the sword could be obtained in-game in base version 0.1.1. Source is frozen at tag `dragonslayer-standalone-v0.1.1`; the preserved local archive is `artifacts/standalone/Dragonslayer-0.1.1-Standalone-v1.4.8.zip` (SHA256 `A045638D7B76F2A5BC7D126F5D0E5FDD246FE8AB503CDF12CB81FF665AB357F4`). This is a source tag/archive, not a published release. Full combat/save-load verification of the base remains pending.
