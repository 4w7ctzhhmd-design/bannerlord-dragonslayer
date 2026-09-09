using System;
using System.IO;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace DragonslayerInstantKill
{
    public sealed class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            Log.Write("Loaded optional Instant Kill 0.1.0; target v1.4.8.119303.");
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            if (!(Game.Current?.GameType is Campaign) || GameNetwork.IsSessionActive) return;
            if (mission.GetMissionBehavior<InstantKillBehavior>() == null)
                mission.AddMissionBehavior(new InstantKillBehavior());
        }
    }

    internal sealed class InstantKillBehavior : MissionBehavior
    {
        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        public override void OnAgentHit(Agent victim, Agent attacker, in MissionWeapon weapon,
            in Blow blow, in AttackCollisionData collision)
        {
            if (Mission.MissionEnded || Mission.DisableDying || Mission.Mode == MissionMode.Conversation
                || Mission.Mode == MissionMode.CutScene) return;
            if (!HitPolicy.ShouldKill(weapon.Item?.StringId, victim?.IsHuman == true,
                victim != null && victim.IsActive() && victim.Health > 0f,
                attacker != null && attacker != victim,
                collision.IsColliderAgent && collision.CollisionResult == CombatCollisionResult.StrikeAgent,
                collision.AttackBlockedWithShield || collision.CollidedWithShieldOnBack,
                collision.IsAlternativeAttack, collision.IsMissile || blow.IsMissile,
                collision.IsFallDamage, collision.IsHorseCharge)) return;

            // v1.4.8 Agent.RegisterBlow calls OnAgentHit, then checks Health < 1 and
            // calls Die with the ORIGINAL blow. Do not recursively call RegisterBlow
            // or Die from this callback: let native attribution/removal finish once.
            var oldHealth = victim.Health;
            victim.Health = 0f;
            Log.Write($"Lethal hit: attacker={attacker.Index}, victim={victim.Index}, remainingHp={oldHealth:0.##}, item=dragonslayer");
        }
    }

    internal static class Log
    {
        internal static void Write(string message)
        {
            try
            {
                var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Dragonslayer", "Logs");
                Directory.CreateDirectory(directory);
                var path = Path.Combine(directory, "InstantKill.log");
                if (File.Exists(path) && new FileInfo(path).Length > 1024 * 1024)
                {
                    File.Copy(path, path + ".previous", true);
                    File.WriteAllText(path, "");
                }
                File.AppendAllText(path, DateTime.UtcNow.ToString("O") + " " + message + Environment.NewLine);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
