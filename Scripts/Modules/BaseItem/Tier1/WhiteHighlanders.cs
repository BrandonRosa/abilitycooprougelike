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

namespace BrannPack.Items
{
    public class ArmorOnMovementRestrict : Item<ArmorOnMovementRestrict>
    {
        public override ItemTier Tier { get; init; } = Tier1.instance;
        public override ItemSubTier SubTier { get; init; } = null;
        public override ItemModifier[] DefaultModifiers { get; init; } = { Highlander.instance };
        public override ItemModifier[] PossibleModifiers { get; init; } = {Highlander.instance};
        public override string Name { get; init; } = "";
        public override string CodeName { get; init; } = "ArmorOnMovementRestrict";
        public override string Description { get; init; } = "When movement restricted, gain 40% of your HP in Armor. Has a 15s Cooldown.";
        public override string AdvancedDescription { get; init; } = "";
        public override bool RequiresConfirmation { get; init; } = false;
        public override bool IsSharable { get; init; } = false;

        public CooldownStat cooldownStat=new CooldownStat(15f);
        public override Dictionary<EffectTag, int> EffectWeight { get; init; } = new Dictionary<EffectTag, int>
        {
            {EffectTag.IsDefensive,3 },
            {EffectTag.IsArmorEnabler,3 },
            {EffectTag.IsCloseRangeEnabler,1 },
            {EffectTag.IsCooldownDep,1 },
            {EffectTag.IsInDangerDep,1 },
            {EffectTag.IsPercentYourHealthDep,3 }

        };

        public override void SetItemEffects(BaseCharacterBody baseCharacter, ItemEffectModifier itemsAdded, ItemEffectModifier totalItems, bool IsAdded = true)
        {
            // Ensure the event is only subscribed once

            //AbilityStats.AbilityStatsHolder<AbilitySlot>.RefreshAbilityStatVariable -= ModifyDamageStat;
            //AbilityStats.AbilityStatsHolder<AbilitySlot>.RefreshAbilityStatVariable += ModifyDamageStat;
            BaseCharacterBody.AfterMovementRestricted += ItemEffect;
            //baseCharacter.AbilityStats.RecalculateDamage();
        }

        private void ItemEffect(BaseCharacterBody baseCharacter,float duration)
        {
            if (baseCharacter.Inventory.AllEffectiveItemCount.TryGetValue(this, out ItemEffectModifier effects) 
                && !baseCharacter.ItemCooldowns.IsOnCooldown(this,1))
            {
                float ArmorAmount=  .4fbaseCharacter.HealthBar.GetMaxHealth() * effects.Positive / Highlander.instance.itemEffectModifier.Multiplier;
                float Duration = cooldownStat.GetCombinedTotal(baseCharacter.AbilityStats.Cooldown);
                baseCharacter.ItemCooldowns.AddCooldown(this, 1, Duration);
                baseCharacter.HealthBar.Heal(ArmorAmount, null, CharacterStats.HealthCatagory.Armor);
            }
        }
    }

    public class UtilityChargeAndMoveSpeedOnUse : Item<UtilityChargeAndMoveSpeedOnUse>
    {
        public override ItemTier Tier { get; init; } = Tier1.instance;
        public override ItemSubTier SubTier { get; init; } = null;
        public override ItemModifier[] DefaultModifiers { get; init; } = { Highlander.instance };
        public override ItemModifier[] PossibleModifiers { get; init; } = { Highlander.instance };
        public override string Name { get; init; } = "";
        public override string CodeName { get; init; } = "UtilityChargeAndMoveSpeedOnUse";
        public override string Description { get; init; } = "Gain a charge on your utility. Gain movespeed after using it.";
        public override string AdvancedDescription { get; init; } = "";
        public override bool RequiresConfirmation { get; init; } = false;
        public override bool IsSharable { get; init; } = false;

        public CooldownStat cooldownStat = new CooldownStat(15f);
        public override Dictionary<EffectTag, int> EffectWeight { get; init; } = new Dictionary<EffectTag, int>
        {
            {EffectTag.IsUtility,3 },
            {EffectTag.IsUtilityEnabler,3 },
            {EffectTag.IsUtilityDep,2 },
            {EffectTag.IsChargeEnabler,3 },
            {EffectTag.IsDurationDep,1 },
            {EffectTag.IsMoveSpeedEnabler,2 }

        };

        public override void SetItemEffects(BaseCharacterBody baseCharacter, ItemEffectModifier itemsAdded, ItemEffectModifier totalItems, bool IsAdded = true)
        {
            // Ensure the event is only subscribed once

            //AbilityStats.AbilityStatsHolder<AbilitySlot>.RefreshAbilityStatVariable -= ModifyDamageStat;
            AbilityStats.AbilityStatsHolder<AbilitySlot>.RefreshAbilityStatVariable += ModifyChargeStat;
            AbilitySlot.AfterAbilitySlotUse += OnUseItemEffect;
            baseCharacter.Utility.ThisAbilityStats.RecalculateCharges();
        }

        private void ModifyChargeStat(AbilitySlot abilitySlot, CharacterAbilityStatVariable variable, ModifiableStat stat)
        {
            if(abilitySlot.SlotType == AbilitySlotType.Utility && abilitySlot.Owner.Inventory.AllEffectiveItemCount.TryGetValue(this, out ItemEffectModifier effects))
            {
                abilitySlot.ThisAbilityStats.Charges.ChangeAdditionalCharges(1f);
            }
        }

        private void OnUseItemEffect(AbilitySlot abilitySlot)
        {
            if (abilitySlot.SlotType==AbilitySlotType.Utility && abilitySlot.Owner.Inventory.AllEffectiveItemCount.TryGetValue(this, out ItemEffectModifier effects)
                && !abilitySlot.Owner.ItemCooldowns.IsOnCooldown(this, 1))
            {
                //Add movespeed up buff, if its already there, refresh it instead. 
            }
        }
    }
}
