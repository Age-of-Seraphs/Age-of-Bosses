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

public class AiTaskSeekAndReviveDeadEntity : AiTaskBase
{
	private float seekRange;
	private float moveSpeed;
	private Entity target;
	private HashSet<string> whitelist;
	private AssetLocation reviveSound;

	private Vec3d lastPos;

	private bool goalReached;
	private bool pathStuck;
	private int stuckCount;

	public AiTaskSeekAndReviveDeadEntity(EntityAgent entity, JsonObject taskConfig, JsonObject aiConfig)
		: base(entity, taskConfig, aiConfig)
	{
		seekRange = taskConfig["searchRange"].AsFloat();
		moveSpeed = taskConfig["movespeed"].AsFloat();
		string soundPath = taskConfig["reviveSound"].AsString(null);
		if (soundPath != null)
		{
			reviveSound = new AssetLocation(soundPath);
		}


		whitelist = new HashSet<string>();//ONly allow things from the JSON whitelist
		if (taskConfig["entityCodes"] != null)
		{
			foreach (var code in taskConfig["entityCodes"].AsArray())
			{
				whitelist.Add(code.AsString());
			}
		}
	}

	public override bool ShouldExecute()
	{
		if (entity.World.Side != EnumAppSide.Server) return false;
		if (entity.World.ElapsedMilliseconds < cooldownUntilMs) return false;

		target = null;
		float nearestDistSq = seekRange * seekRange;

		entity.World.GetEntitiesAround(entity.Pos.XYZ, seekRange, seekRange, (Entity potential) =>
		{
			if (potential.Alive) return false;
			if (!whitelist.Contains(potential.Code.ToString())) return false;

			float distSq = (float)entity.Pos.SquareDistanceTo(potential.Pos);
			if (distSq < nearestDistSq)
			{
				target = potential;
				nearestDistSq = distSq;
			}

			return false;
		});

		return target != null;
	}

	public override void StartExecute()
	{
		base.StartExecute();

		goalReached = false;
		pathStuck = false;

		lastPos = entity.Pos.XYZ.Clone();

		pathTraverser.NavigateTo_Async(
			target.ServerPos.XYZ,
			moveSpeed,
			1f,//HOw close it has to get
			OnGoalReached,
			OnStuck,
			null,
			999,
			0,
			null
		);
		stuckCount = 0;

		entity.Api.Logger.Debug("[QueenDebug] Path started.");
	}

	public override bool ContinueExecute(float dt)
	{
		if (target == null) return false;

		//If navigator says we reached goal
		if (goalReached)
		{
			
			target.Revive();
			if (reviveSound != null)
			{
				entity.World.PlaySoundAt(reviveSound, entity);
			}
			entity.Api.Logger.Debug("[QueenDebug] Target revived.");
			return false;
		}

		//If navigator says stuck ABORT task and revive / teleport locust to us to prevent chesing between rooms and such
		if (pathStuck)
		{
			entity.Api.Logger.Debug("[QueenDebug] Navigator repeatedly stuck aborting and reviving target!");
			target.Revive();// doing a "cheeky revive" even if the queen fails to get to the locust so we're not stuck targeting it again
			target.ServerPos.SetPos(entity.ServerPos.X, entity.ServerPos.Y, entity.ServerPos.Z);
			//entity.World.PlaySoundAt(new AssetLocation("repairablelocust:sounds/automatonrepair"), entity);
			if (reviveSound != null)
			{
				entity.World.PlaySoundAt(reviveSound, entity);
			}
			return false;
		}

		return true;
	}

	public override void FinishExecute(bool cancelled)
	{
		base.FinishExecute(cancelled);
		pathTraverser.Stop();

		target = null;
		goalReached = false;
		pathStuck = false;

		entity.Api.Logger.Debug("[QueenDebug] Task finished.");
	}

	private void OnGoalReached()
	{
		goalReached = true;
	}

	private void OnStuck()
	{
		stuckCount++;

		entity.Api.Logger.Debug($"[QueenDebug] Navigator stuck attempt {stuckCount}");

		if (stuckCount >= 3)
		{
			pathStuck = true;//tell ContinueExecute to abort
			return;
		}

		//Retry path
		pathTraverser.NavigateTo_Async(
			target.ServerPos.XYZ,
			0.02f,
			1f,//HOw close it has to get
			OnGoalReached,
			OnStuck,
			null,
			999,
			0,
			null
		);
	}
}
