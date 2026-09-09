# Dragonslayer Guts Powers — Phase 1, version 0.1.0

Optional companion to the working **Dragonslayer 0.1.1** on Bannerlord **v1.4.8.119303**. The verified item ID is `dragonslayer`. Neither its assets, item definitions, grant command, base stats nor the separate Instant Kill module are rewritten. The standalone source tag/archive remain available.

## Install and scope

Extract the ZIP's `Modules/DragonslayerGutsPowers` into the game's Modules folder with the game closed. Enable **Dragonslayer Guts Powers (Optional)** after **Dragonslayer**. For the intended mortal swordsman, disable **Dragonslayer Instant Kill (Optional)** in the launcher; it remains a separate module and would still kill on contact if also enabled. Restart the game when changing modules or configuration.

From the repository:

```powershell
./scripts/build-guts-powers.ps1
./scripts/install.ps1 -ModuleId DragonslayerGutsPowers
./scripts/uninstall.ps1 -ModuleId DragonslayerGutsPowers
```

Only the new module folder is installed/removed. The package carries mod DLLs, configuration, registration and a file ownership manifest, never game assemblies. No campaign save behavior or persistent character/weapon edits are added. All agent subscriptions are released on removal/mission cleanup. Main campaign testing should wait for separate test-save verification.

## Phase 1 architecture and installed API evidence

- `SubModule`: campaign-only registration through the explicit generic `IGameStarter.AddModel<T>` overload. Local inspection confirmed that overload initializes `MBGameModel<T>.BaseModel` with the previous model. Missing base models cause logging and skip registration.
- `DragonslayerWielderController`: reads `Agent.WieldedWeapon.Item.StringId`, active/human status and player-control settings. Inventory ownership alone never qualifies. Agent build, wield-change and mounted-state events trigger refresh; death/deletion/end cleanup releases subscriptions. Every model calculation checks current wielding directly, including player-control changes. There is no every-frame scan of all agents.
- `GutsStatModel`: delegates to the prior model before multiplying `AgentDrivenProperties.SwingSpeedMultiplier` and `HandlingMultiplier`. The installed Sandbox model resets those properties during each update, so bonuses do not accumulate. Switching away triggers fresh native values.
- `GutsDamageModel`: modest final blade damage, native shield-damage multiplier, defender stun on blocked strikes, increased native stagger threshold for modest standard melee hits, and a limited chance to resist a knockback the native model already selected. It forwards all other abstract/virtual hooks, including weather/skills/mount and naval behavior, to the previous model.
- `DragonslayerConfig` and `DebugLog`: bounded XML configuration, invariant numeric parsing, invalid-config fail-closed behavior, rotating diagnostics.

## Configuration

Edit `Modules/DragonslayerGutsPowers/ModuleData/guts_config.xml`, then fully restart the game. Every listed option is required; invalid/unknown options disable powers and log the reason. These are mod-enforced ranges, not theoretical engine limits.

| Option | Default | Meaning / range |
|---|---:|---|
| EnableGutsPowers | true | Master enable, true/false |
| PlayerOnly | false | false includes NPCs actively wielding the sword; true limits to player-controlled agents |
| DebugLogging | true | Wielder transitions and shield events; startup/config errors always logged |
| StrengthMultiplier | 1.25 | Final melee blade damage scale, 1–4; retains native armour, damage-ignore and friendly-fire rules |
| SwingSpeedBonus | 0.45 | Additive fraction: native swing multiplier ×1.45; 0–1 |
| HandlingBonus | 0.35 | Native handling multiplier ×1.35; 0–1 |
| ShieldDamageMultiplier | 2.5 | Native shield damage ×2.5; 1–4 |
| StaggerPowerMultiplier | 1.5 | Defender stun duration on blocked sword strikes ×1.5; 1–4 |
| PoiseMultiplier | 1.6 | Native stagger threshold scale outside attacks; 1–4 |
| AttackPoiseMultiplier | 2.2 | Threshold scale while preparing/releasing an attack; 1–4 |
| PoiseMaxIncomingDamage | 35 | Upper eligible incoming damage points; 1–100; stronger blows retain native disruption |
| KnockbackResistanceBonus | 0.15 | Probability, 0–1, to resist an otherwise native knockback from eligible modest melee hits |

No healing, health increase, injury suppression or god mode is implemented. Damage still reduces health normally. Poise excludes projectile/fall/nonstandard blows and powerful hits; knockback resistance also explicitly excludes horse charges. Native knockdown/dismount rules remain unchanged.

## Approximations and deferred work

Swing/handling multipliers are the stable Phase 1 approximation for managing the sword's mass and recovery. No separate recovery-animation hack is used. Shield damage naturally scales from the underlying hit; blocked-strike stun uses a constant multiplier because that hook does not supply hit energy. Heavy-hit discrimination, guard-crush/knockdown effects and cleave belong to Phase 2. Poise raises an existing stagger threshold rather than forcing animation cancellation or total interruption immunity. No extra ragdoll impulses are applied.

Phases 2–6 (cleave, rage, Berserker Mode, injury costs, last stand, fear, projectile defense and cosmetics/UI) are not implemented. There are no placeholder config switches claiming those features work. Future systems can consume the shared detection/config architecture.

## How to test Phase 1

All new gameplay behavior is pending user testing. Disable Instant Kill first. Use a separate campaign save and official modules + Dragonslayer + Guts Powers.

1. Load/start a campaign and use `dragonslayer.give` if needed. Confirm the original sword/visual still works.
2. Carry it in inventory without drawing it: no powers. Draw it: `GutsPowers.log` should show `powers=True`. Switch to another weapon or sheath: `powers=False` and ordinary speed/handling should return. Repeat many times to check no bonus stacking.
3. Compare swings/handling with the module disabled, using the same character/weapon. Inventory base stats are not globally modified, so assess actual animations and combat.
4. Test NPC wielding with PlayerOnly=false, then restart with PlayerOnly=true to confirm NPCs lose bonuses. Test player-control transfers if available.
5. Hit a shield repeatedly and compare damage/stagger with the module disabled. Check a normal sword remains unchanged. Test light/glancing versus stronger attacks; no forced explosion or launch should occur.
6. Take modest melee hits while idle and while attacking. Interruption should be reduced, not removed. Test heavy strikes, missiles and horse impacts still disrupt normally and health remains finite.
7. Draw/switch before and after mounting/dismounting. Test battles, sieges and campaign tournaments. Death/respawn, ending a mission and save/reload must not leave stale bonuses or crash.
8. Disable only Guts Powers and restart: the same standalone sword should still work normally.

Logs: `%LOCALAPPDATA%/Dragonslayer/Logs/GutsPowers.log`, rotates at 1 MiB. Report test step, config, enabled modules, exact error and relevant log lines. For a crash also provide the newest native `rgl_log_*` and allow a local crash dump to be generated; no upload is required for local analysis.
