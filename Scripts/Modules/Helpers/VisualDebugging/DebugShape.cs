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
}
