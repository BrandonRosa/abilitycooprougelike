using Godot;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.Character
{
    public abstract partial class EntityController: GodotObject
    {
        public BaseCharacter Owner;

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
                Owner.Primary.TryUseAbility();
            if (useSecondary)
                Owner.Secondary.TryUseAbility();
            if (useUtility)
                Owner.Utility.TryUseAbility();
            if (useSpecial)
                Owner.Special.TryUseAbility();
            if (useUlt)
                Owner.Ult.TryUseAbility();

            Owner.MoveDirection = inputDirection;
        }
    }

    public abstract partial class AIController:EntityController
    {
        public abstract void ObtainTarget();
    }
}
