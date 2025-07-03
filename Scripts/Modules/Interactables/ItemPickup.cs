using BrannPack.Character;
using BrannPack.InputHelpers;
using BrannPack.Interactable;
using BrannPack.ItemHandling;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.Interactables
{
    public partial class ItemPickup : BaseInteractable
    {
        private Sprite2D DisplayImage;

        private InventoryItemStack _itemStack;

        public InventoryItemStack ItemStack
        {
            get => _itemStack;
            set
            {
                _itemStack = value;
                UpdateDisplay();
            }
        }

        private void UpdateDisplay()
        {

        }
        public override void Activate(BaseCharacterBody body, string actionKeyName, InputPressState inputPressState)
        {
            if (actionKeyName == "interact1" && inputPressState == InputPressState.JustReleased)
                body.CharacterMaster.Inventory.TryAddItemToInventory(ItemStack);
        }
    }
}
