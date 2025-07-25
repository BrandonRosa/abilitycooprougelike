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

		public abstract void UpdateInput(double delta);

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
		public override void UpdateInput(double delta)
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
			bool useInteract1 = Input.GetActionStrength("interact1_" + OwnerMaster.ControllerID) > 0;

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
			if (useInteract1 && OwnerBody.InteractiveArea != null && OwnerBody.InteractiveArea.GetClosestInteractable() != null)
				OwnerBody.InteractiveArea.GetClosestInteractable().Activate(OwnerBody, "interact1", InputPressState.JustPressed);

			OwnerBody.MoveDirection = inputDirection;
			OwnerBody.AimDirection = aimDirection;
		}
	}
    public enum AIState
    {
        Idle,
        Roam,
        Chase,
        Flee
    }
	public enum AIInfoAquisitionType
	{
        None,Clue,LOS,Command
	}
    public partial class EnemyAIController:EntityController
	{
		public static float LOSInfoDuration=4f;
		public static float GossipClueDuration = 6f;
		public static float CommandDuration = 360f;

		public static float UpdateTargetInterval = 0.5f;
		public static float UpdateMoveDirectionInterval = 0.5f;
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

		public float TimeUntilNextTargetUpdate = UpdateTargetInterval;
		public float TimeUntilNextMoveDirectionUpdate = UpdateMoveDirectionInterval;

		public float EnemyGossipRadius;
		public (float,float) LOSRadius;
		
		public NavigationAgent2D NavigationAgent2D;

        public AIState AIState;
        public AIInfoAquisitionType InfoAquisitionType;
		public float InfoDuration;
        public BaseCharacterBody AcquiredTarget;
        public Vector2? FinalMoveLocation;

		public bool HasLOSThisFrame = false;
		
		public void InitializeNavAgent()
		{
			NavigationAgent2D=new();

			OwnerBody.AddChild(NavigationAgent2D);
		}

		public bool SetState(bool OverrideNaturalOrder, AIState state, AIInfoAquisitionType IAT, float infoDuration, BaseCharacterBody target = null, Vector2? finalMoveLocation = null)
        {
            if(OverrideNaturalOrder)
			{
				AIState= state;
				InfoAquisitionType = IAT;
				InfoDuration = infoDuration;
				AcquiredTarget = target;
				FinalMoveLocation = finalMoveLocation;
				return true;
			}
			else if(IAT>InfoAquisitionType)
			{
				AIState = state;
				InfoAquisitionType = IAT;
				InfoDuration = infoDuration;
				AcquiredTarget = target;
				FinalMoveLocation = finalMoveLocation;
				return true;

			}
			return false;
        }
		public void ObtainTarget()
		{
			List<BaseCharacterBody> AlliesInGossipRange=new();
			List<BaseCharacterBody> TargetsInProximity=new();
			List<BaseCharacterBody> TargetsInLOSRange=new();
			bool isTargetInLOS = false;
			if (InfoDuration <= 0)
			{
				switch (AIState)
				{
					//Update TargetsInProximity and Targets inLOS
					case AIState.Chase:
						if (TargetsInProximity.Count > 0)
						{
							if (TargetsInProximity.Contains(AcquiredTarget))
								InfoDuration = LOSInfoDuration;
							else
							{
								AcquiredTarget = TargetsInProximity[0];
								FinalMoveLocation = AcquiredTarget.GlobalTransform.Origin;
								InfoDuration = LOSInfoDuration;
							}

                            InfoAquisitionType = AIInfoAquisitionType.LOS;
                        }
						else
						{
							AIState=AIState.Roam;
							AcquiredTarget = null;
							FinalMoveLocation = null;//Make a wander script
							InfoDuration = LOSInfoDuration;
							InfoAquisitionType= AIInfoAquisitionType.None;
						}
						break;
					case AIState.Roam:
						if (TargetsInLOSRange.Count > 0)
						{
							AcquiredTarget = TargetsInLOSRange[0];
							FinalMoveLocation = AcquiredTarget.GlobalTransform.Origin;
							InfoDuration = LOSInfoDuration;
							AIState = AIState.Chase;
						}
						break;
					case AIState.Flee:
						AIState = AIState.Roam;
						AcquiredTarget = null;
						FinalMoveLocation = null;
						//Set roaming location...
						break;
				}
			}
			if(AcquiredTarget!=null && AIState==AIState.Chase)
			{
				foreach(var ally in AlliesInGossipRange)
				{
					((EnemyAIController)ally.Controller)?.SetState(false, AIState, AIInfoAquisitionType.Clue, GossipClueDuration, AcquiredTarget, FinalMoveLocation);
				}
			}
		}
		public void UpdateMoveLocation()
		{
			switch(AIState)
			{ 
                case AIState.Idle:
                    OwnerBody.MoveDirection = Vector2.Zero;
                    break;
                case AIState.Roam:
                    if (FinalMoveLocation.HasValue)
                    {
                        OwnerBody.MoveDirection = (NavigationAgent2D.GetNextPathPosition() - OwnerBody.GlobalPosition).Normalized(); // Move towards the next path position in the navigation agent.
                    }
                    else
                    {
                        OwnerBody.MoveDirection = Vector2.Zero; // No final move location, stop moving.
                    }
                    break;
                case AIState.Chase:
					if (AcquiredTarget != null)
					{
                        NavigationAgent2D.SetTargetPosition(AcquiredTarget.GlobalPosition);
						OwnerBody.MoveDirection = (NavigationAgent2D.GetNextPathPosition() - OwnerBody.GlobalPosition).Normalized();
                    }
					else if (FinalMoveLocation.HasValue) // If no target, but a final move location is set, move towards it.
					{
                        NavigationAgent2D.SetTargetPosition(FinalMoveLocation.Value);
                        OwnerBody.MoveDirection = (NavigationAgent2D.GetNextPathPosition() - OwnerBody.GlobalPosition).Normalized();
                    }
					else
					{
						OwnerBody.MoveDirection = Vector2.Zero; // No target, stop moving.
						AIState = AIState.Idle;
						InfoAquisitionType = AIInfoAquisitionType.None; // Reset info acquisition type since we're idle.
						InfoDuration = 0; // Reset info duration since we're idle.
					}
                    break;
                case AIState.Flee:
                    if (AcquiredTarget != null)
                    {
                        Vector2 fleeDirection = (OwnerBody.GlobalPosition - AcquiredTarget.GlobalPosition).Normalized();
                        OwnerBody.MoveDirection = fleeDirection;
                    }
                    else
                    {
                        OwnerBody.MoveDirection = Vector2.Zero; // No target, stop moving.
                    }
                    break;

			}
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

		public override void UpdateInput(double delta)
		{
			TimeUntilNextTargetUpdate -= (float)delta;
            TimeUntilNextMoveDirectionUpdate -= (float)delta;
			InfoDuration-= (float)delta;
			if (InfoDuration <= 0)
				ObtainTarget();
			if(TimeUntilNextTargetUpdate <= 0)
			{
				   TimeUntilNextTargetUpdate = UpdateTargetInterval;
			}
            TryUseAbilities();
            
        }
	}
}
