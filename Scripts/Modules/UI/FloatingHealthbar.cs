using BrannPack.AbilityHandling;
using BrannPack.Character;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BrannPack.ModifiableStats.CharacterStats;

namespace BrannPack.UI
{
    public partial class FloatingHealthBar : Control
    {
        private Dictionary<(HealthType healthType, bool isOverHealth), ColorRect> healthSegments = new();
        private BaseCharacterBody ownerBody => (Owner is BaseCharacterBody bcb) ? bcb : null;
        private CharacterMaster owner=>ownerBody?.CharacterMaster;
        private float maxWidth = 100f; // Max width of the health bar

        public override void _Ready()
        {

            InitializeHealthBar();
            SetAnchorsPreset(Control.LayoutPreset.TopLeft); // No stretch
            Position = Vector2.Zero;
            Size = new Vector2(100, 20);
            CustomMinimumSize = Size;
        }
        public override void _Process(double delta)
        {
            if (ownerBody != null)
            {
                // Adjust the Y offset to float above the character's head
                GlobalPosition = ownerBody.GlobalPosition + new Vector2(0, -40);
            }
        }

        private void InitializeHealthBar()
        {
            CreateBackground();
           // owner.HealthBar.UIHealthUpdated += UpdateHealthBar;

            CreateHealthSegment((HealthType.Health, false), new Color(0f, 0.8f, 0f));
            //CreateHealthSegment("CelledHealth", new Color(0.5f, 1f, 0.5f));
            //CreateHealthSegment("CursedHealth", new Color(0f, 0f, 0f, 0f), true);
            CreateHealthSegment((HealthType.Armor, true), new Color(0.6f, 0.6f, 0.6f));
            CreateHealthSegment((HealthType.Armor, false), new Color(0.3f, 0.3f, 0.3f), true);
            CreateHealthSegment((HealthType.Shield, false), new Color(0f, 0.3f, 0.8f));
            //CreateHealthSegment("CelledShield", new Color(0.3f, 0.5f, 1f));
            //CreateHealthSegment("Guard", new Color(0f, 0.9f, 1f));
            CreateHealthSegment((HealthType.Barrier, true), new Color(1f, 0.8f, 0f, 0.3f));
            CreateHealthSegment((HealthType.Barrier, false), new Color(1f, 0.8f, 0f));
        }

        private void CreateHealthSegment((HealthType healthType, bool isOverHealth) key, Color color, bool isHollow = false)
        {
            var segment = new ColorRect();
            segment.Color = color;
            AddChild(segment);
            healthSegments[key] = segment;
            if (isHollow)
            {
                //segment.Material = new ShaderMaterial { Shader = (Shader)GD.Load("res://shaders/hollow_border.shader") };
            }
        }

        private void CreateBackground()
        {
            var segment = new ColorRect();
            segment.Color =new Color(0,0,0);
            AddChild(segment);

        }


        public void UpdateHealthBar()
        {
            if(owner?.HealthBar?.UIInfo == null)
                return;
            
            foreach (var var in owner.HealthBar.UIInfo)
            {
                float startx = var.startPosition * maxWidth / owner.HealthBar.CurrentMaxVisible;
                float width = var.width * maxWidth / owner.HealthBar.CurrentMaxVisible;
                if (width>0 && startx>=0)
                ArrangeSegment((var.type, var.isOverHealth), startx ,width );
            }
        }

        private void ArrangeSegment((HealthType, bool) key, float startX, float width)
        {
            if (healthSegments.TryGetValue(key, out var segment))
            {

                segment.Position = new Vector2(startX, 0);
                segment.Size = new Vector2(width, 10);
                segment.Visible = width > 0;

                //if (overlay)
                //    segment.Modulate = new Color(segment.Modulate.r, segment.Modulate.g, segment.Modulate.b, 0.3f);
            }
        }
    }
    public partial class MiniCooldownIcon : Control
    {
        AbilitySlot abilitySlot;
        float CooldownProgress => abilitySlot.CCooldown.PercentageComplete;
        int MaxIntCharges => (int)abilitySlot.CCooldown.TrackedMaxCharges.CalculateTotal();

        int CurrentIntCharges => (int)abilitySlot.CCooldown.CurrentCharges;
        int OldIntCharges=0;

        private const float CircleRadius = 12f;
        private const float ArcRadius = 16f;
        private const float ArcDegrees = 130f;
        private const float ArcGap = 5f;

        public override void _Draw()
        {
            Vector2 center = Size / 2;

            // Draw main dark circle
            DrawCircle(center, CircleRadius, new Color(0.1f, 0.1f, 0.1f)); // Dark gray

            // Cooldown fill (simulate radial fill with segments)
            DrawCooldownFill(center, CircleRadius - 1, 32, CooldownProgress);

            if (OldIntCharges != CurrentIntCharges)
            {
                // Draw charge arcs
                DrawChargeArcs(center, ArcRadius, ArcDegrees, MaxIntCharges, CurrentIntCharges);
                OldIntCharges = CurrentIntCharges;
            }
        }

        private void DrawCooldownFill(Vector2 center, float radius, int segments, float progress)
        {
            if (progress <= 0f) return;
            float anglePer = Mathf.Tau / segments;
            int filledSegments = (int)(segments * (1 - progress));

            for (int i = 0; i < filledSegments; i++)
            {
                float angle = -Mathf.Pi / 2 + i * anglePer;
                Vector2 from = center;
                Vector2 to1 = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                Vector2 to2 = center + new Vector2(Mathf.Cos(angle + anglePer), Mathf.Sin(angle + anglePer)) * radius;

                DrawPolygon(new Vector2[] { from, to1, to2 }, new Color[] { Colors.White });
            }
        }

        private void DrawChargeArcs(Vector2 center, float radius, float totalAngle, int maxCharges, int currentCharges)
        {
            if (maxCharges <= 0) return;

            float arcPer = totalAngle / Mathf.Max(maxCharges, 1);
            float startAngle = -Mathf.DegToRad(totalAngle) / 2;

            for (int i = 0; i < maxCharges; i++)
            {
                float angle = startAngle + Mathf.DegToRad(i * arcPer);
                Color arcColor = i < currentCharges ? Colors.White : new Color(0.5f, 0.5f, 0.5f);
                DrawArc(center, radius, angle, Mathf.DegToRad(arcPer - ArcGap), 8, arcColor, 2f);
            }
        }

        public override void _Process(double delta)
        {
            if(!abilitySlot.CCooldown.IsPaused)
                QueueRedraw(); // Redraw each frame for cooldown updates
        }
    }
}
