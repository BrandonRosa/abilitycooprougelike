using BrannPack.Character;
using BrannPack.GameDirectrs;
using BrannPack.InputHelpers;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.Interactables
{
    public interface IInteractable
    {
        public virtual string[] ActionKeyNames => ["interact1","interact2"];

        /// <summary>
        /// Called when the interactable is activated.
        /// </summary>
        public void Activate(BaseCharacterBody body,string actionKeyName, InputPressState inputPressState);



        /// <summary>
        /// Checks if the interactable can be activated.
        /// </summary>
        /// <returns>True if it can be activated, otherwise false.</returns>
        public abstract bool IsEnabled { get; set; }

        public bool IsInRange => false;

        /// <summary>
        /// Called when a character enters interactive range.
        /// </summary>
        public virtual void OnEnterInteractRange(BaseCharacterBody body) { }

        /// <summary>
        /// Called when a character exits interactive range.
        /// </summary>
        public virtual void OnExitInteractRange(BaseCharacterBody body) { }
    }

   

    
}
