using BrannPack.Character;
using BrannPack.CooldownHandling;
using BrannPack.Helpers.Initializers;
using BrannPack.ItemHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.StatusEffectHandling
{
    public class StatusEffectHandler
    {
        public BaseCharacter Owner;
        //Int is the StatusEffect Index
        Dictionary<int, List<StatusEffectStack>> StatusEffects;

        public static event Action<BaseCharacter, StatusEffectInfo> BeforeSEGain;
        public static event Action<BaseCharacter, StatusEffectInfo> AfterSEGain;

        public static event Action<BaseCharacter, StatusEffectStack> SELoss;

        public StatusEffectHandler(BaseCharacter owner)
        {
            Owner = owner;
        }

        /// <summary>
        /// Adds a status effect to the character.
        /// </summary>
        public void AddStatusEffect(StatusEffect statusEffect, float duration, float count)
        {
            if (statusEffect == null) return;

            StatusEffectInfo info = new()
            {
                Source = Owner,
                Target = Owner,
                InitialDuration = duration,
                InitialCount = count
            };

            // Invoke the "Before Status Effect Gain" event
            BeforeSEGain?.Invoke(Owner, info);

            // Get the index of the status effect
            int index = statusEffect.Index;

            if (!StatusEffects.TryGetValue(index, out var stacks))
            {
                stacks = new List<StatusEffectStack>();
                StatusEffects[index] = stacks;
            }

            // Check if we should stack or replace the effect
            var existingStack = stacks.FirstOrDefault();
            if (existingStack != null)
            {
                existingStack.Count += count;
                existingStack.cooldown = new Cooldown(duration); // Refresh duration
            }
            else
            {
                var newStack = new StatusEffectStack
                {
                    statusEffectInfo = info,
                    cooldown = new Cooldown(duration),
                    Count = count
                };

                stacks.Add(newStack);
                statusEffect.OnEffectGain();
            }

            // Invoke the "After Status Effect Gain" event
            AfterSEGain?.Invoke(Owner, info);
        }

        /// <summary>
        /// Removes a status effect from the character.
        /// </summary>
        public void RemoveStatusEffect(StatusEffect statusEffect)
        {
            if (statusEffect == null) return;

            int index = statusEffect.Index;
            if (StatusEffects.TryGetValue(index, out var stacks))
            {
                foreach (var stack in stacks)
                {
                    SELoss?.Invoke(Owner, stack);
                    statusEffect.OnEffectLoss();
                }

                StatusEffects.Remove(index);
            }
        }

        /// <summary>
        /// Updates all status effects (should be called every frame or tick).
        /// </summary>
        public void UpdateStatusEffects(float deltaTime)
        {
            List<int> expiredEffects = new();

            foreach (var (index, stacks) in StatusEffects)
            {
                List<StatusEffectStack> expiredStacks = new();

                foreach (var stack in stacks)
                {
                    stack.cooldown.Update(deltaTime);
                    if (stack.cooldown.IsExpired)
                    {
                        expiredStacks.Add(stack);
                        SELoss?.Invoke(Owner, stack);
                    }
                }

                // Remove expired stacks
                foreach (var expired in expiredStacks)
                {
                    stacks.Remove(expired);
                }

                if (stacks.Count == 0)
                {
                    expiredEffects.Add(index);
                }
            }

            // Remove fully expired effects
            foreach (int index in expiredEffects)
            {
                StatusEffects.Remove(index);
            }
        }
    }
    public class StatusEffectStack
    {
        
        public StatusEffectInfo statusEffectInfo;
        public Cooldown cooldown;
        public float Count;
    }
    public abstract class StatusEffect<T> : StatusEffect where T : StatusEffect<T>
    {
        public static T instance { get; private set; }

        public StatusEffect()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            instance = this as T;
            instance.SetIndex();
        }
    }
    public abstract class StatusEffect : IIndexable
    {
        protected static int NextIndex = 0;
        public int Index { get; protected set; } = -1;

        public void SetIndex() { if (Index != -1) Index = NextIndex++; }
        public abstract string Name { get; init; }
        public abstract string CodeName { get; init; }
        public abstract string Description { get; init; }

        public abstract HashSet<StatusEffectTag> Tags { get; init; }

        public abstract Type StatusEffectInfo { get; init; }

        public abstract void Init();

        public abstract void Update();

        public abstract void OnEffectGain();

        public abstract void OnEffectLoss();


    }

    public class StatusEffectInfo:EventInfo
    {
        public float InitialDuration;
        public float InitialCount;
    }


    public enum StatusEffectTag
    {
        //Basic Category
        IsBuff, IsDebuff, IsNeutral,

        //Debuff Type
        IsDamageDealing, IsDamageDown, IsMovementRestriction, IsDefenseDown,

        //BuffType
        IsDamageUp, IsMovementUp, IsDefenseUp
    }
}
