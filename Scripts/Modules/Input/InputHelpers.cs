using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.InputHelpers
{
    public enum InputPressState
    {
        None,
        JustPressed,
        Pressing,
        JustReleased
    }

    public static class InputHelper
    {
        public static InputPressState GetPressState(string action)
        {
            if (Input.IsActionJustPressed(action))
                return InputPressState.JustPressed;
            else if (Input.IsActionJustReleased(action))
                return InputPressState.JustReleased;
            else if (Input.IsActionPressed(action))
                return InputPressState.Pressing;
            return InputPressState.None;
        }
    }
}
