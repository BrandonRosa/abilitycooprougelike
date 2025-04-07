using BrannPack.AbilityHandling;
using BrannPack.Character;
using BrannPack.CooldownHandling;
using BrannPack.ItemHandling;
using BrannPack.ModifiableStats;
using BrannPack.StatusEffectHandling;
using BrannPack.Tiers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BrannPack.Character.BaseCharacterBody;
using static BrannPack.ModifiableStats.AbilityStats;
using static BrannPack.ModifiableStats.CharacterStats;

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

        public override void Init()
        {
            BaseCharacterBody.AfterMovementRestricted += ItemEffect;
        }

        private void ItemEffect(BaseCharacterBody baseCharacter,float duration)
        {
            var master = baseCharacter.CharacterMaster;
            if (master.Inventory.AllEffectiveItemCount.TryGetValue(this, out ItemEffectModifier effects) 
                && !master.Cooldowns.IsOnCooldown((0,instance.Index,1)))
            {
                float ArmorAmount=  .4f*master.HealthBar.GetMaxHealth() * effects.Positive / Highlander.instance.itemEffectModifier.Multiplier;
                float cooldownDuration = cooldownStat.GetCombinedTotal(Stat.Cooldown,master.Stats);
                master.Cooldowns.AddCooldown((0, instance.Index, 1),cooldownDuration);
               master.HealthBar.Heal(ArmorAmount, null, CharacterStats.HealthBehavior.Armor);
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
        public DurationStat durationStat = new DurationStat(5f,25f);
        public float SpeedPercentIncrease = .15f;
        public override Dictionary<EffectTag, int> EffectWeight { get; init; } = new Dictionary<EffectTag, int>
        {
            {EffectTag.IsUtility,3 },
            {EffectTag.IsUtilityEnabler,3 },
            {EffectTag.IsUtilityDep,2 },
            {EffectTag.IsChargeEnabler,3 },
            {EffectTag.IsDurationDep,1 },
            {EffectTag.IsMoveSpeedEnabler,2 }

        };

        public override void Init()
        {
            // Ensure the event is only subscribed once

            //Stats.StatsHolder<AbilitySlot>.RefreshAbilityStatVariable -= ModifyDamageStat;
            StatsHolder<AbilitySlot>.GlobalRefreshAbilityStatVariable += ModifyChargeStat;
            StatsHolder<CharacterMaster>.GlobalRefreshAbilityStatVariable += ModifyMoveSpeed;
            AbilitySlot.AfterAbilitySlotUse += OnUseItemEffect;
            
        }

        

        public override void SetItemEffects(Inventory inventory, ItemEffectModifier changes, ItemEffectModifier totalItems, bool IsAdded = true)
        {
            inventory.InventoryOf.Utility.ThisAbilityStats.RecalculateByStatVariable(Stat.Charges);
        }

        private void ModifyChargeStat(AbilitySlot abilitySlot, Stat variable, ModifiableStat stat)
        {
            if(abilitySlot.SlotType == AbilitySlotType.Utility && variable==Stat.Charges && abilitySlot.Owner.Inventory.AllEffectiveItemCount.TryGetValue(this, out ItemEffectModifier effects))
            {
                float charges = 1f * effects.Positive / Highlander.instance.itemEffectModifier.Positive;
                //abilitySlot.ThisAbilityStats.GetStatByVariable<ChargeStat>(Stat.Charges)
                ((ChargeStat)stat).ChangeAdditionalCharges(charges);
            }
        }

        private void ModifyMoveSpeed(CharacterMaster master, Stat variable, ModifiableStat stat)
        {
            if (variable == Stat.MoveSpeed && master.Cooldowns.IsOnCooldown((0, instance.Index, 1)))
            {
                ((MoveSpeedStat)stat).ChangeFlatSpeedPercentage(SpeedPercentIncrease);
            }
        }

        private void OnUseItemEffect(AbilitySlot abilitySlot)
        {
            if (abilitySlot.SlotType==AbilitySlotType.Utility && abilitySlot.Owner.Inventory.AllEffectiveItemCount.TryGetValue(this, out ItemEffectModifier effects)
                && !abilitySlot.Owner.Cooldowns.IsOnCooldown((0, instance.Index, 1)))
            {
                CharacterMaster master = abilitySlot.Owner;
                float duration = durationStat.GetCombinedTotal(Stat.Duration, master.Stats);

                master.Cooldowns.AddCooldown((0, instance.Index, 1), duration).CompletedCooldown+=(Cooldown cooldown) => { master.Stats.RecalculateByStatVariable(Stat.MoveSpeed); };
                master.Stats.RecalculateByStatVariable(Stat.MoveSpeed);
            }
        }

    }
}
