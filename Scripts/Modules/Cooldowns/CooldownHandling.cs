using BrannPack.Helpers.Initializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.CooldownHandling
{
    public class CooldownHandler<T> where T : IIndexable
    {
        // First int: Index of the item/ability/status effect (from IIndexable).
        // Second int: Cooldown instance (for multiple cooldowns per item).
        private Dictionary<int, Dictionary<int, Cooldown>> Cooldowns = new();

        /// <summary>
        /// Adds a cooldown for a given instance of T.
        /// </summary>
        public void AddCooldown(T instance, int cooldownInstance, float duration)
        {
            if (!Cooldowns.TryGetValue(instance.Index, out var instanceCooldowns))
            {
                instanceCooldowns = new Dictionary<int, Cooldown>();
                Cooldowns[instance.Index] = instanceCooldowns;
            }

            instanceCooldowns[cooldownInstance] = new Cooldown(duration);
        }

        /// <summary>
        /// Checks if a cooldown is still active.
        /// </summary>
        public bool IsOnCooldown(T instance, int cooldownInstance)
        {
            if (Cooldowns.TryGetValue(instance.Index, out var instanceCooldowns))
            {
                if (instanceCooldowns.TryGetValue(cooldownInstance, out var cooldown))
                {
                    return !cooldown.IsExpired;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the remaining cooldown time.
        /// </summary>
        public float GetRemainingCooldown(T instance, int cooldownInstance)
        {
            if (Cooldowns.TryGetValue(instance.Index, out var instanceCooldowns))
            {
                if (instanceCooldowns.TryGetValue(cooldownInstance, out var cooldown))
                {
                    return cooldown.RemainingTime;
                }
            }
            return 0f;
        }

        /// <summary>
        /// Updates all cooldowns (should be called every frame or tick).
        /// </summary>
        public void UpdateCooldowns(float deltaTime)
        {
            List<int> toRemove = new();

            foreach (var (index, instanceCooldowns) in Cooldowns)
            {
                List<int> expiredInstances = new();

                foreach (var (cooldownInstance, cooldown) in instanceCooldowns)
                {
                    cooldown.Update(deltaTime);
                    if (cooldown.IsExpired)
                        expiredInstances.Add(cooldownInstance);
                }

                foreach (int expired in expiredInstances)
                    instanceCooldowns.Remove(expired);

                if (instanceCooldowns.Count == 0)
                    toRemove.Add(index);
            }

            foreach (int index in toRemove)
                Cooldowns.Remove(index);
        }
    }


    public class Cooldown
    {
        public float Duration { get; }
        private float elapsedTime;

        public Cooldown(float duration)
        {
            Duration = duration;
            elapsedTime = 0f;
        }

        public bool IsExpired => elapsedTime >= Duration;
        public float RemainingTime => Math.Max(0, Duration - elapsedTime);

        public void Update(float deltaTime)
        {
            elapsedTime += deltaTime;
        }
    }

}
