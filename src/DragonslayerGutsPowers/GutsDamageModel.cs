using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;
namespace DragonslayerGutsPowers {
internal sealed class GutsDamageModel : AgentApplyDamageModel {
// Delegate unchanged behavior to the model registered before this one.
public override bool IsDamageIgnored(in AttackInformation attackInformation, in AttackCollisionData collisionData) => BaseModel.IsDamageIgnored(in attackInformation, in collisionData);
public override float ApplyDamageAmplifications(in AttackInformation attackInformation, in AttackCollisionData collisionData, float baseDamage) => BaseModel.ApplyDamageAmplifications(in attackInformation, in collisionData, baseDamage);
public override float ApplyDamageScaling(in AttackInformation attackInformation, in AttackCollisionData collisionData, float baseDamage) => BaseModel.ApplyDamageScaling(in attackInformation, in collisionData, baseDamage);
public override float ApplyDamageReductions(in AttackInformation attackInformation, in AttackCollisionData collisionData, float baseDamage) => BaseModel.ApplyDamageReductions(in attackInformation, in collisionData, baseDamage);
public override float ApplyGeneralDamageModifiers(in AttackInformation attackInformation, in AttackCollisionData collisionData, float baseDamage) {
    float value = BaseModel.ApplyGeneralDamageModifiers(in attackInformation, in collisionData, baseDamage);
    return IsBladeHit(in attackInformation) && !collisionData.IsAlternativeAttack && !collisionData.IsMissile
        && !collisionData.IsHorseCharge && !collisionData.IsFallDamage ? value * DragonslayerConfig.Current.StrengthMultiplier : value;
}
public override void DecideMissileWeaponFlags(Agent attackerAgent, in MissionWeapon missileWeapon, ref WeaponFlags missileWeaponFlags) => BaseModel.DecideMissileWeaponFlags(attackerAgent, in missileWeapon, ref missileWeaponFlags);
public override void CalculateDefendedBlowStunMultipliers(Agent attackerAgent, Agent defenderAgent, CombatCollisionResult collisionResult, WeaponComponentData attackerWeapon, WeaponComponentData defenderWeapon, ref float attackerStunPeriod, ref float defenderStunPeriod) {
    BaseModel.CalculateDefendedBlowStunMultipliers(attackerAgent, defenderAgent, collisionResult, attackerWeapon, defenderWeapon, ref attackerStunPeriod, ref defenderStunPeriod);
    if (DragonslayerWielderController.IsWielder(attackerAgent) && attackerWeapon == attackerAgent.WieldedWeapon.CurrentUsageItem)
        defenderStunPeriod *= DragonslayerConfig.Current.StaggerPowerMultiplier;
}
public override float CalculateStaggerThresholdDamage(Agent defenderAgent, in Blow blow) {
    float value = BaseModel.CalculateStaggerThresholdDamage(defenderAgent, in blow);
    var c = DragonslayerConfig.Current;
    if (!DragonslayerWielderController.IsWielder(defenderAgent) || blow.IsMissile || blow.IsFallDamage || blow.AttackType != AgentAttackType.Standard
        || blow.InflictedDamage > c.PoiseMaxIncomingDamage) return value;
    return value * (DragonslayerWielderController.IsAttacking(defenderAgent) ? c.AttackPoiseMultiplier : c.PoiseMultiplier);
}
public override float CalculateAlternativeAttackDamage(in AttackInformation attackInformation, in AttackCollisionData collisionData, WeaponComponentData weapon) => BaseModel.CalculateAlternativeAttackDamage(in attackInformation, in collisionData, weapon);
public override float CalculatePassiveAttackDamage(BasicCharacterObject attackerCharacter, in AttackCollisionData collisionData, float baseDamage) => BaseModel.CalculatePassiveAttackDamage(attackerCharacter, in collisionData, baseDamage);
public override MeleeCollisionReaction DecidePassiveAttackCollisionReaction(Agent attacker, Agent defender, bool isFatalHit) => BaseModel.DecidePassiveAttackCollisionReaction(attacker, defender, isFatalHit);
public override void DecideWeaponCollisionReaction(in Blow registeredBlow, in AttackCollisionData collisionData, Agent attacker, Agent defender, in MissionWeapon attackerWeapon, bool isFatalHit, bool isShruggedOff, float momentumRemaining, out MeleeCollisionReaction colReaction) => BaseModel.DecideWeaponCollisionReaction(in registeredBlow, in collisionData, attacker, defender, in attackerWeapon, isFatalHit, isShruggedOff, momentumRemaining, out colReaction);
public override float CalculateShieldDamage(in AttackInformation attackInformation, float baseDamage) {
    float value = BaseModel.CalculateShieldDamage(in attackInformation, baseDamage);
    if (!IsBladeHit(in attackInformation)) return value;
    var boosted = value * DragonslayerConfig.Current.ShieldDamageMultiplier;
    DebugLog.Write("Shield pressure: agent=" + attackInformation.AttackerAgent.Index + ", base=" + value + ", boosted=" + boosted);
    return boosted;
}
public override float CalculateSailFireDamage(Agent attackerAgent, IShipOrigin shipOrigin, float baseDamage, bool damageFromShipMachine) => BaseModel.CalculateSailFireDamage(attackerAgent, shipOrigin, baseDamage, damageFromShipMachine);
public override float CalculateHullFireDamage(float baseFireDamage, IShipOrigin shipOrigin) => BaseModel.CalculateHullFireDamage(baseFireDamage, shipOrigin);
public override float GetDamageMultiplierForBodyPart(BoneBodyPartType bodyPart, DamageTypes type, bool isHuman, bool isMissile) => BaseModel.GetDamageMultiplierForBodyPart(bodyPart, type, isHuman, isMissile);
public override bool CanWeaponIgnoreFriendlyFireChecks(WeaponComponentData weapon) => BaseModel.CanWeaponIgnoreFriendlyFireChecks(weapon);
public override bool CanWeaponDealSneakAttack(in AttackInformation attackInformation, WeaponComponentData weapon) => BaseModel.CanWeaponDealSneakAttack(in attackInformation, weapon);
public override bool CanWeaponDismount(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData) => BaseModel.CanWeaponDismount(attackerAgent, attackerWeapon, in blow, in collisionData);
public override bool CanWeaponKnockback(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData) => BaseModel.CanWeaponKnockback(attackerAgent, attackerWeapon, in blow, in collisionData);
public override bool CanWeaponKnockDown(Agent attackerAgent, Agent victimAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData) => BaseModel.CanWeaponKnockDown(attackerAgent, victimAgent, attackerWeapon, in blow, in collisionData);
public override bool DecideCrushedThrough(Agent attackerAgent, Agent defenderAgent, float totalAttackEnergy, Agent.UsageDirection attackDirection, StrikeType strikeType, WeaponComponentData defendItem, bool isPassiveUsageHit) => BaseModel.DecideCrushedThrough(attackerAgent, defenderAgent, totalAttackEnergy, attackDirection, strikeType, defendItem, isPassiveUsageHit);
public override float CalculateRemainingMomentum(float originalMomentum, in Blow b, in AttackCollisionData collisionData, Agent attacker, Agent victim, in MissionWeapon attackerWeapon, bool isCrushThrough) => BaseModel.CalculateRemainingMomentum(originalMomentum, in b, in collisionData, attacker, victim, in attackerWeapon, isCrushThrough);
public override bool DecideAgentShrugOffBlow(Agent victimAgent, in AttackCollisionData collisionData, in Blow blow) => BaseModel.DecideAgentShrugOffBlow(victimAgent, in collisionData, in blow);
public override bool DecideAgentDismountedByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow) => BaseModel.DecideAgentDismountedByBlow(attackerAgent, victimAgent, in collisionData, attackerWeapon, in blow);
public override bool DecideAgentKnockedBackByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow) {
    bool native = BaseModel.DecideAgentKnockedBackByBlow(attackerAgent, victimAgent, in collisionData, attackerWeapon, in blow);
    if (!native || !DragonslayerWielderController.IsWielder(victimAgent) || collisionData.IsMissile || collisionData.IsHorseCharge
        || collisionData.IsFallDamage || blow.InflictedDamage > DragonslayerConfig.Current.PoiseMaxIncomingDamage) return native;
    return MBRandom.RandomFloat >= DragonslayerConfig.Current.KnockbackResistanceBonus;
}
public override bool DecideAgentKnockedDownByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow) => BaseModel.DecideAgentKnockedDownByBlow(attackerAgent, victimAgent, in collisionData, attackerWeapon, in blow);
public override bool DecideMountRearedByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow) => BaseModel.DecideMountRearedByBlow(attackerAgent, victimAgent, in collisionData, attackerWeapon, in blow);
public override bool ShouldMissilePassThroughAfterShieldBreak(Agent attackerAgent, WeaponComponentData attackerWeapon) => BaseModel.ShouldMissilePassThroughAfterShieldBreak(attackerAgent, attackerWeapon);
public override float GetDismountPenetration(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData) => BaseModel.GetDismountPenetration(attackerAgent, attackerWeapon, in blow, in collisionData);
public override float GetKnockBackPenetration(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData) => BaseModel.GetKnockBackPenetration(attackerAgent, attackerWeapon, in blow, in collisionData);
public override float GetKnockDownPenetration(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData) => BaseModel.GetKnockDownPenetration(attackerAgent, attackerWeapon, in blow, in collisionData);
public override float GetHorseChargePenetration() => BaseModel.GetHorseChargePenetration();
private static bool IsBladeHit(in AttackInformation info) => DragonslayerWielderController.IsWielder(info.AttackerAgent)
    && info.AttackerWeapon.Item?.StringId == DragonslayerWielderController.ItemId;
}}
