using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static BrannPack.ItemHandling.ItemCatalog;
using BrannPack.Tiers;
using BrannPack.Character;
using BrannPack.Helpers.Initializers;
using BrannPack.AbilityHandling;
using BrannPack.ModifiableStats;


namespace BrannPack.ItemHandling
{
    public abstract class Item<T> : Item where T : Item<T>
    {
        public static T instance { get; private set; }

        public Item()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            instance = this as T;
            instance.SetIndex();
            instance.Init();
            Item.ItemRegistry.Register(instance);
        }
    }

    public abstract class Item:IIndexable
    {
        protected static int NextIndex = 0;
        public static Registry<Item> ItemRegistry = new Registry<Item>();
        public int Index { get; protected set; } = -1;

        public void SetIndex() { if(Index!=-1)Index = NextIndex++; }

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
        public abstract ItemTier Tier { get; init; }
        public abstract ItemSubTier SubTier { get; init; }
        public abstract ItemModifier[] DefaultModifiers { get; init; }
        public abstract ItemModifier[] PossibleModifiers { get; init; }
        

        public abstract string Name { get; init; }
        public abstract string CodeName { get; init; }
        public abstract string Description { get; init; }
        public abstract string AdvancedDescription { get; init; }
        public virtual Texture2D WorldTexture { get; init; } = GD.Load<Texture2D>("res://Assets/PlaceholderAssets/ItemTextures/ErrorItemDisplay.png");

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
            //3- Items that Syergize with AbilityInstance Catagory are very high priority
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

        //public static bool IfMasterHasItem() { }
        public static ItemEffectModifier? IfMasterHasItemAndCheckStat(Item item,CharacterMaster master,Stat refreshingStat, Stat statToModify)
        {
            if (master.UsingInventory && refreshingStat == statToModify && master.Inventory.AllEffectiveItemCount.TryGetValue(item, out ItemEffectModifier iem))
                return iem;
            return null;
        }

        public virtual void Init() { }

        public virtual void ItemCountChangeBehavior(Inventory inventory,InventoryItemStack changed, bool IsAdded = true) { }

    }
    public class ErrorItem : Item<ErrorItem>
    {
        public override ItemTier Tier { get; init; } = Tier0.instance;
        public override ItemSubTier SubTier { get; init; } = null;
        public override ItemModifier[] DefaultModifiers { get; init; } = new ItemModifier[0];
        public override ItemModifier[] PossibleModifiers { get; init; } = new ItemModifier[0];
        public override string Name { get; init; } = "ERROR ITEM";
        public override string CodeName { get; init; } = "ERROR_ITEM";
        public override string Description { get; init; } = "ERROR";
        public override string AdvancedDescription { get; init; } = "ERROR";
        public override bool RequiresConfirmation { get; init; } = false;
        public override bool IsSharable { get; init; } = false;
        public override Dictionary<EffectTag, int> EffectWeight { get; init; } = new();

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void Init()
        {
            base.Init();
        }


        public override string ToString()
        {
            return base.ToString();
        }
    }

    public struct ItemEffectModifier
    {
        public static ItemEffectModifier StandardEffect = new ItemEffectModifier { Positive = 1f, Multiplier = 1f, Negative = 0f };
        public float Positive;
        public float Negative;
        public float Multiplier;

        public (float positive, float negative) EffectiveValues() { return (Positive * Multiplier, Negative * Multiplier); }

        public ItemEffectModifier EquivalentModifier() { return new ItemEffectModifier { Positive = this.Positive * this.Multiplier, Negative = this.Negative * this.Multiplier }; }

        public static ItemEffectModifier operator +(ItemEffectModifier a, ItemEffectModifier b)
        {
            return new ItemEffectModifier
            {

                Positive = a.Positive + b.Positive,
                Negative = a.Negative + b.Negative,
                Multiplier = a.Multiplier * b.Multiplier // Assuming multipliers should also add
            };
        }



    }
    public abstract class ItemModifier<T> : ItemModifier where T : ItemModifier<T>
    {
        public static T instance { get; private set; }

        public ItemModifier()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            instance = this as T;
        }
    }
    public abstract class ItemModifier
    {
        public abstract string Name { get; init; }
        public abstract string CodeName { get; init; }
        public abstract float ValueMult { get; init; }
        public List<Item> AllItemsInTier;
        public List<Item> AllUnlockedItems;

        public abstract ItemEffectModifier itemEffectModifier { get; init; }

    }

    public class Highlander:ItemModifier<Highlander>
    {
        public override string Name { get; init; } = "Highlander";
        public override string CodeName { get; init; } = "NoStack";

        public override float ValueMult { get; init; } = 3f;
        public override ItemEffectModifier itemEffectModifier { get; init; } = new ItemEffectModifier { Multiplier = 2.5f };
    }

    public class Chthonic : ItemModifier<Chthonic>
    {
        public override string Name { get; init; } = "Chthonic";
        public override string CodeName { get; init; } = "GoodAndBad";

        public override float ValueMult { get; init; } = 1.5f;
        public override ItemEffectModifier itemEffectModifier { get; init; } = new ItemEffectModifier {Positive=1.75f, Negative=.5f };
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

        //AbilityInstance Catagory
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
            IsEnemySlowStuckDep, IsEnemySlowStuckEnabler, IsSEDebuffDeper, IsSEDebuffEnabler, IsSEBuffDeper, IsSEBuffEnabler, IsResistReducerDeper, IsResistReducerEnabler,

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
