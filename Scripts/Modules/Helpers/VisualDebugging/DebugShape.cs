using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.Debugging
{
    [GlobalClass]
    public abstract partial class DebugShape: Node2D
    {
        public static bool IsDebugging => true;
        public float Duration { get; set; } = .5f;
        protected float timeElapsed { get; set; } = 0;
        public Color BaseColor { get; set; } = Color.Color8(255, 255, 255);

        public override void _Process(double delta)
        {
            timeElapsed += (float)delta;
            if (timeElapsed >= Duration)
            {
                QueueFree(); // Remove the line after its lifespan
            }

            QueueRedraw();
        }


    }
    [GlobalClass]
    public partial class DebugLine: DebugShape
    {
        public Vector2 From { get; set; }
        public Vector2 To { get; set; }
        public float Width { get; set; } = 2f;


        public override void _Draw()
        {
            DrawLine(From, To, BaseColor, Width);
        }

        public override void _Ready()
        {
            SetProcess(true); // ensure _Process is running
        }

        public void Initialize(Vector2 from, Vector2 to, Color color, float width = 2f, float duration = 0.5f)
        {
            From = from;
            To = to;
            BaseColor = color;
            Width = width;
            Duration = duration;
            
            QueueRedraw(); // important to trigger initial draw
        }
    }

    [GlobalClass]
    public partial class DebugRect : DebugShape
    {
        public Vector2 Size { get; set; } = Vector2.One;
        public float LineWidth { get; set; } = 2f;

        public bool IsHollow = true;

        public override void _Ready()
        {
            SetProcess(true); // ensure _Process is running
        }

        public void Initialize(Transform2D transform, Vector2 size, Color color, float lineWidth = 2f, float duration = 0.5f, bool isHollow=true)
        {
            Transform = transform;
            Size = size;
            BaseColor = color;
            LineWidth = lineWidth;
            Duration = duration;
            IsHollow = isHollow;

            QueueRedraw(); // trigger drawing
        }

        public override void _Draw()
        {
            Rect2 rect = new Rect2(-Size/2f, Size);
            if(!IsHollow)
                DrawRect(rect, BaseColor, IsHollow, LineWidth);
            else
                DrawRect(rect, BaseColor, IsHollow);
        }

    }

    [GlobalClass]
    public partial class DebugCircle : DebugShape
    {
        public float Radius { get; set; } = 0f;
        public float LineWidth { get; set; } = 2f;

        public bool IsHollow = true;

        public override void _Ready()
        {
            SetProcess(true); // ensure _Process is running
        }

        public void Initialize(Transform2D transform, float radius, Color color, float lineWidth = 2f, float duration = 0.5f, bool isHollow = true)
        {
            Transform = transform;
            Radius = radius;
            BaseColor = color;
            LineWidth = lineWidth;
            Duration = duration;
            IsHollow = isHollow;

            QueueRedraw(); // trigger drawing
        }

        public override void _Draw()
        {

            if (IsHollow)
                DrawCircle(Vector2.Zero, Radius, BaseColor, IsHollow,LineWidth);
            else
                DrawCircle(Vector2.Zero, Radius, BaseColor, IsHollow, LineWidth);
        }

    }

}
