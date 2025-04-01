using BrannPack.AbilityHandling;
using BrannPack.Character;
using BrannPack.CooldownHandling;
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
using static BrannPack.ModifiableStats.CharacterStats;

namespace BrannPack.Items
{
    public class OnHighDamage_DealMoreAndArmor : Item<OnHighDamage_DealMoreAndArmor>
    {
        public override ItemTier Tier { get; init; } = Tier1.instance;
        public override ItemSubTier SubTier { get; init; } = null;
        public override ItemModifier[] DefaultModifiers { get; init; } = { };
        public override ItemModifier[] PossibleModifiers { get; init; } = { Highlander.instance };
        public override string Name { get; init; } = "";
        public override string CodeName { get; init; } = "OnHighDamage_DealMoreAndArmor";
        public override string Description { get; init; } = "Before dealing a large burst of damage, deal 25 more damage and gain armor. CD 10s";
        public override string AdvancedDescription { get; init; } = "";
        public override bool RequiresConfirmation { get; init; } = false;
        public override bool IsSharable { get; init; } = true;


        public DamageStat damage = new DamageStat(25f, .8f);
        public CooldownStat cooldown = new CooldownStat(10f);
        public float initialArmor = 15f;
        public float armorPerStack = 1f; //1.2f
        public float requiredDamage = 50f;
        public float stackToDamageRatio = 1.25f;
        public override Dictionary<EffectTag, int> EffectWeight { get; init; } = new Dictionary<EffectTag, int>
        {
            {EffectTag.IsArmorEnabler,2},
            {EffectTag.IsAttack,2 },
            {EffectTag.IsDefensive,1 },
            {EffectTag.IsCooldownDep,1 },
            {EffectTag.IsDamageDep,3 },
            {EffectTag.IsDamageEnabler,2 },
            {EffectTag.IsHighDamageHitDep,3 },
            {EffectTag.IsHighDamageHitEnabler,2 }

        };

        public override void Init()
        {
            // Ensure the event is only subscribed once
            CharacterMaster.BeforeDealDamage += HighDamageHit;
        }

        private void HighDamageHit(CharacterMaster source, CharacterMaster victim, DamageInfo damageInfo, EventChain eventChain)
        {
            if (damageInfo.Damage > requiredDamage && !source.Cooldowns.IsOnCooldown((0, instance.Index, 1))
                && source.Inventory.AllEffectiveItemCount.TryGetValue(this, out ItemEffectModifier effects))
            {
                //Prep For BeforeAttack
                StatsHolder attackstat = new StatsHolder();
                DamageStat instDamage = damage.Copy();
                instDamage.ChangeAdditionalDamage((effects.Positive - 1f) * 1.25f);

                CooldownStat instCooldown = cooldown.Copy();
                instCooldown.AddCombinedStats(source.Stats.GetStatByVariable<CooldownStat>(Stat.Cooldown));

                attackstat.SetStat(Stat.Damage, instDamage);
                attackstat.SetStat(Stat.Cooldown, instCooldown);

                AttackInfo attackInfo = new AttackInfo(source, victim, (0, instance.Index, 1), damageInfo.IsCrit, attackstat);


                source.BeforeAttack(attackInfo, eventChain);

                float cooldownDuration = instCooldown.CalculateTotal();
                source.Cooldowns.AddCooldown((0, instance.Index, 1), cooldownDuration);

                damageInfo.Damage += instDamage.CalculateTotal();
                eventChain.TryAddEventInfo(attackInfo);

                float addedArmor = initialArmor + (effects.Positive - 1f) * armorPerStack;

                source.HealthBar.Heal(new HealingInfo(source, source, (0, instance.Index, 1), addedArmor, null, HealthCatagory.Armor), eventChain);


                source.AfterAttack(attackInfo, eventChain);
            }
        }
    }
    public class Cth_LSUp_FRDn : Item<Cth_LSUp_FRDn>
    {
        public override ItemTier Tier { get; init; } = Tier1.instance;
        public override ItemSubTier SubTier { get; init; } = null;
        public override ItemModifier[] DefaultModifiers { get; init; } = { Chthonic.instance };
        public override ItemModifier[] PossibleModifiers { get; init; } = { Chthonic.instance, Highlander.instance };
        public override string Name { get; init; } = "";
        public override string CodeName { get; init; } = "Cth_LSUp_FRDn";
        public override string Description { get; init; } = "Your Primary gains Lifesteal but loses firerate";
        public override string AdvancedDescription { get; init; } = "";
        public override bool RequiresConfirmation { get; init; } = false;
        public override bool IsSharable { get; init; } = true;

        public float primaryLifesteal = .07f;
        public float primaryFireRateDown = .05f;
        public override Dictionary<EffectTag, int> EffectWeight { get; init; } = new Dictionary<EffectTag, int>
        {
            {EffectTag.IsAttack,-1 }
            {EffectTag.IsDamageDep,2 },
            {EffectTag.IsDefensive,2 },
            {EffectTag.IsFireRateDep,2 },
            {EffectTag.IsFireRateEnabler,-1 }
            {EffectTag.IsHealEnabler,2 },
            {EffectTag.IsPrimaryDep,3 }

        };

        public override void Init()
        {
            // Ensure the event is only subscribed once
            StatsHolder<AbilitySlot>.GlobalRefreshAbilityStatVariable += ApplyStats;
        }

        public override void SetItemEffects(Inventory inventory, ItemEffectModifier changes, ItemEffectModifier totalItems, bool IsAdded = true)
        {
            inventory.InventoryOf.Stats.RecalculateByStatVariable(Stat.Lifesteal);
            inventory.InventoryOf.Stats.RecalculateByStatVariable(Stat.FireRate);
        }

        private void ApplyStats(AbilitySlot abilitySlot, Stat statType, ModifiableStat stat)
        {
            if(abilitySlot.Owner.UsingInventory && abilitySlot.SlotType==AbilitySlotType.Primary && (statType==Stat.Lifesteal || statType==Stat.FireRate)
                && abilitySlot.Owner.Inventory.AllEffectiveItemCount.TryGetValue(this, out ItemEffectModifier effects))
            {
                if(statType==Stat.Lifesteal)
                {
                    ((EffectivenessStat)stat).ChangeAdditivePercentage(primaryLifesteal * effects.Positive / Chthonic.instance.itemEffectModifier.Positive);
                }
                else
                {
                    float fireRateDown = -(1f-MathF.Pow(1 - primaryFireRateDown, effects.Negative / Chthonic.instance.itemEffectModifier.Negative));
                    ((FireRateStat)stat).ChangeFireRatePercentage(fireRateDown);
                }
            }
        }
    }
}
