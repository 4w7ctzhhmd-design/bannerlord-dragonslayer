// Test doubles only. No game DLLs, native engine code or simulated combat.
// A premature equipment read throws, so tests exercise production guard ordering.
using System;
namespace TaleWorlds.Core { public class Banner {} }
namespace TaleWorlds.MountAndBlade
{
    public enum MissionBehaviorType { Other }
    public enum AgentState { Killed }
    public struct KillingBlow {}
    public static class GameNetwork { public static bool IsSessionActive; }
    public class Item { public string StringId; }
    public struct MissionWeapon { public Item Item; }
    public class Mission
    {
        public MissionBehavior Behavior;
        public T GetMissionBehavior<T>() where T : class => Behavior as T;
    }
    public abstract class MissionBehavior
    {
        public abstract MissionBehaviorType BehaviorType { get; }
        public virtual void OnAgentBuild(Agent agent, TaleWorlds.Core.Banner banner) {}
        public virtual void OnAgentControllerSetToPlayer(Agent agent) {}
        public virtual void OnAgentRemoved(Agent agent, Agent attacker, AgentState state, KillingBlow blow) {}
        public virtual void OnAgentDeleted(Agent agent) {}
        public virtual void OnRemoveBehavior() {}
    }
    public class Agent
    {
        public enum ActionStage { None, AttackReady, AttackQuickReady, AttackRelease }
        public bool IsHuman, IsPlayerControlled, Active = true, RejectWeaponRead = true;
        public int Index, WeaponReads;
        public object Equipment;
        public Mission Mission;
        public string WeaponId;
        public Action OnAgentWieldedItemChange, OnAgentMountedStateChanged;
        public bool IsActive() => Active;
        public ActionStage GetCurrentActionStage(int channel) => ActionStage.None;
        public void UpdateAgentProperties() {}
        public MissionWeapon WieldedWeapon {
            get {
                WeaponReads++;
                if (RejectWeaponRead) throw new InvalidOperationException("Premature WieldedWeapon access");
                return new MissionWeapon { Item = WeaponId == null ? null : new Item { StringId = WeaponId } };
            }
        }
    }
}
