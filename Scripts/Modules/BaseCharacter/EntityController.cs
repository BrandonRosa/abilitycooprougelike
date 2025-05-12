using BrannPack.InputHelpers;
using Godot;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public override void UpdateInput()
        {
            Vector2 inputDirection;
            inputDirection.X = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
            inputDirection.Y = Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up");
            bool usePrimary = Input.GetActionStrength("use_primary")>0;
            bool useSecondary = Input.GetActionStrength("use_secondary") > 0;
            bool useUtility = Input.GetActionStrength("use_utility") > 0;
            bool useSpecial = Input.GetActionStrength("use_special") > 0;
            bool useUlt = Input.GetActionStrength("use_ult") > 0;

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
        }
    }

    public partial class AIController:EntityController
    {
        public NavigationAgent2D NavigationAgent2D;
        public Node LimboHSM;
        public float AggroRange = 10f;
        public void ObtainTarget()
        {

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
            //AI logic to determine movement and ability usage
            //For now, just move towards the player
            if (LimboHSM != null)
            {
                
            }
        }
    }
}
