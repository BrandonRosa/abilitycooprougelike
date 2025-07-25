﻿using BrannPack.AbilityHandling;
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

namespace BrannPack.Items
{
    public class AbUpPrimaryFireRate : Item<AbUpPrimaryFireRate>
    {
        public override ItemTier Tier { get; init; } = Tier1.instance;
        public override ItemSubTier SubTier { get; init; } = null;
        public override ItemModifier[] DefaultModifiers { get; init; } = new ItemModifier[0];
        public override ItemModifier[] PossibleModifiers { get; init; } = new ItemModifier[0];
        public override string Name { get; init; } = "";
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
                    FirerateStat.ChangeFireRatePercentage(.15f * effects.Positive);
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
        public override string Name { get; init; } = "";
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
}
