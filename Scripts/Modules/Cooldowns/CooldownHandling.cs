using BrannPack.Helpers.Initializers;
using BrannPack.ModifiableStats;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

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
        public Cooldown AddCooldown((int indexType, int sourceIndex, int cooldownSource) key, float duration, bool removeFromHandlerOnCompletion=false, Action<Cooldown> onComplete = null)
        {
            //If the cooldown 
            if (!Cooldowns.TryGetValue(key, out var cooldown))
            {
                cooldown = new Cooldown(duration,removeFromHandlerOnCompletion,onComplete);
                Cooldowns[key] = cooldown;
                
            }
            else 
            {
                cooldown.Duration += duration;
                if (onComplete != null)
                    cooldown.CompletedCooldown += onComplete;
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
        protected float elapsedTime;
        public virtual float PercentageComplete => elapsedTime/Duration;
        public bool IsPaused = false;

        public bool RemoveFromHandlerOnCompletion;

        public Cooldown(float duration, bool removeFromHandlerOnCompletion = true, Action<Cooldown> onComplete = null)
        {
            Duration = duration;
            elapsedTime = 0f;
            RemoveFromHandlerOnCompletion = removeFromHandlerOnCompletion;

            if (onComplete != null)
                CompletedCooldown += onComplete;
        }

        public event Action<Cooldown> CompletedCooldown;

        public virtual bool IsExpired => elapsedTime >= Duration;
        public float RemainingTime => Math.Max(0, Duration - elapsedTime);

        public virtual void Update(float deltaTime)
        {
            if(!IsExpired && !IsPaused)
                elapsedTime += deltaTime;
                if (IsExpired)
                    CompletedCooldown?.Invoke(this);
        }

        public virtual void Reset()
        {
            elapsedTime = 0f;
        }
    }

    public class ChargedCooldown : Cooldown
    {
        public float CurrentCharges = 0;
        public float PartialCharges => CurrentCharges + PercentageComplete; 
        public ModifiableStats.AbilityStats.ChargeStat TrackedMaxCharges;
        public ModifiableStats.AbilityStats.CooldownStat TrackedCooldown;

        public ChargedCooldown(ModifiableStats.AbilityStats.CooldownStat cooldownStat, ModifiableStats.AbilityStats.ChargeStat chargeStat, Action<Cooldown> onComplete = null) : base(cooldownStat.CalculateTotal(), false, onComplete)
        {
            TrackedMaxCharges = chargeStat;
            TrackedCooldown = cooldownStat;
            cooldownStat.ChangedTotal += UpdateDuration;
        }

        public void SetCooldownStat(ModifiableStats.AbilityStats.CooldownStat cooldownStat)
        {
            TrackedCooldown.ChangedTotal -= UpdateDuration;
            var old = TrackedCooldown.CalculateTotal();

            TrackedCooldown = cooldownStat;
            cooldownStat.ChangedTotal += UpdateDuration;
            UpdateDuration(cooldownStat.CalculateTotal(),old);
        }

        private void UpdateDuration(float newValue, float oldValue)
        {
            elapsedTime = PercentageComplete * newValue;
            Duration = newValue;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            if (IsExpired)
                CurrentCharges++;
            if (PartialCharges >= TrackedMaxCharges.CalculateTotal())
            {
                IsPaused = true;
            }
            else
            {
                Reset();
            }
        }

        public bool TryUseCharge()
        {
            if (CurrentCharges <= 0f)
                return false;
            CurrentCharges--;
            
            return true;
        }
    }
}
