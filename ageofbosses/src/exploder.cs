using System;
using System.Text;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using System.Runtime;
using AgeOfBosses;



public class AiTaskExploderAttack : AiTaskMeleeAttack
{
    public AiTaskExploderAttack(EntityAgent entity, JsonObject taskConfig, JsonObject aiConfig)
        : base(entity, taskConfig, aiConfig)
    {
    }

    protected override void attackTarget()
    {
        base.attackTarget();

        if (entity.World.Side != EnumAppSide.Server) return;
        if (!entity.Alive) return;

        var serverWorld = entity.World as IServerWorldAccessor;
        if (serverWorld == null) return;

        //Hard-coded explosion values
        float blastRadius = 3f;      //Visual I think, scrap is set to 4
        float injureRadius = 15f;     //damage radius, scrap is set to 15, other bombs to 10
        float damage = 10f; //not actually sure how this is applying, seems scaled but odd?

        serverWorld.CreateExplosion(
            entity.Pos.AsBlockPos,
            (EnumBlastType)2,//2 is no block damage
            blastRadius,
            injureRadius,
            damage,
            null//No player attribution
        );
        entity.Die();
    }
}