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
        public abstract ItemTierModifier[] DefaultModifiers { get; init; }
        public abstract ItemTierModifier[] PossibleModifiers { get; init; }
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

        public readonly float Count;
        public readonly List<(float count, float duration)> TemporaryStacks;

        

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
            //For Boost-Enabler Pairs, try to get them to have even scores


            //If nothing can be found, return the error item.
            return null;
        }

        public static EffectTag[] FindPerfectTags(Dictionary<EffectTag, float> totalWeights, int count)
        {
            //If The Top one is a Boost and it's associated Enable score is low, give the associated enabler
            //If The Top one is a Enabler and its associated boost is low, give it a boost

            return null;
        }

    }

    public class ItemDef
    {
        public ItemIndex _itemIndex;
        public ItemIndex itemIndex
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get
            {
                throw null;
            }
            [MethodImpl(MethodImplOptions.NoInlining)]
            set
            {
                throw null;
            }
        }
        //Whats this do??
        public struct Pair : IEquatable<Pair>
        {
            public ItemDef itemDef1;

            public ItemDef itemDef2;

            [MethodImpl(MethodImplOptions.NoInlining)]
            public override int GetHashCode()
            {
                throw null;
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public override bool Equals(object obj)
            {
                throw null;
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public bool Equals(Pair other)
            {
                throw null;
            }
        }


    }

    
    public class ItemTierModifier
    {
        private static string Name;
        private static string CodeName;
        private static List<Item> AllItemsInTier;
        private static List<Item> AllUnlockedItems;
    }

    //Boost = Quality of Effects are Improved
    //Enabler = Frequency/Quantity of Effect Is Increased
    public enum EffectTag
    {
        //Basic Catagory
        IsAttack,IsDefensive,IsUtility,

        //AttackProperties
        IsDamageBoost,IsRangeBoost,IsDurationBoost,IsCooldownBoost,IsFireRateBoost,IsChargeBoost,

        //Ability Catagory
        IsPrimaryBoost,IsSecondaryBoost,IsUtilityBoost,IsSpecialBoost,IsUltBoost,IsItemBoost,IsActiveBoost, 

        //Item Tier Catagory
        IsTealBoost, IsTealEnabler, IsWhiteBoost, IsWhiteEnabler, IsGreenBoost, IsGreenEnabler, IsOrangeBoost, IsOrangeEnabler, IsYellowBoost, IsYellowEnabler, IsRedBoost, IsRedEnabler,

        //Item Sub-Tier Catagory
        IsEssenceBoost, IsEssenceEnabler, IsPromiseBoost, IsPromiseEnabler, 

        //Item Modifier Catagory
        IsHighlanderBoost, IsHighlanderEnabler, IsHermeticBoost, IsHermeticEnabler, IsCurseBoost, IsCurseEnabler, IsBrokenBoost, IsBrokenEnabler, IsPrismaticBoost, IsPrismaticEnabler, IsTemporaryItemBoost, IsTemporaryItemEnabler,

        //Debuff Catagory
        IsBleedBoost,IsBleedEnabler,IsCritBoost,IsCritEnabler, IsPoisonBoost, IsPoisonEnabler, IsFireBoost, IsFireEnabler, IsEnemySlowStuckBoost, IsEnemySlowStuckEnabler,

        //Other Item Synergy
        IsFamiliarBoost, IsFamiliarEnabler, IsMinionBoost, IsMinionEnabler, IsAllyBoost, IsAllyEnabler,IsHealingBoost, IsHealingEnabler,IsOnKillBoost, IsOnKillEnabler,  
            IsDiscoverBoost, IsDiscoverEnabler, IsHighDamageHitBoost, IsHighDamageHitEnabler, IsFastHitsBoost, IsFastHitsEnabler, IsSEDebuffBooster, IsSEDebuffEnabler, IsSEBuffBooster, IsSEBuffEnabler,IsAdditionalItemBooster, IsAdditionalItemEnabler, 
            IsHealthBoost, IsHealthEnabler, IsRegenBoost, IsRegenEnabler, IsHealBoost, IsHealEnabler,IsArmorBoost, IsArmorEnabler, IsShieldBoost, IsShieldEnabler, IsBarrierBoost, IsBarrierEnabler, IsStandStillBoost, IsStandStillEnabler, IsInDangerBoost, 
            IsInDangerEnabler,IsOutOfDangerBoost, IsOutOfDangerEnabler, IsCloseRangeBoost, IsCloseRangeEnabler, IsMediumRangeBoost, IsMediumRangeEnabler, IsLowHPBoost, IsLowHPEnabler, IsHighHPBoost, IsHighHPEnabler,
            IsPercentBossHealthBoost, IsPercentBossHealthEnabler, IsPercentEnemyHealthBoost, IsPercentEnemyHealthEnabler, IsPercentYourHealthBoost, IsPercentYourHealthEnabler, IsSingleTargetEnemyBoost, IsSingleTargetEnemyEnabler, IsAOEEnemyBoost, IsAOEEnemyEnabler,
            IsSingleTargetAllyBoost, IsSingleTargetAllyEnabler, IsAOEAllyBoost, IsAOEAllyEnabler, IsBossTargetBoost, IsBossTargetEnabler, IsEliteTargetBoost, IsEliteTargetEnabler, IsNegativeEffectsReducerBoost, IsNegativeEffectsReducerEnabler, 
            IsPerfectItemBoost, IsPerfectItemEnabler

    }
    
}
