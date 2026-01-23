using BrannPack.Character;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.UI
{
    [GlobalClass]
    public partial class FixedPlayerInfoBar : Control
    {
        [Export]
        public CharacterMaster CharacterMaster { get; set; } = null;
        [Export]
        public BigAbilityIcon[] AbilityIcons { get; set; }

        public override void _Ready()
        {
            
            if (CharacterMaster == null)
            {
                GD.PrintErr("CharacterMaster is not set for FixedPlayerInfoBar. Please assign a CharacterMaster instance.");
                return;
            }
            for (int i = 0; i < AbilityIcons.Length; i++)
            {
                AbilityIcons[i] = GetNode<BigAbilityIcon>($"AbilityIcon{i}");
                switch(i)
                {

                   case 0:
                        AbilityIcons[i].AbilitySlot = CharacterMaster.Primary;
                        break;
                    case 1:
                        AbilityIcons[i].AbilitySlot = CharacterMaster.Secondary;
                        break;
                    case 2:
                        AbilityIcons[i].AbilitySlot = CharacterMaster.Utility;
                        break;
                    case 3:
                        AbilityIcons[i].AbilitySlot = CharacterMaster.Special;
                        break;
                    case 4:
                        AbilityIcons[i].AbilitySlot = CharacterMaster.Ult;
                        break;
                    case 5:
                        AbilityIcons[i].AbilitySlot = CharacterMaster.Equipment;
                        break;
                    
                }
            }
        }
    }
}
