using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static BrannPack.ItemHandling.ItemCatalog;
using BrannPack.Tiers;


namespace BrannPack.ItemHandling
{
    public abstract class Item<T> : Item where T : Item<T>
    {
        public static T instance { get; private set; }

        public Item()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            instance = this as T;
        }
    }

    public abstract class Item
    {

        public abstract ItemTier Tier { get; init; }
        public abstract ItemSubTier SubTier { get; init; }
        public abstract ItemModifier[] DefaultModifiers { get; init; }
        public abstract ItemModifier[] PossibleModifiers { get; init; }
        private int _itemIndex;
        public int ItemIndex
        {
            get => _itemIndex;
            set
            {
                if (_itemIndex == 0) // Assuming 0 means "uninitialized"
                    _itemIndex = value;
            }
        }

        public abstract string Name { get; init; }
        public abstract string CodeName { get; init; }
        public abstract string Description { get; init; }
        public abstract string AdvancedDescription { get; init; }

        public abstract bool RequiresConfirmation { get; init; }
        public abstract bool IsSharable { get; init; }

        //Weight ranges from -3 to 3
        // 0 = No Effect (no tag needed)
        // 1 = Slight Positive Effect
        // 2 = Good Positive Effect
        // 3 = Strong Positive Effect
        // NOTE: Weight is compared to items in similar rarity, so a Tier0 item CAN have weights of 3
        public abstract Dictionary<EffectTag, int> EffectWeight { get; init; }

        

        public static Item[] FindPerfectItems(Item[] currentItems, ItemFilter itemFilter, int count)
        {
            //BestChoices:
            //1- If close to completing a set MAKE THOSE HIGHEST PRIORITY
            //2- Items that synergize with current items(Debuff/other item synergy) are very high priority
            //3- Items that Syergize with Ability Catagory are very high priority
            //4- Highest Basic Catagory is low priority
            //5- Highest Tier/Sub-Tier/Modifier catagory is low priority
            //6- Lowest Basic Catagory is lowest priority

            //Diversity Choices:
            //Always make the last choice a diversity choice. 
            //Diversity choice is a perfect item with the highest score in the lowest basic catagory
            //Basically we'll Get the pool for Items in the selected basic catagory, filter for the top 5 weighted items then use THAT as the FindPerfectItemsPool

            //Tag Notes:
            //For Dep-Enabler Pairs, try to get them to have even scores


            //If nothing can be found, return the error item.
            return null;
        }

        public static EffectTag[] FindPerfectTags(Dictionary<EffectTag, float> totalWeights, int count)
        {
            //If The Top one is a Dep and it's associated Enable score is low, give the associated enabler
            //If The Top one is a Enabler and its associated Dep is low, give it a Dep

            return null;
        }

    }

    public struct ItemEffectModifier
    {
        public float Positive;
        public float Negative;
        public float Multiplier;

        public (float positive, float negative) EffectiveValues() { return (Positive * Multiplier, Negative * Multiplier); }
    }
    public class ItemModifier
    {
        private static string Name;
        private static string CodeName;
        private static List<Item> AllItemsInTier;
        private static List<Item> AllUnlockedItems;

        ItemEffectModifier itemEffectModifier;

    }

    //Dep = Quality of Effects are Improved (Crit based effects)
    //Enabler = Frequency/Quantity of Effect Is Increased (Crit Lense)
    public enum EffectTag
    {
        //Basic Catagory
        IsAttack,IsDefensive,IsUtility,

        //AttackProperties
        IsDamageDep, IsDamageEnabler, IsRangeDep,IsRangeEnabler, IsDurationDep,IsDurationEnabler, IsCooldownDep, IsCooldownEnabler,
        IsFireRateDep,IsFireRateEnabler, IsChargeDep, IsChargeEnabler,

        //Ability Catagory
        IsPrimaryDep, IsPrimaryEnabler,IsSecondaryDep, IsSeconaryEnabler, IsUtilityDep, IsUtilityEnabler, IsSpecialDep, IsSpecialEnabler,IsUltDep, IsUltEnabler,IsItemDep, IsItemEnabler,IsActiveDep, IsActiveEnabler,

        //Item Tier Catagory
        IsTealDep, IsTealEnabler, IsWhiteDep, IsWhiteEnabler, IsGreenDep, IsGreenEnabler, IsOrangeDep, IsOrangeEnabler, 
            IsYellowDep, IsYellowEnabler, IsRedDep, IsRedEnabler,

        //Item Sub-Tier Catagory
        IsEssenceDep, IsEssenceEnabler, IsPromiseDep, IsPromiseEnabler, 

        //Item Modifier Catagory
        IsHighlanderDep, IsHighlanderEnabler, IsChthonicDep, IsChthonicEnabler, IsCurseDep, IsCurseEnabler, IsBrokenDep, IsBrokenEnabler, 
            IsPrismaticDep, IsPrismaticEnabler, IsTemporaryItemDep, IsTemporaryItemEnabler,

        //Debuff Catagory
        IsBleedDep,IsBleedEnabler,IsCritDep,IsCritEnabler, IsPoisonDep, IsPoisonEnabler, IsFireDep, IsFireEnabler, 
            IsEnemySlowStuckDep, IsEnemySlowStuckEnabler, IsSEDebuffDeper, IsSEDebuffEnabler, IsSEBuffDeper, IsSEBuffEnabler, IsResistReducerDeper, IsResistReducerEnabler

        //Ally Catgory
        IsFamiliarDep, IsFamiliarEnabler, IsMinionDep, IsMinionEnabler, IsAllyDep, IsAllyEnabler,
        
        //Defensive Items
        IsHealthDep, IsHealthEnabler, IsRegenDep, IsRegenEnabler, IsHealDep, IsHealEnabler, IsArmorDep, IsArmorEnabler, IsResistanceDep, IsResistanceEnabler,
            IsShieldDep, IsShieldEnabler, IsBarrierDep, IsBarrierEnabler, IsNegativeEffectsReducerDep, IsNegativeEffectsReducerEnabler, 

        //Utility Items
        IsMoveSpeedDep, IsMoveSpeedEnabler, IsMovementRestrictionDep, IsMovementRestrictionBoost,

        //Item Triggers
        IsOnKillDep, IsOnKillEnabler, IsHighDamageHitDep, IsHighDamageHitEnabler, IsFastHitsDep, IsFastHitsEnabler, IsStandStillDep, IsStandStillEnabler, 
            IsInDangerDep, IsInDangerEnabler,IsOutOfDangerDep, IsOutOfDangerEnabler, IsCloseRangeDep, IsCloseRangeEnabler, 
            IsMediumRangeDep, IsMediumRangeEnabler, IsLowHPDep, IsLowHPEnabler, IsHighHPDep, IsHighHPEnabler,
            IsPercentBossHealthDep, IsPercentBossHealthEnabler, IsPercentEnemyHealthDep, IsPercentEnemyHealthEnabler, IsPercentYourHealthDep, IsPercentYourHealthEnabler,  

        //Item Targets
        IsSingleTargetEnemyDep, IsSingleTargetEnemyEnabler, IsAOEEnemyDep, IsAOEEnemyEnabler, IsSingleTargetAllyDep, IsSingleTargetAllyEnabler, 
            IsAOEAllyDep, IsAOEAllyEnabler, IsBossTargetDep, IsBossTargetEnabler, IsEliteTargetDep, IsEliteTargetEnabler,

        //Item Finding
        IsDiscoverDep, IsDiscoverEnabler, IsPerfectItemDep, IsPerfectItemEnabler, IsAdditionalItemDeper, IsAdditionalItemEnabler,

    }
    
}
