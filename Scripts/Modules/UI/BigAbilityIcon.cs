using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrannPack.AbilityHandling;
using Godot;

namespace BrannPack.UI
{
    [GlobalClass]
    public partial class BigAbilityIcon : Control
    {
        public enum CooldownDisplayStyle
        {
            CircularFade,
            VerticalBar
        }

        [Export]
        public CooldownDisplayStyle DisplayStyle { get; set; } = CooldownDisplayStyle.CircularFade;

        public AbilitySlot AbilitySlot { get; set; } = null;
        public Texture2D Icon => AbilitySlot?.AbilityInstance?.Icon;
        public float CooldownTime => AbilitySlot?.CCooldown?.RemainingTime ?? 0;
        public float CooldownPercentage => AbilitySlot?.CCooldown?.PercentageComplete ?? 0;

        public override void _Draw()
        {
            if (Icon != null)
            {
                // Draw Icon
                DrawTextureRect(Icon, new Rect2(Vector2.Zero, Size), tile: false, modulate: Colors.White, transpose: false);


                // Draw cooldown overlay
                if (CooldownTime > 0)
                {
                    switch (DisplayStyle)
                    {
                        case CooldownDisplayStyle.CircularFade:
                            DrawCircularCooldownOverlay();
                            break;
                        case CooldownDisplayStyle.VerticalBar:
                            DrawVerticalCooldownOverlay();
                            break;
                    }

                    // Draw remaining time text
                    string seconds = Mathf.CeilToInt(CooldownTime).ToString();
                    var font = GetThemeDefaultFont();
                    var textSize = font.GetStringSize(seconds);
                    var center = Size / 2 - textSize / 2;

                    DrawString(font, center, seconds, HorizontalAlignment.Center, -1, 14, Colors.White);
                }
            }
        }

        public override void _Process(double delta)
        {
            QueueRedraw();
        }

        private void DrawCircularCooldownOverlay()
        {
            float angle = Mathf.Tau * (1 - CooldownPercentage);
            Vector2 center = Size / 2;
            float radius = Mathf.Min(Size.X, Size.Y) / 2;

            var drawColor = new Color(0, 0, 0, 0.6f);
            DrawArc(center, radius, -Mathf.Pi / 2, angle, 32, drawColor, radius);
        }

        private void DrawVerticalCooldownOverlay()
        {
            float height = Size.Y * (1 - CooldownPercentage);
            var rect = new Rect2(0, height, Size.X, Size.Y*.1f );
            DrawRect(rect, new Color(1, 1, 1, 0.5f), filled: true);
        }
    }
}
