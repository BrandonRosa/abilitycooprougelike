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
using static BrannPack.Character.BaseCharacter;

namespace BrannPack.Items
{
    public class AbUpPrimaryFireRate : Item<AbUpPrimaryFireRate>
    {
        public override ItemTier Tier { get; init; } = Tier1.instance;
        public override ItemSubTier SubTier { get; init; } = null;
        public override ItemModifier[] DefaultModifiers { get; init; } = new ItemModifier[0];
        public override ItemModifier[] PossibleModifiers { get; init; } = new ItemModifier[0];
        public override string Name { get; init; } = "";
        public override string CodeName { get; init; } = "AbUp_PrimaryFireRate";
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

        public override void SetItemEffects(BaseCharacter baseCharacter, ItemEffectModifier itemsAdded, ItemEffectModifier totalItems, bool IsAdded = true)
        {
            // Ensure the event is only subscribed once

            AbilityStats.AbilityStatsHolder<AbilitySlot>.RefreshAbilityStatVariable -= ModifyDamageStat;
            AbilityStats.AbilityStatsHolder<AbilitySlot>.RefreshAbilityStatVariable += ModifyDamageStat;

            baseCharacter.AbilityStats.RecalculateDamage();
        }

        private void ModifyDamageStat(AbilitySlot slot, CharacterAbilityStatVariable casv, ModifiableStat modStat)
        {
            if (casv == CharacterAbilityStatVariable.FireRate && slot.SlotType==AbilitySlotType.Primary && slot.Owner.Inventory.AllEffectiveItemCount.TryGetValue(this, out ItemEffectModifier effects))
            {
                if (modStat is AbilityStats.FireRateStat FirerateStat)
                {
                    FirerateStat.ChangeFireRatePercentage(.15f * effects.Positive);
                }
            }
        }
    }
}
