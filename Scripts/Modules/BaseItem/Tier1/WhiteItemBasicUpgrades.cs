using BrannPack.AbilityHandling;
using BrannPack.Character;
using BrannPack.ItemHandling;
using BrannPack.ModifiableStats;
using BrannPack.Tiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BrannPack.Character.BaseCharacterBody;
using static BrannPack.ModifiableStats.AbilityStats;
using static Godot.WebSocketPeer;

namespace BrannPack.Items
{
    public class AbUpPrimaryFireRate : Item<AbUpPrimaryFireRate>
    {
        public override ItemTier Tier { get; init; } = Tier1.instance;
        public override ItemSubTier SubTier { get; init; } = null;
        public override ItemModifier[] DefaultModifiers { get; init; } = new ItemModifier[0];
        public override ItemModifier[] PossibleModifiers { get; init; } = new ItemModifier[0];
        public override string Name { get; init; } = "Rapid Rounds Oil";
        public override string CodeName { get; init; } = "PRIM_FR_UP";
        public override string Description { get; init; } = "Increase the firerate on your primary ability";
        public override string AdvancedDescription { get; init; } = "";
        public override bool RequiresConfirmation { get; init; } = false;
        public override bool IsSharable { get; init; } = true;
        public override Dictionary<EffectTag, int> EffectWeight { get; init; } = new Dictionary<EffectTag, int>
        {
            {EffectTag.IsAttack,3 },
            {EffectTag.IsBleedEnabler,1 },
            {EffectTag.IsDamageEnabler,2 },
            {EffectTag.IsFastHitsEnabler,3 },
            {EffectTag.IsFireRateEnabler,3},
            {EffectTag.IsPrimaryEnabler,3 }

        };

        public override void Init()
        {
            AbilityStats.StatsHolder<AbilitySlot>.GlobalRefreshAbilityStatVariable += ModifyStat;
        }


        public override void ItemCountChangeBehavior(Inventory inventory, InventoryItemStack itemStack, bool IsAdded = true)
        {
            // Ensure the event is only subscribed once
                

            inventory.InventoryOf.Primary.ThisAbilityStats.RecalculateByStatVariable(Stat.FireRate);
        }

        private void ModifyStat(AbilitySlot slot, Stat casv, ModifiableStat modStat)
        {
            if (slot.Owner.UsingInventory && casv == Stat.FireRate && slot.SlotType==AbilitySlotType.Primary && slot.Owner.Inventory.AllEffectiveItemCount.TryGetValue(this, out ItemEffectModifier effects))
            {
                if (modStat is AbilityStats.FireRateStat FirerateStat)
                {
                    FirerateStat.ChangeFireRatePercentage(.10f * effects.Positive);
                }
            }
        }
    }

    public class AbUpSecondaryCharge : Item<AbUpSecondaryCharge>
    {
        public override ItemTier Tier { get; init; } = Tier1.instance;
        public override ItemSubTier SubTier { get; init; } = null;
        public override ItemModifier[] DefaultModifiers { get; init; } = new ItemModifier[0];
        public override ItemModifier[] PossibleModifiers { get; init; } = new ItemModifier[0];
        public override string Name { get; init; } = "Arccell Pack";
        public override string CodeName { get; init; } = "SEC_CHRG_UP";
        public override string Description { get; init; } = "Gain a charge on your secondary ability";
        public override string AdvancedDescription { get; init; } = "";
        public override bool RequiresConfirmation { get; init; } = false;
        public override bool IsSharable { get; init; } = true;
        public override Dictionary<EffectTag, int> EffectWeight { get; init; } = new Dictionary<EffectTag, int>
        {
            {EffectTag.IsAttack,1 },
            {EffectTag.IsChargeEnabler,3 },
            {EffectTag.IsSeconaryEnabler,3 },
            {EffectTag.IsUtility,2 }

        };

        public override void Init()
        {
            AbilityStats.StatsHolder<AbilitySlot>.GlobalRefreshAbilityStatVariable += ModifyStat;
        }

        public override void ItemCountChangeBehavior(Inventory inventory, InventoryItemStack itemStack, bool IsAdded = true)
        {
            // Ensure the event is only subscribed once


            inventory.InventoryOf.Secondary.ThisAbilityStats.RecalculateByStatVariable(Stat.Charges);
        }

        private void ModifyStat(AbilitySlot slot, Stat casv, ModifiableStat modStat)
        {
            if (slot.Owner.UsingInventory && casv == Stat.Charges && slot.SlotType == AbilitySlotType.Secondary && slot.Owner.Inventory.AllEffectiveItemCount.TryGetValue(this, out ItemEffectModifier effects))
            {
                if (modStat is AbilityStats.ChargeStat ChargeStat)
                {
                    ChargeStat.ChangeAdditionalCharges(1f * effects.Positive);
                }
            }
        }
    }

    public class AbUpUtilityDuration : Item<AbUpUtilityDuration>
    {
        public override ItemTier Tier { get; init; } = Tier1.instance;
        public override ItemSubTier SubTier { get; init; } = null;
        public override ItemModifier[] DefaultModifiers { get; init; } = new ItemModifier[0];
        public override ItemModifier[] PossibleModifiers { get; init; } = new ItemModifier[0];
        public override string Name { get; init; } = "";
        public override string CodeName { get; init; } = "UTIL_DUR_UP";
        public override string Description { get; init; } = "Increase the duration of your utility.";
        public override string AdvancedDescription { get; init; } = "";
        public override bool RequiresConfirmation { get; init; } = false;
        public override bool IsSharable { get; init; } = true;
        public override Dictionary<EffectTag, int> EffectWeight { get; init; } = new Dictionary<EffectTag, int>
        {
            {EffectTag.IsDurationEnabler,3 },
            {EffectTag.IsUtility,3 },
            {EffectTag.IsUtilityDep,2 },
            {EffectTag.IsUtilityEnabler,3 }
        };

        public override void Init()
        {
            AbilityStats.StatsHolder<AbilitySlot>.GlobalRefreshAbilityStatVariable += ModifyStat;
        }

        public override void ItemCountChangeBehavior(Inventory inventory, InventoryItemStack itemStack, bool IsAdded = true)
        {
            // Ensure the event is only subscribed once


            inventory.InventoryOf.Utility.ThisAbilityStats.RecalculateByStatVariable(Stat.Duration);
        }

        private void ModifyStat(AbilitySlot slot, Stat casv, ModifiableStat modStat)
        {
            if (slot.SlotType != AbilitySlotType.Utility)
                return;
            ItemEffectModifier? chargeMod = Item.IfMasterHasItemAndCheckStat(this, slot.Owner, casv, Stat.Duration);
            if (chargeMod?.Positive > 0f)
            {
                float extraduration = .05f * chargeMod.Value.Positive;
                ((DurationStat)modStat).ChangeDurationPercentage(extraduration);
            }

        }
    }

    public class AbUpSpecialRange : Item<AbUpSpecialRange>
    {
        public override ItemTier Tier { get; init; } = Tier1.instance;
        public override ItemSubTier SubTier { get; init; } = null;
        public override ItemModifier[] DefaultModifiers { get; init; } = new ItemModifier[0];
        public override ItemModifier[] PossibleModifiers { get; init; } = new ItemModifier[0];
        public override string Name { get; init; } = "Standard Issue Binoculars";
        public override string CodeName { get; init; } = "SPEC_RANG_UP";
        public override string Description { get; init; } = "Increase the range of your special.";
        public override string AdvancedDescription { get; init; } = "";
        public override bool RequiresConfirmation { get; init; } = false;
        public override bool IsSharable { get; init; } = true;
        public override Dictionary<EffectTag, int> EffectWeight { get; init; } = new Dictionary<EffectTag, int>
        {
            {EffectTag.IsRangeEnabler,3 },
            {EffectTag.IsUtility,3 },
            {EffectTag.IsSpecialDep,2 },
            {EffectTag.IsSpecialEnabler,3 }
       
        };

        public override void Init()
        {
            AbilityStats.StatsHolder<AbilitySlot>.GlobalRefreshAbilityStatVariable += ModifyStat;
        }

        public override void ItemCountChangeBehavior(Inventory inventory, InventoryItemStack itemStack, bool IsAdded = true)
        {
            // Ensure the event is only subscribed once


            inventory.InventoryOf.Special.ThisAbilityStats.RecalculateByStatVariable(Stat.Range);
        }

        private void ModifyStat(AbilitySlot slot, Stat casv, ModifiableStat modStat)
        {
            if (slot.SlotType != AbilitySlotType.Special)
                return;
            ItemEffectModifier? chargeMod = Item.IfMasterHasItemAndCheckStat(this, slot.Owner, casv, Stat.Range);
            if (chargeMod?.Positive > 0f)
            {
                float extrarange = .06f * chargeMod.Value.Positive;
                ((RangeStat)modStat).ChangeRangePercentage(extrarange);
            }

        }
    }

    public class AbUpUltDamage : Item<AbUpUltDamage>
    {
        public override ItemTier Tier { get; init; } = Tier1.instance;
        public override ItemSubTier SubTier { get; init; } = null;
        public override ItemModifier[] DefaultModifiers { get; init; } = new ItemModifier[1] {Highlander.instance};
        public override ItemModifier[] PossibleModifiers { get; init; } = new ItemModifier[1] {Highlander.instance};
        public override string Name { get; init; } = "Addrenaline Pack";
        public override string CodeName { get; init; } = "ULT_DMG_UP";
        public override string Description { get; init; } = "Increase the damage of your ult.";
        public override string AdvancedDescription { get; init; } = "";
        public override bool RequiresConfirmation { get; init; } = false;
        public override bool IsSharable { get; init; } = false;
        public override Dictionary<EffectTag, int> EffectWeight { get; init; } = new Dictionary<EffectTag, int>
        {
            {EffectTag.IsDamageEnabler,3 },
            {EffectTag.IsAttack,3 },
            {EffectTag.IsUltDep,2 },
            {EffectTag.IsUltEnabler,3 }

        };

        public override void Init()
        {
            AbilityStats.StatsHolder<AbilitySlot>.GlobalRefreshAbilityStatVariable += ModifyStat;
        }

        public override void ItemCountChangeBehavior(Inventory inventory, InventoryItemStack itemStack, bool IsAdded = true)
        {
            // Ensure the event is only subscribed once


            inventory.InventoryOf.Ult.ThisAbilityStats.RecalculateByStatVariable(Stat.Damage);
        }

        private void ModifyStat(AbilitySlot slot, Stat casv, ModifiableStat modStat)
        {
            if (slot.SlotType != AbilitySlotType.Ult)
                return;
            ItemEffectModifier? chargeMod = Item.IfMasterHasItemAndCheckStat(this, slot.Owner, casv, Stat.Damage);
            if (chargeMod?.Positive > 0f)
            {
                float extradamage = .10f * chargeMod.Value.Positive;
                ((DamageStat)modStat).ChangeAdditionalDamage(extradamage);
            }

        }
    }
}
