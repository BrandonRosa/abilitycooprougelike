using BrannPack.AbilityHandling;
using BrannPack.InputHelpers;
using Godot;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using static Godot.SkeletonProfile;

namespace BrannPack.Character
{
	public abstract partial class EntityController: Node
	{
		public BaseCharacterBody OwnerBody;
		public CharacterMaster OwnerMaster;

		public abstract void UpdateInput();

	}

	public partial class LocalPlayerController:EntityController
	{
		public override void _Input(InputEvent @event)
		{
			base._Input(@event);
			
		}
		public void UpdateInput(InputEvent @event)
		{
			if (@event.Device != OwnerMaster.ControllerID)
				return;
			Vector2 aimDirection = OwnerBody.AimDirection;
			if (@event.Device == 0 && GetViewport()!=null)
			{
				
				Vector2 mouseWorld = GetViewport().GetCamera2D().GetGlobalMousePosition();
				aimDirection = (mouseWorld - OwnerBody.GlobalPosition).Normalized();
				GD.Print("AIM" + aimDirection);
			}
			else
			{

			}


			Vector2 inputDirection;
			inputDirection.X = @event.GetActionStrength("move_right") - @event.GetActionStrength("move_left");
			inputDirection.Y = @event.GetActionStrength("move_down") - @event.GetActionStrength("move_up");
			bool usePrimary = @event.GetActionStrength("use_primary") > 0;
			bool useSecondary = @event.GetActionStrength("use_secondary") > 0;
			bool useUtility = @event.GetActionStrength("use_utility") > 0;
			bool useSpecial = @event.GetActionStrength("use_special") > 0;
			bool useUlt = @event.GetActionStrength("use_ult") > 0;

			if (usePrimary)
				OwnerMaster.Primary.TryUseAbility(InputPressState.JustPressed);
			if (useSecondary)
				OwnerMaster.Secondary.TryUseAbility(InputPressState.JustPressed);
			if (useUtility)
				OwnerMaster.Utility.TryUseAbility(InputPressState.JustPressed);
			if (useSpecial)
				OwnerMaster.Special.TryUseAbility(InputPressState.JustPressed);
			if (useUlt)
				OwnerMaster.Ult.TryUseAbility(InputPressState.JustPressed);

			OwnerBody.MoveDirection = inputDirection;
			OwnerBody.AimDirection = aimDirection;
		}
		public override void UpdateInput()
		{
			//if (Input.Device != OwnerMaster.ControllerID)
			//    return;
			Vector2 aimDirection = OwnerBody.AimDirection;
			if (OwnerBody.GetViewport() != null)
			{

				Vector2 mouseWorld = OwnerBody.GetViewport().GetCamera2D().GetGlobalMousePosition();
				aimDirection = (mouseWorld - OwnerBody.GlobalPosition).Normalized();
			}
			else
			{

			}

			Vector2 inputDirection;
			inputDirection.X = Input.GetActionStrength("move_right_"+OwnerMaster.ControllerID) - Input.GetActionStrength("move_left_" + OwnerMaster.ControllerID);
			inputDirection.Y = Input.GetActionStrength("move_down_" + OwnerMaster.ControllerID) - Input.GetActionStrength("move_up_" + OwnerMaster.ControllerID);
			bool usePrimary = Input.GetActionStrength("use_primary_" + OwnerMaster.ControllerID) > 0;
			bool useSecondary = Input.GetActionStrength("use_secondary_" + OwnerMaster.ControllerID) > 0;
			bool useUtility = Input.GetActionStrength("use_utility_" + OwnerMaster.ControllerID) > 0;
			bool useSpecial = Input.GetActionStrength("use_special_" + OwnerMaster.ControllerID) > 0;
			bool useUlt = Input.GetActionStrength("use_ult_" + OwnerMaster.ControllerID) > 0;

			if (usePrimary)
				OwnerMaster.Primary.TryUseAbility(InputPressState.JustPressed);
			if (useSecondary)
				OwnerMaster.Secondary.TryUseAbility(InputPressState.JustPressed);
			if (useUtility)
				OwnerMaster.Utility.TryUseAbility(InputPressState.JustPressed);
			if (useSpecial)
				OwnerMaster.Special.TryUseAbility(InputPressState.JustPressed);
			if (useUlt)
				OwnerMaster.Ult.TryUseAbility(InputPressState.JustPressed);

			OwnerBody.MoveDirection = inputDirection;
			OwnerBody.AimDirection = aimDirection;
		}
	}
    public enum AIState
    {
        Idle,
        Chase,
        Attack,
        Flee,
        Patrol,
		Assist
    }
    public partial class EnemyAIController:EntityController
	{
		public static Vector2? GlobalSwarmLocation;
		public static bool WouldAIUseAbility(EnemyAIController controller,AbilitySlot abilitySlot,float targetDistance,float currentHealthPercent,bool hasLOS)
		{
			var ability = abilitySlot.AbilityInstance;
			var hint = ability.AbilityHint;

			return ((hasLOS || !hint.RequiresLOS) 
				&& abilitySlot.IsUsable 
				&& (!hint.IsPanicButton || controller.AIState == AIState.Flee) 
				&& (hint.UseRangeOverrideBounds!=null?
					(hint.UseRangeOverrideBounds?.Min<=targetDistance && hint.UseRangeOverrideBounds?.Max >= targetDistance) 
					:(hint.RangeUseMultiplierBounds!=null?(hint.RangeUseMultiplierBounds?.Min<=targetDistance && hint.RangeUseMultiplierBounds?.Max>=targetDistance):true))
				&& (hint.HealthPercentBounds==null || (hint.HealthPercentBounds?.Min<= currentHealthPercent && hint.HealthPercentBounds?.Max>= currentHealthPercent)));
		}

		public float EnemyProximityRadius;
		public float GossipRadius;
		public float LOSRadius;
		public bool IsLeader;

		public NavigationAgent2D NavigationAgent2D;
		public BaseCharacterBody AcquiredTarget;
		public Vector2? FinalMoveLocation;
		public EnemyAIController ClosestLeadingLeader;
		public bool HasLOSThisFrame = false;
		public (Vector2 location, float duration)? AccurateLocationInfo;
		public (Vector2 location, float duration)? EstimatedLocationInfo;
		public (Vector2 location, float duration)? OverrideLocationInfo;
		public AIState AIState;
		public void ObtainTarget()
		{
			List<BaseCharacterBody> AlliesInProximity;
			List<BaseCharacterBody> AlliesInGossipRange;
			List<BaseCharacterBody> TargetsInProximity=new();
			List<BaseCharacterBody> TargetsInLOSRange=new();
			bool isTargetInLOS = false;
			//Check To See if target is still valid
			if (AcquiredTarget != null && !TargetsInProximity.Contains(AcquiredTarget) && !TargetsInLOSRange.Contains(AcquiredTarget))
			{
				AcquiredTarget = null;
			}
			//else if (IsTargetInLOS())
			//{
			//	isTargetInLOS = true;
   //             //set AccurateLocation to target Location
   //         }
			else if(/*Check if target is in proximity*/false)
			{
				//Set estimated location to target location with offset
			}
			else if (/*get first target in TargetsInLOSRange that are in LOS*/ false)
			{
				//set new target and set isTargetInLOS to true
				//set AccurateLocation to target Location
			}
			else if (TargetsInProximity.Count > 0)
			{
				//pick a random target
				//Get target location and randomize it a bit to make it a guess
			}

			if(AcquiredTarget!=null && IsLeader)
			{
				//Get all allies within gossip range and if they dont have a valid leader, set this as the leader
			}

				Vector2? updatedFinalMove = null;
			if (OverrideLocationInfo != null)
			{
				//Go to override Location
				updatedFinalMove = OverrideLocationInfo?.location;

			}
			else if (AccurateLocationInfo!=null)
			{
                //set final move to accurate location
                
                //Update accuratelocation for nearby allies within gossip radius that have the same target

            }
			else if(ClosestLeadingLeader!=null /*also check if CLL has the same target or if this has no target*/)
			{
				//if ClosestLeadingLeader (CLL) has AccurateLocationInfo, copy it. else if it has Estimated copy it
			}
			else if(GlobalSwarmLocation!=null)
			{
				//if ther ii a global swarm location, go there
			}
			else if (EstimatedLocationInfo != null)
			{
				// Go to estimated location
			}

			if (IsLeader && FinalMoveLocation != null)
			{
				//Update ClosestLeadingLeader for all allies in proximity
			}

			// if final location hasnt been set, set the AIState to Patrol for 1.5s and set a random location
			//if target is in any of the abilities range, set to attack if character can
			//if there is a location in mind set to chase and move there.
		}

		public void TryUseAbilities()
		{
            float targetDistance = 0;
            float currentHealthPercent = 1f;
            bool hasLOS = true;

            foreach (AbilitySlot abilitySlot in new List<AbilitySlot>{OwnerMaster.Primary,OwnerMaster.Secondary,OwnerMaster.Utility,OwnerMaster.Special,OwnerMaster.Ult,OwnerMaster.Equipment})
			{
				if (WouldAIUseAbility(this, abilitySlot, targetDistance, currentHealthPercent, hasLOS))
					abilitySlot.TryUseAbility(InputPressState.Pressing);
			}
		}

		public bool TryUsePrimary()
		{
			return OwnerMaster.Primary.TryUseAbility(InputPressState.JustPressed);
		}

		public bool TryUseSecondary()
		{
			return OwnerMaster.Secondary.TryUseAbility(InputPressState.JustPressed);
		}

		public bool TryUseUtility()
		{
			return OwnerMaster.Utility.TryUseAbility(InputPressState.JustPressed);
		}

		public bool TryUseSpecial()
		{
			return OwnerMaster.Special.TryUseAbility(InputPressState.JustPressed);
		}

		public bool TryUseUlt()
		{
			return OwnerMaster.Ult.TryUseAbility(InputPressState.JustPressed);
		}

		public bool TryUseEquipment()
		{
			return OwnerMaster.Equipment.TryUseAbility(InputPressState.JustPressed);
		}

		public void MoveInNavDirection()
		{
			OwnerBody.MoveDirection = (NavigationAgent2D.GetNextPathPosition() - OwnerBody.GlobalPosition).Normalized();
		}

		public override void UpdateInput()
		{
			TryUseAbilities();
		}
	}
}
