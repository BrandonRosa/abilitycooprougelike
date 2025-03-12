using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BrannPack.AbilityHandling
{
    public class Ability
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

    

    public enum AbilitySlot
    {
        Primary,Secondary,Utility,Special,Ult
    }

    public enum AbilityStat
    {
        Damage, CritChance, CritDamage, FireRate, ProjectileSpeed, Charges, Range, Duration, Chance, ProcChance, PositiveEffect,NegativeEffect
    }
}
