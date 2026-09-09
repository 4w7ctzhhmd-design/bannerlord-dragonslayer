using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;
namespace DragonslayerGutsPowers {
internal sealed class GutsStatModel : AgentStatCalculateModel {
// Delegate unchanged behavior to the model registered before this one.
public override void InitializeAgentStats(Agent agent, Equipment spawnEquipment, AgentDrivenProperties agentDrivenProperties, AgentBuildData agentBuildData) => BaseModel.InitializeAgentStats(agent, spawnEquipment, agentDrivenProperties, agentBuildData);
public override void InitializeMissionEquipment(Agent agent) => BaseModel.InitializeMissionEquipment(agent);
public override void InitializeAgentStatsAfterDeploymentFinished(Agent agent) => BaseModel.InitializeAgentStatsAfterDeploymentFinished(agent);
public override void InitializeMissionEquipmentAfterDeploymentFinished(Agent agent) => BaseModel.InitializeMissionEquipmentAfterDeploymentFinished(agent);
public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties) {
    BaseModel.UpdateAgentStats(agent, agentDrivenProperties);
    if (!DragonslayerWielderController.IsWielder(agent)) return;
    var c = DragonslayerConfig.Current;
    agentDrivenProperties.SwingSpeedMultiplier *= 1f + c.SwingSpeedBonus;
    agentDrivenProperties.HandlingMultiplier *= 1f + c.HandlingBonus;
}
public override float GetDifficultyModifier() => BaseModel.GetDifficultyModifier();
public override bool CanAgentRideMount(Agent agent, Agent targetMount) => BaseModel.CanAgentRideMount(agent, targetMount);
public override bool HasHeavyArmor(Agent agent) => BaseModel.HasHeavyArmor(agent);
public override float GetEffectiveArmorEncumbrance(Agent agent, Equipment equipment) => BaseModel.GetEffectiveArmorEncumbrance(agent, equipment);
public override float GetEffectiveMaxHealth(Agent agent) => BaseModel.GetEffectiveMaxHealth(agent);
public override float GetEnvironmentSpeedFactor(Agent agent) => BaseModel.GetEnvironmentSpeedFactor(agent);
public override float GetWeaponInaccuracy(Agent agent, WeaponComponentData weapon, int weaponSkill) => BaseModel.GetWeaponInaccuracy(agent, weapon, weaponSkill);
public override float GetDetachmentCostMultiplierOfAgent(Agent agent, IDetachment detachment) => BaseModel.GetDetachmentCostMultiplierOfAgent(agent, detachment);
public override float GetInteractionDistance(Agent agent) => BaseModel.GetInteractionDistance(agent);
public override float GetMaxCameraZoom(Agent agent) => BaseModel.GetMaxCameraZoom(agent);
public override int GetEffectiveSkill(Agent agent, SkillObject skill) => BaseModel.GetEffectiveSkill(agent, skill);
public override int GetEffectiveSkillForWeapon(Agent agent, WeaponComponentData weapon) => BaseModel.GetEffectiveSkillForWeapon(agent, weapon);
public override float GetWeaponDamageMultiplier(Agent agent, WeaponComponentData weapon) => BaseModel.GetWeaponDamageMultiplier(agent, weapon);
public override float GetEquipmentStealthBonus(Agent agent) => BaseModel.GetEquipmentStealthBonus(agent);
public override float GetSneakAttackMultiplier(Agent agent, WeaponComponentData weapon) => BaseModel.GetSneakAttackMultiplier(agent, weapon);
public override float GetKnockBackResistance(Agent agent) => BaseModel.GetKnockBackResistance(agent);
public override float GetKnockDownResistance(Agent agent, StrikeType strikeType = StrikeType.Invalid) => BaseModel.GetKnockDownResistance(agent, strikeType);
public override float GetDismountResistance(Agent agent) => BaseModel.GetDismountResistance(agent);
public override float GetBreatheHoldMaxDuration(Agent agent, float baseBreatheHoldMaxDuration) => BaseModel.GetBreatheHoldMaxDuration(agent, baseBreatheHoldMaxDuration);
public override string GetMissionDebugInfoForAgent(Agent agent) => BaseModel.GetMissionDebugInfoForAgent(agent);
}}
