using Godot;
using System;
using System.Collections.Generic;

namespace BrannPack
{
    public class Ability
    {
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
        private String Name;
        private String Description;
        private String AdvancedDescription;
        private int APCost;
        private int LockCost;
    }
}
