using BrannPack.Character;
using BrannPack.InputHelpers;
using BrannPack.ItemHandling;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.Interactables
{
    [GlobalClass]
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

        public override void _Ready()
        {
            base._Ready();
            DisplayImage = new Sprite2D();
            AddChild(DisplayImage);
            SetCircleInteractable(100f);
        }

        private void UpdateDisplay()
        {
            if (DisplayImage == null)
            {
                DisplayImage = new Sprite2D();
                AddChild(DisplayImage);
            }
            DisplayImage.Texture = _itemStack.Item.WorldTexture;
            GD.Print("ITEM_DISPLAYUPDATED:" + _itemStack.Item.Name);
        }
        public override void Activate(BaseCharacterBody body, string actionKeyName, InputPressState inputPressState)
        {
            base.Activate(body,actionKeyName,inputPressState);
            GD.Print("ATTEMTP ACTIVATE");
            if (actionKeyName == "interact1" && inputPressState == InputPressState.JustPressed)
                body.CharacterMaster.Inventory.TryAddItemToInventory(ItemStack);
        }
    }
}
