using BrannPack.Character;
using BrannPack.Helpers.Initializers;
using BrannPack.ItemHandling;
using BrannPack.ModifiableStats;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrannPack.AbilityHandling
{
    public abstract class Ability<T> : Ability where T : Ability<T>
    {
        public static T instance { get; private set; }

        public Ability()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            instance = this as T;
            instance.SetIndex();
        }
    }
    public class AbilitySlot
    {
        public BaseCharacter Owner;
        public BaseCharacter CurrentTarget;
        public AbilitySlotType SlotType;
        public Ability Ability;
        public AbilityUpgradeTree CurrentUpgrades;
        public AbilityStats.AbilityStatsHolder<AbilitySlot> ThisAbilityStats;
        public Timer Cooldown;
        public bool IsUsable;

        public static event Action<AbilitySlot> BeforeAbilitySlotUse;
        public static event Action<AbilitySlot> AfterAbilitySlotUse;

        public void Initialize()
        {
            //When Owner's Base stats are upated, this should also recalculate stats.
            AbilityStats.AbilityStatsHolder<BaseCharacter>.StatUpdatedWithNewTotal +=
                (BaseCharacter baseCharacter, BaseCharacter.CharacterAbilityStatVariable variable, ModifiableStat modStat, float newTotal, float oldTotal) =>
                {
                    if (baseCharacter == Owner)
                    {
                        ThisAbilityStats.RecalculateAndAddStats(variable,baseCharacter.AbilityStats.GetStatByVariable(variable));
                    }
                };

            
        }
        public bool TryUseAbility()
        {
            if(IsUsable)
            {
                BeforeAbilityUse?.Invoke(this);
                Ability.UseAbility(Owner, CurrentUpgrades, CurrentTarget);
                AfterAbilityUse?.Invoke(this);
                return true;
            }
            return false;
        }
    }
    public abstract class Ability : IIndexable
    {
        protected static int NextIndex = 0;
        public int Index { get; protected set; } = -1;

        public void SetIndex() { if (Index != -1) Index = NextIndex++; }

        public Dictionary<AbilityStat, Ability> ModifiableStats;
        private float BaseCooldown;
        private float CurrentCooldown;
        private float NoSpamCooldown;
        private float MaxCharges;
        private float CurrentCharges;

        private bool CanCharge;
        private bool IsInfiniteUse;
        private bool IsMultiPrompt;

        private AbilityUpgradeTree UpgradeTree;

        //private image ArtWork;
        private static string Description;
        private static string AdvancedDescription;
        private static string Name;
        private static List<AbilityUpgrade> AbilityUpgrades;

        public abstract void UseAbility(BaseCharacter baseCharacter, AbilityUpgradeTree treeProgress, BaseCharacter target);
        public abstract BaseCharacter UpdateTarget();

    }

    public class AbilityUpgradeTree
    {
        public Dictionary<AbilityUpgrade, bool> IsUpgraded;
    }

    public class AbilityUpgrade
    {
        public List<AbilityUpgrade> Requirements;
        public string Name;
        public string Description;
        public string AdvancedDescription;
        public int APCost;
        public int LockCost;
    }

    

    public enum AbilitySlotType
    {
        Primary,Secondary,Utility,Special,Ult
    }

    public enum AbilityStat
    {
        Damage, CritChance, CritDamage, FireRate, ProjectileSpeed, Charges, Range, Duration, Chance, ProcChance, PositiveEffect,NegativeEffect
    }
}
