//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using BrannPack.Character;
//using Godot;
//using System.Collections.Generic;

//namespace BrannPack.UI
//{
//    public partial class PlayerFollowerUI : Control
//    {
//        [Export] private CharacterMaster player; // Assign in the editor or dynamically.
//        [Export] private TextureProgressBar healthBar;
//        private List<TextureProgressBar> abilityIcons = new List<TextureProgressBar>();

//        private Camera2D camera;

//        public override void _Ready()
//        {
//            camera = GetViewport().GetCamera2D();
//        }

//        public override void _Process(double delta)
//        {
//            if (player == null || camera == null) return;

//            // Position UI above player
//            Vector2 screenPosition = camera.UnprojectPosition(player.GlobalPosition);
//            Position = screenPosition + new Vector2(0, -50); // Adjust Y-offset to float above the player

//            UpdateHealthBar();
//            UpdateAbilityCooldowns();
//        }

//        private void UpdateHealthBar()
//        {
//            float healthPercent = player.Health / player.MaxHealth;
//            healthBar.Value = healthPercent * 100;
//        }

//        private void UpdateAbilityCooldowns()
//        {
//            for (int i = 0; i < abilityIcons.Count; i++)
//            {
//                var ability = player.Abilities[i];
//                if (ability == null) continue;

//                float cooldownPercent = ability.CurrentCooldown / ability.MaxCooldown;
//                abilityIcons[i].Value = (1 - cooldownPercent) * 100; // Fill when ready

//                // Update stock display
//                int maxStocks = ability.MaxStocks;
//                int currentStocks = ability.CurrentStocks;
//                UpdateStockDisplay(abilityIcons[i], maxStocks, currentStocks);
//            }
//        }

//        private void UpdateStockDisplay(TextureProgressBar icon, int maxStocks, int currentStocks)
//        {
//            if (maxStocks > 1)
//            {
//                float segmentSize = 1f / maxStocks;
//                icon.RadialFill = currentStocks * segmentSize; // Split outer glow into segments
//            }
//        }
//    }

//}
