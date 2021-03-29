using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace FireArrow
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
        }

        public override void OnMissionBehaviourInitialize(Mission mission)
        {
            mission.AddMissionBehaviour(new FireArrow());
        }
        /*protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            base.OnGameStart(game, gameStarter);
            CampaignGameStarter campaignGameStarter = (CampaignGameStarter)gameStarter;
            campaignGameStarter.AddBehavior();
        }*/
    }
}
