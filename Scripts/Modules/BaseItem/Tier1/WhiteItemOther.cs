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
    public class OnHighDamage_DealMoreAndArmor:Item<OnHighDamage_DealMoreAndArmor>
    {
        public override ItemTier Tier { get; init; } = Tier1.instance;
        public override ItemSubTier SubTier { get; init; } = null;
        public override ItemModifier[] DefaultModifiers { get; init; } = {};
        public override ItemModifier[] PossibleModifiers { get; init; } = { Highlander.instance };
        public override string Name { get; init; } = "";
        public override string CodeName { get; init; } = "OnHighDamage_DealMoreAndArmor";
        public override string Description { get; init; } = "Before dealing a large burst of damage, deal 25 more damage and gain armor. CD 10s";
        public override string AdvancedDescription { get; init; } = "";
        public override bool RequiresConfirmation { get; init; } = false;
        public override bool IsSharable { get; init; } = true;

        public CooldownStat cooldownStat = new CooldownStat(10f);
        public DamageStat extraDamage = new DamageStat(25f,2.5f);
        public DamageStat gainedArmor = new DamageStat(15f, 1.5f);
        public float requiredDamage = 50f;
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
            if(damageInfo.Damage>requiredDamage && !source.Cooldowns.IsOnCooldown((0,instance.Index,1)) 
                && source.Inventory.AllEffectiveItemCount.TryGetValue(this, out ItemEffectModifier effects))
            {
                float cooldownDuration = cooldownStat.GetCombinedTotal(Stat.Cooldown, source.Stats);
                source.Cooldowns.AddCooldown((0, instance.Index, 1), cooldownDuration);

                DamageStat extraStackDamage = new DamageStat(0, 0, 0);
                extraDamage.ChangeAdditionalDamage(effects.Positive - 1f);
                float addedDamage = extraDamage.GetCombinedTotal(extraStackDamage);

                damageInfo.Damage += addedDamage;

                eventChain.TryAddEventInfo(new AttackInfo(damageInfo.Source, damageInfo.Destination, (0, instance.Index, 1), addedDamage, false));
                float addedArmor = gainedArmor.GetCombinedTotal(extraStackDamage);

                source.HealthBar.Heal(new HealingInfo(source, source, (0, instance.Index, 1), addedArmor, null, HealthCatagory.Armor),eventChain);

            }
        }
    }
