﻿using BrannPack.Helpers.Initializers;
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
    public partial class CooldownHandler: Node
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

        public Cooldown AddCooldown((int indexType, int sourceIndex, int cooldownSource) key, Cooldown cooldown)
        {
            if (!Cooldowns.TryGetValue(key, out var existingCooldown))
            {
                Cooldowns[key] = cooldown;
                return cooldown;
            }
            else
            {
                existingCooldown.Duration += cooldown.Duration;
                //if(cooldown.CompletedCooldown)
                return existingCooldown;
            }
        }

        public Cooldown RemoveCooldown((int indexType, int sourceIndex, int cooldownSource) key, bool triggerCompletion)
        {
            Cooldowns.Remove(key, out Cooldown ans);
            if (ans!=null && triggerCompletion)
                ans.ForceInvokeCompletion();
            return ans;
        }

        public void SetCooldown((int indexType, int sourceIndex, int cooldownSource) key, float duration, bool removeFromHandlerOnCompletion = false)
        {  
                Cooldowns[key] = new Cooldown(duration, removeFromHandlerOnCompletion);
        }

        public void SetCooldown((int indexType, int sourceIndex, int cooldownSource) key, Cooldown cooldown)
        {
            Cooldowns[key] = cooldown;
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
        public float GetRemainingCooldown((int indexType, int sourceIndex, int cooldownSource) key)
        {
            if (Cooldowns.TryGetValue(key, out var cooldown))
            {
                    return cooldown.RemainingTime;
            }
            return 0f;
        }

        public Cooldown GetCooldown((int indexType, int sourceIndex, int cooldownSource) key)
        {
            if (Cooldowns.TryGetValue(key, out var cooldown))
            {
                return cooldown;
            }
            return null;
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

        public override void _Process(double delta)
        {
            base._Process(delta);
            UpdateCooldowns((float)delta);
        }
    }


    public class Cooldown
    {
        public float Duration;
        protected float elapsedTime;
        public virtual float PercentageComplete => elapsedTime/Duration;
        public bool IsPaused = false;

        public bool RemoveFromHandlerOnCompletion;
        public bool ResetAndPauseOnCompletion;

        public Cooldown(float duration, bool removeFromHandlerOnCompletion = true, Action<Cooldown> onComplete = null, bool resetAndPauseOnCompletion = false)
        {
            Duration = duration;
            elapsedTime = 0f;
            RemoveFromHandlerOnCompletion = removeFromHandlerOnCompletion;
            ResetAndPauseOnCompletion = resetAndPauseOnCompletion;

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
            {
                CompletedCooldown?.Invoke(this);
                if(!RemoveFromHandlerOnCompletion && ResetAndPauseOnCompletion)
                {
                    Reset();
                    IsPaused = true;
                }
            }
        }

        public virtual void Reset()
        {
            elapsedTime = 0f;
        }

        public void ForceInvokeCompletion() { CompletedCooldown?.Invoke(this); }
    }

    public class ChargedCooldown : Cooldown
    {
        public float CurrentCharges = 0;
        public float PartialCharges => CurrentCharges + PercentageComplete; 
        public ModifiableStats.AbilityStats.ChargeStat TrackedMaxCharges;
        public ModifiableStats.AbilityStats.CooldownStat TrackedCooldown;

        public ChargedCooldown(ModifiableStats.AbilityStats.CooldownStat cooldownStat, ModifiableStats.AbilityStats.ChargeStat chargeStat, Action<Cooldown> onComplete = null) : base(cooldownStat?.CalculateTotal() ?? .1f, false, onComplete)
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
            if (IsExpired && !IsPaused)
            {
                
                if (PartialCharges >= (TrackedMaxCharges?.CalculateTotal() ?? 1))
                {
                    CurrentCharges++;
                    IsPaused = true;
                }
                else
                {
                    CurrentCharges++;
                    Reset();
                }
            }
        }

        public bool TryUseCharge()
        {
            if (CurrentCharges <= 0f)
                return false;
            CurrentCharges--;
            IsPaused = false;
            
            return true;
        }

        public void MaxOut()
        {
            var maxCharges = TrackedMaxCharges.CalculateTotal();
            CurrentCharges = Mathf.Floor(maxCharges);
            elapsedTime = maxCharges % 1f * Duration;
            IsPaused = true;
        }
    }

    public class Windup : Cooldown
    {
        public float BufferTime;

        public override bool IsExpired => elapsedTime >= Duration+BufferTime;
        public virtual bool IsWindupComplete => elapsedTime >= Duration;
        public Windup(float duration, float bufferTime, bool removeFromHandlerOnCompletion = true, bool resetAndPauseOnCompletion=true,Action<Cooldown> onComplete = null) 
                : base(duration, removeFromHandlerOnCompletion, onComplete,resetAndPauseOnCompletion) => (BufferTime) = (bufferTime);
    }
}
