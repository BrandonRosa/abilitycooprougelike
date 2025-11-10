using BrannPack.Character;
using BrannPack.GameDirectrs;
using BrannPack.InputHelpers;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BrannPack.GameDirectrs.GameDirector;

namespace BrannPack.Interactables
{
    [GlobalClass]
    public partial class BaseInteractable : Area2D, IInteractable
    {
        public Action<BaseCharacterBody, string, InputPressState> OnActivate;
        public static Action<BaseInteractable, BaseCharacterBody, string, InputPressState> OnGlobalActivate;
        public bool IsEnabled { get; set; } = false;
        public override void _Ready()
        {
            base._Ready();
            CollisionMask = GameDirector.CollisionLayers.Interactable;
            CollisionLayer = GameDirector.CollisionLayers.Interactable;
           
            
        }
        public virtual void SetCircleInteractable(float radius, bool replaceAllOthers = true)
        {
            if (replaceAllOthers)
            {
                foreach (var child in GetChildren())
                {
                    if (child is CollisionShape2D shape)
                    {
                        shape.QueueFree(); // or shape.Remove() if using a custom method
                    }
                }
            }
            AddChild(new CollisionShape2D() { Shape = new CircleShape2D() { Radius = radius } });
        }
        public virtual void Activate(BaseCharacterBody body, string actionKeyName, InputPressState inputPressState)
        {
            OnActivate?.Invoke(body, actionKeyName, inputPressState);
            OnGlobalActivate?.Invoke(this, body, actionKeyName, inputPressState);
        }
    }
}
