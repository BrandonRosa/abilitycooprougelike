using BrannPack.Character;
using BrannPack.Interactable;
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

        public override void _Ready()
        {
             AreaEntered += OnBodyEntered;
            AreaEntered += OnBodyExited;
        }

        private void OnBodyEntered(Node2D node)
        {
            if (node is IInteractable interactable)
            {
                interactablesInRange.Add(interactable);
                interactable.OnEnterInteractRange(GetParent<BaseCharacterBody>());
            }
        }

        private void OnBodyExited(Node2D node)
        {
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
