using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;
namespace DragonslayerGutsPowers
{
    public sealed class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad() { base.OnSubModuleLoad(); DragonslayerConfig.Load(); }
        protected override void OnGameStart(Game game, IGameStarter starter)
        {
            base.OnGameStart(game, starter);
            if (!(game.GameType is Campaign) || !DragonslayerConfig.Current.EnableGutsPowers) return;
            if (!starter.Models.OfType<AgentStatCalculateModel>().Any() || !starter.Models.OfType<AgentApplyDamageModel>().Any()) {
                DebugLog.Write("Missing native models; no powers registered.", true); return;
            }
            // Explicit generic overload binds BaseModel to the prior model in v1.4.8.
            starter.AddModel<AgentStatCalculateModel>(new GutsStatModel());
            starter.AddModel<AgentApplyDamageModel>(new GutsDamageModel());
            DebugLog.Write("Phase 1 native model decorators registered.", true);
        }
        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            if (Game.Current?.GameType is Campaign && !GameNetwork.IsSessionActive
                && mission.GetMissionBehavior<DragonslayerWielderController>() == null)
                mission.AddMissionBehavior(new DragonslayerWielderController());
        }
    }
}
