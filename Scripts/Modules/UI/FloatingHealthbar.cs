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
        private Dictionary<(HealthType healthType,bool isOverHealth), ColorRect> healthSegments = new();
        private CharacterMaster owner;
        private float maxWidth = 100f; // Max width of the health bar

        public override void _Ready()
        {
            owner = GetParent<CharacterMaster>();
            InitializeHealthBar();
        }

        private void InitializeHealthBar()
        {
            owner.HealthBar.UIHealthUpdated += UpdateHealthBar;

            CreateHealthSegment((HealthType.Health,false), new Color(0f, 0.8f, 0f));
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
                segment.Material = new ShaderMaterial { Shader = (Shader)GD.Load("res://shaders/hollow_border.shader") };
            }
        }


        private void UpdateHealthBar()
        {
            foreach(var var in owner.HealthBar.UIInfo)
            {
                ArrangeSegment((var.type, var.isOverHealth), var.startPosition*maxWidth/owner.HealthBar.CurrentMaxVisible, var.width * maxWidth / owner.HealthBar.CurrentMaxVisible);
            }
        }

        private void ArrangeSegment((HealthType,bool) key, float startX, float width)
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
