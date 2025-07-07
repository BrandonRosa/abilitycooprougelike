using BrannPack.Character;
using BrannPack.GameDirectrs;
using BrannPack.ItemHandling;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.Interactables
{
    public partial class PlayerInteractiveArea:Area2D
    {
        private readonly HashSet<IInteractable> interactablesInRange = new();

        protected CollisionShape2D ColShape;

        public float Radius
        {
            get => (ColShape.Shape as CircleShape2D).Radius;
            set
            {
                if(ColShape==null)
                    ColShape = new CollisionShape2D() { Shape = (new CircleShape2D() { Radius = value }) };
                else
                    ColShape.Shape = new CircleShape2D() { Radius = value };
            }
        }
        public override void _Ready()
        {
            AreaEntered += OnBodyEntered;
            AreaExited += OnBodyExited;
            Radius = 100f;
            CollisionMask = GameDirector.CollisionLayers.Interactable;
            CollisionLayer = GameDirector.CollisionLayers.Interactable;
            AddChild(ColShape);
        }

        private void OnBodyEntered(Node2D node)
        {
            GD.Print("ENTERED");
            if (node is IInteractable interactable)
            {
                interactablesInRange.Add(interactable);
                interactable.OnEnterInteractRange(GetParent<BaseCharacterBody>());
            }
        }

        private void OnBodyExited(Node2D node)
        {
            GD.Print("EXIT");
            if (node is IInteractable interactable)
            {
                interactablesInRange.Remove(interactable);
                interactable.OnExitInteractRange(GetParent<BaseCharacterBody>());
            }
        }

        public IInteractable GetClosestInteractable()
        {
            float dist = float.MaxValue;
            IInteractable closest = null;
            foreach(var inter in interactablesInRange)
            {
                var newdist = ((Node2D)inter).GlobalPosition.DistanceTo(GlobalPosition);
                if (newdist < dist)
                {
                    dist = newdist;
                    closest = inter;
                }
            }
            return closest;
        }
    }
}
