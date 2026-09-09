using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace DragonslayerGutsPowers
{
    internal sealed class DragonslayerWielderController : MissionBehavior
    {
        internal const string ItemId = "dragonslayer"; // Verified in the working CraftedItem XML.
        private readonly Dictionary<Agent, Action> subscriptions = new Dictionary<Agent, Action>();
        private readonly HashSet<Agent> powered = new HashSet<Agent>();
        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;
        internal static bool IsWielder(Agent agent)
        {
            var c = DragonslayerConfig.Current;
            return agent != null && WielderPolicy.Eligible(c.EnableGutsPowers, agent.IsHuman,
                agent.IsActive(), c.PlayerOnly, agent.IsPlayerControlled, GameNetwork.IsSessionActive,
                agent.WieldedWeapon.Item?.StringId);
        }
        internal static bool IsAttacking(Agent agent)
        {
            var stage = agent.GetCurrentActionStage(1);
            return stage == Agent.ActionStage.AttackReady || stage == Agent.ActionStage.AttackQuickReady || stage == Agent.ActionStage.AttackRelease;
        }
        public override void OnAgentBuild(Agent agent, Banner banner)
        {
            if (!agent.IsHuman || subscriptions.ContainsKey(agent)) return;
            Action refresh = () => Refresh(agent);
            subscriptions.Add(agent, refresh);
            agent.OnAgentWieldedItemChange += refresh;
            agent.OnAgentMountedStateChanged += refresh;
            Refresh(agent);
        }
        private void Refresh(Agent agent)
        {
            bool on = IsWielder(agent);
            bool changed = on ? powered.Add(agent) : powered.Remove(agent);
            if (changed) DebugLog.Write("Wielder " + agent.Index + " powers=" + on);
            if (agent.IsActive()) agent.UpdateAgentProperties();
        }
        public override void OnAgentControllerSetToPlayer(Agent agent) => Refresh(agent);
        public override void OnAgentRemoved(Agent agent, Agent attacker, AgentState state, KillingBlow blow) => Remove(agent);
        public override void OnAgentDeleted(Agent agent) => Remove(agent);
        private void Remove(Agent agent)
        {
            if (subscriptions.TryGetValue(agent, out var action)) {
                agent.OnAgentWieldedItemChange -= action;
                agent.OnAgentMountedStateChanged -= action;
                subscriptions.Remove(agent);
            }
            powered.Remove(agent);
        }
        public override void OnRemoveBehavior()
        {
            foreach (var pair in subscriptions) {
                pair.Key.OnAgentWieldedItemChange -= pair.Value;
                pair.Key.OnAgentMountedStateChanged -= pair.Value;
            }
            subscriptions.Clear(); powered.Clear();
        }
    }
}
