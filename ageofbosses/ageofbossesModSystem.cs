using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using System.Runtime;
using AgeOfBosses;
using Vintagestory.API.Util;

namespace AgeOfBosses
{


    public class AgeOfBosses : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            AiTaskRegistry.Register<AiTaskExploderAttack>("ageofbossesexploderattack");
            AiTaskRegistry.Register<AiTaskSeekAndReviveDeadEntity>("ageofbossesseekandrevivedeadentity");
            //api.RegisterEntityBehaviorClass ("repairablelocustcommandable", typeof(EntityBehaviorRepairableLocustCommandable));
            //api.RegisterEntity ("EntityRepairableLocust", typeof(EntityRepairableLocust));
            //api.RegisterItemClass("ItemHackedLocustSpawner", typeof(ItemHackedLocustSpawner));
        }
    }
}