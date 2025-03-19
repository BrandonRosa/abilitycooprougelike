using BrannPack.Character;
using BrannPack.ItemHandling;
using BrannPack.ModifiableStats;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrannPack.AbilityHandling
{
    public abstract class AbilitySlot<T> : AbilitySlot where T : AbilitySlot<T>
    {
        public static T instance { get; private set; }

        public AbilitySlot()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            instance = this as T;
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
                Ability.UseAbility(Owner, CurrentUpgrades, CurrentTarget);
                return true;
            }
            return false;
        }
    }
    public abstract class Ability
    {
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
        private Dictionary<AbilityUpgrade, bool> IsUpgraded;
    }

    public class AbilityUpgrade
    {
        private List<AbilityUpgrade> Upgrades;
        private AbilityUpgrade Previous;
        private string Name;
        private string Description;
        private string AdvancedDescription;
        private int APCost;
        private int LockCost;
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
