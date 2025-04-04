using BrannPack.Character;
using BrannPack.Helpers.Initializers;
using BrannPack.ItemHandling;
using BrannPack.ModifiableStats;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static BrannPack.ModifiableStats.AbilityStats;

namespace BrannPack.AbilityHandling
{
    
    public class AbilitySlot
    {
        public CharacterMaster Owner;
        public BaseCharacterBody CurrentTarget;
        public AbilitySlotType SlotType;
        public Ability Ability;
        public AbilityUpgradeTree CurrentUpgrades;
        public AbilityStats.StatsHolder<AbilitySlot> ThisAbilityStats;
        public Timer Cooldown;
        public bool IsUsable;

        public static event Action<AbilitySlot> BeforeAbilitySlotUse;
        public static event Action<AbilitySlot> AfterAbilitySlotUse;

        public void Initialize()
        {
            ////When OwnerBody's Base _stats are upated, this should also recalculate _stats.
            //Stats.StatsHolder<BaseCharacterBody>.StatUpdatedWithNewTotal +=
            //    (BaseCharacterBody baseCharacter, BaseCharacterBody.Stat variable, ModifiableStat modStat, float newTotal, float oldTotal) =>
            //    {
            //        if (baseCharacter == Owner)
            //        {
            //            ThisAbilityStats.RecalculateAndAddStats(variable,baseCharacter.Stats.GetStatByVariable(variable));
            //        }
            //    };

            
        }
        public bool TryUseAbility()
        {
            if(IsUsable)
            {
                BeforeAbilitySlotUse?.Invoke(this);
                Ability.UseAbility(Owner, CurrentUpgrades, CurrentTarget);
                AfterAbilitySlotUse?.Invoke(this);
                return true;
            }
            return false;
        }
    }

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
    public abstract class Ability : IIndexable
    {
        protected static int NextIndex = 0;
        public int Index { get; protected set; } = -1;

        public void SetIndex() { if (Index != -1) Index = NextIndex++; }

        public abstract StatsByCritera<AbilityUpgrade> Stats { get; set; }
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

        public abstract void UseAbility(CharacterMaster master,AbilitySlot abilitySlot,AbilityUpgradeTree treeProgress, BaseCharacterBody target, EventChain eventChain=default);
        public abstract BaseCharacterBody UpdateTarget();

    }

    public class AbilityUpgradeTree
    {
        public Dictionary<AbilityUpgrade, bool> IsUpgraded;
    }

    [GlobalClass]  // Enables it to be created in the Godot Editor
    public partial class AbilityUpgrade : Resource, IComparable<AbilityUpgrade>
    {
        [Export] public string Name;
        [Export] public string Description;
        [Export] public string AdvancedDescription;
        [Export] public int APCost;
        [Export] public int LockCost;
        [Export] public List<AbilityUpgrade> Requirements;
        [Export] public int Height;
        [Export] public int Column;

        public int CompareTo(object compareTo)
        {
            if (compareTo is not AbilityUpgrade other)
                throw new ArgumentException("Object is not an AbilityUpgrade");

            // If 'other' is a requirement of this, 'this' is always greater
            if (Requirements.Contains(other))
                return 1;

            // If 'this' is a requirement of 'other', 'this' is always smaller
            if (other.Requirements.Contains(this))
                return -1;

            // Compare by height (higher values are considered "greater")
            return Height.CompareTo(other.Height);
        }

        public int CompareTo(AbilityUpgrade other)
        {
            // If 'other' is a requirement of this, 'this' is always greater
            if (Requirements.Contains(other))
                return 1;

            // If 'this' is a requirement of 'other', 'this' is always smaller
            if (other.Requirements.Contains(this))
                return -1;

            // Compare by height (higher values are considered "greater")
            return Height.CompareTo(other.Height);
        }
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
