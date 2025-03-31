using BrannPack.Helpers.Initializers;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.CooldownHandling
{
    public class CooldownHandler
    {
        // Second int: Index of the item/ability/status effect (from IIndexable).
        // Third int: Cooldown instance (for multiple cooldowns per item).
        private Dictionary<(int indexType,int sourceIndex,int cooldownSource), Cooldown> Cooldowns = new();


        /// <summary>
        /// Adds a cooldown for a given instance of T.
        /// </summary>
        public Cooldown AddCooldown((int indexType, int sourceIndex, int cooldownSource) key, float duration, bool removeFromHandlerOnCompletion=false)
        {
            //If the cooldown 
            if (!Cooldowns.TryGetValue(key, out var cooldown))
            {
                cooldown = new Cooldown(duration,removeFromHandlerOnCompletion);
                Cooldowns[key] = cooldown;
                
            }
            else 
            {
                cooldown.Duration += duration;
            }

            return cooldown;
        }

        public void SetCooldown((int indexType, int sourceIndex, int cooldownSource) key, float duration, bool removeFromHandlerOnCompletion = false)
        {  
                Cooldowns[key] = new Cooldown(duration, removeFromHandlerOnCompletion);
        }

        /// <summary>
        /// Checks if a cooldown is still active.
        /// </summary>
        public bool IsOnCooldown((int indexType, int sourceIndex, int cooldownSource) key)
        {
            if (Cooldowns.TryGetValue(key, out var cooldown))
            {
                return !cooldown.IsExpired;
            }
            return false;
        }

        /// <summary>
        /// Gets the remaining cooldown time.
        /// </summary>
        public float GetRemainingCooldown((int indexType, int sourceIndex, int cooldownSource) key, int cooldownInstance)
        {
            if (Cooldowns.TryGetValue(key, out var cooldown))
            {
                    return cooldown.RemainingTime;
            }
            return 0f;
        }

        /// <summary>
        /// Updates all cooldowns (should be called every frame or tick).
        /// </summary>
        public void UpdateCooldowns(float deltaTime)
        {
            List<(int indexType, int sourceIndex, int cooldownSource)> toRemove = new();

            foreach (var (key, cooldown) in Cooldowns)
            {
                cooldown.Update(deltaTime);
                if (cooldown.IsExpired && cooldown.RemoveFromHandlerOnCompletion)
                    toRemove.Add(key);
            }

            foreach (var key in toRemove)
                Cooldowns.Remove(key);
        }
    }


    public class Cooldown
    {
        public float Duration;
        private float elapsedTime;

        public bool RemoveFromHandlerOnCompletion;

        public Cooldown(float duration, bool removeFromHandlerOnCompletion=true)
        {
            Duration = duration;
            elapsedTime = 0f;
            RemoveFromHandlerOnCompletion = removeFromHandlerOnCompletion;
        }

        public event Action<Cooldown> CompletedCooldown;

        public bool IsExpired => elapsedTime >= Duration;
        public float RemainingTime => Math.Max(0, Duration - elapsedTime);

        public void Update(float deltaTime)
        {
            if(!IsExpired)
                elapsedTime += deltaTime;
                if (IsExpired)
                    CompletedCooldown?.Invoke(this);
        }
    }

}
