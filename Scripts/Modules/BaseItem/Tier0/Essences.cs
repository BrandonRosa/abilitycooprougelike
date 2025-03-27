using BrannPack;
using BrannPack.Character;
using BrannPack.ItemHandling;
using BrannPack.ModifiableStats;
using BrannPack.Tiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static BrannPack.Character.BaseCharacterBody;

namespace AbilityCoopRougelike.Items
{
    public class EOLife : Item<EOLife>
    {
        public override ItemTier Tier { get; init; } =Tier0.instance;
        public override ItemSubTier SubTier { get; init; } =ItemSubTier.Essences;
        public override ItemModifier[] DefaultModifiers { get; init; } = new ItemModifier[0];
        public override ItemModifier[] PossibleModifiers { get; init; } =new ItemModifier[0];
        public override string Name { get; init; } = "Essence of Life";
        public override string CodeName { get; init; } = "EO_Health";
        public override string Description { get; init; } ="Slightly Increase Max Health";
        public override string AdvancedDescription { get; init; } ="";
        public override bool RequiresConfirmation { get; init; } = false;
        public override bool IsSharable { get; init; } = true;
        public override Dictionary<EffectTag, int> EffectWeight { get; init; } = new Dictionary<EffectTag, int> 
        {
            {EffectTag.IsDefensive,3 }, //Gaining Health Should Be a 3
            {EffectTag.IsHealthEnabler,3 }, //Gives more Health
            {EffectTag.IsLowHPEnabler,2 }, //Gives you more room to work with if youre low on HP
            //{EffectTag.IsTealEnabler,1 } //Doest enable you Teal items, it IS a teal item. If it gave you teal items it would be different
        };

        public void Subscribe(BaseCharacterBody character)
        {
            character.OnStatCalculation += ApplyStats;
        }

        private void ApplyStats(object sender, StatHookEventArgs e)
        {
            e.MaxHealthMultAdd += 0.03f; // 3% health boost
        }
    }

    public class EORecovery : Item<EORecovery>
    {
        public override ItemTier Tier { get; init; } = Tier0.instance;
        public override ItemSubTier SubTier { get; init; } = ItemSubTier.Essences;
        public override ItemModifier[] DefaultModifiers { get; init; } = new ItemModifier[0];
        public override ItemModifier[] PossibleModifiers { get; init; } = new ItemModifier[0];
        public override string Name { get; init; } = "Essence of Recovery";
        public override string CodeName { get; init; } = "EO_Regen";
        public override string Description { get; init; } = "Slightly Increase Regen";
        public override string AdvancedDescription { get; init; } = "";
        public override bool RequiresConfirmation { get; init; } = false;
        public override bool IsSharable { get; init; } = true;
        public override Dictionary<EffectTag, int> EffectWeight { get; init; } = new Dictionary<EffectTag, int>
        {
            {EffectTag.IsDefensive,2 }, //Gaining Regen Should Be a 2
            {EffectTag.IsRegenEnabler,3 }, 
            {EffectTag.IsHighHPEnabler,1 }, 
            {EffectTag.IsHealEnabler,1 }
            
        };

        public void Subscribe(BaseCharacterBody character)
        {
            character.OnStatCalculation += ApplyStats;
        }

        private void ApplyStats(object sender, StatHookEventArgs e)
        {
            e.RegenMultAdd += 0.03f; // 3% base regen boost
        }
    }

    public class EOResiliance : Item<EOResiliance>
    {
        public override ItemTier Tier { get; init; } = Tier0.instance;
        public override ItemSubTier SubTier { get; init; } = ItemSubTier.Essences;
        public override ItemModifier[] DefaultModifiers { get; init; } = new ItemModifier[0];
        public override ItemModifier[] PossibleModifiers { get; init; } = new ItemModifier[0];
        public override string Name { get; init; } = "Essence of Resiliance";
        public override string CodeName { get; init; } = "EO_Resistance";
        public override string Description { get; init; } = "Slightly Increase Damage Resistance";
        public override string AdvancedDescription { get; init; } = "";
        public override bool RequiresConfirmation { get; init; } = false;
        public override bool IsSharable { get; init; } = true;
        public override Dictionary<EffectTag, int> EffectWeight { get; init; } = new Dictionary<EffectTag, int>
        {
            {EffectTag.IsDefensive,3 }, 
            {EffectTag.IsResistanceEnabler,3}

        };

        public void Subscribe(BaseCharacterBody character)
        {
            character.OnStatCalculation += ApplyStats;
        }

        private void ApplyStats(object sender, StatHookEventArgs e)
        {
            e.ResistanceMultAdd.Add(.015f);
        }
    }

    public class EOVelocity : Item<EOVelocity>
    {
        public override ItemTier Tier { get; init; } = Tier0.instance;
        public override ItemSubTier SubTier { get; init; } = ItemSubTier.Essences;
        public override ItemModifier[] DefaultModifiers { get; init; } = new ItemModifier[0];
        public override ItemModifier[] PossibleModifiers { get; init; } = new ItemModifier[0];
        public override string Name { get; init; } = "Essence of Velocity";
        public override string CodeName { get; init; } = "EO_Movespeed";
        public override string Description { get; init; } = "Slightly Increase Move Speed";
        public override string AdvancedDescription { get; init; } = "";
        public override bool RequiresConfirmation { get; init; } = false;
        public override bool IsSharable { get; init; } = true;
        public override Dictionary<EffectTag, int> EffectWeight { get; init; } = new Dictionary<EffectTag, int>
        {
            {EffectTag.IsUtility,2 }, //Gaining Regen Should Be a 2
            {EffectTag.IsDefensive,1 },
            {EffectTag.IsMoveSpeedEnabler,3}

        };

        public void Subscribe(BaseCharacterBody character)
        {
            character.OnStatCalculation += ApplyStats;
        }

        private void ApplyStats(object sender, StatHookEventArgs e)
        {
            e.TopSpeedMultAdd += 0.03f; // 3% base regen boost
        }
    }

    public class EOInfluence : Item<EOInfluence>
    {
        public override ItemTier Tier { get; init; } = Tier0.instance;
        public override ItemSubTier SubTier { get; init; } = ItemSubTier.Essences;
        public override ItemModifier[] DefaultModifiers { get; init; } = new ItemModifier[0];
        public override ItemModifier[] PossibleModifiers { get; init; } = new ItemModifier[0];
        public override string Name { get; init; } = "Essence of Influence";
        public override string CodeName { get; init; } = "EO_Range";
        public override string Description { get; init; } = "Slightly Increase Range";
        public override string AdvancedDescription { get; init; } = "";
        public override bool RequiresConfirmation { get; init; } = false;
        public override bool IsSharable { get; init; } = true;
        public override Dictionary<EffectTag, int> EffectWeight { get; init; } = new Dictionary<EffectTag, int>
        {
            {EffectTag.IsUtility,2 }, //Gaining Regen Should Be a 2
            {EffectTag.IsAttack,1 },
            {EffectTag.IsAOEAllyEnabler,2 },
            {EffectTag.IsAOEEnemyEnabler,2 }

        };

        public override void SetItemEffects(BaseCharacterBody baseCharacter, ItemEffectModifier itemsAdded, ItemEffectModifier totalItems, bool IsAdded = true)
        {
            // Ensure the event is only subscribed once
            AbilityStats.AbilityStatsHolder<BaseCharacterBody>.RefreshAbilityStatVariable -= ModifyRangeStat;
            AbilityStats.AbilityStatsHolder<BaseCharacterBody>.RefreshAbilityStatVariable += ModifyRangeStat;
            baseCharacter.RecalculateRange();
        }

        private void ModifyRangeStat(BaseCharacterBody baseCharacter, CharacterAbilityStatVariable casv, ModifiableStat modStat)
        {
            if (casv == CharacterAbilityStatVariable.Range && baseCharacter.Inventory.AllEffectiveItemCount.TryGetValue(this, out ItemEffectModifier effects))
            {
                if (modStat is AbilityStats.RangeStat rangeStat)
                {
                    rangeStat.RangeFlatPercentage += 0.03f * effects.Positive;
                }
            }
        }
    }

    public class EOVigor : Item<EOVigor>
    {
        public override ItemTier Tier { get; init; } = Tier0.instance;
        public override ItemSubTier SubTier { get; init; } = ItemSubTier.Essences;
        public override ItemModifier[] DefaultModifiers { get; init; } = new ItemModifier[0];
        public override ItemModifier[] PossibleModifiers { get; init; } = new ItemModifier[0];
        public override string Name { get; init; } = "Essence of Vigor";
        public override string CodeName { get; init; } = "EO_Cooldown";
        public override string Description { get; init; } = "Slightly Reduce Cooldown";
        public override string AdvancedDescription { get; init; } = "";
        public override bool RequiresConfirmation { get; init; } = false;
        public override bool IsSharable { get; init; } = true;
        public override Dictionary<EffectTag, int> EffectWeight { get; init; } = new Dictionary<EffectTag, int>
        {
            {EffectTag.IsUtility,3 }, //Gaining Regen Should Be a 2
            {EffectTag.IsChargeEnabler,1 },
            {EffectTag.IsCooldownEnabler,3 },
            {EffectTag.IsSeconaryEnabler,1 },
            {EffectTag.IsSpecialEnabler,1 },
            {EffectTag.IsUltEnabler,1 },
            {EffectTag.IsUtilityEnabler,1 },
            

        };

        public override void SetItemEffects(BaseCharacterBody baseCharacter, ItemEffectModifier itemsAdded, ItemEffectModifier totalItems, bool IsAdded = true)
        {
            // Ensure the event is only subscribed once
            AbilityStats.AbilityStatsHolder<BaseCharacterBody>.RefreshAbilityStatVariable -= ModifyCooldownStat;
            AbilityStats.AbilityStatsHolder<BaseCharacterBody>.RefreshAbilityStatVariable += ModifyCooldownStat;
            baseCharacter.RecalculateCooldown();
        }

        private void ModifyCooldownStat(BaseCharacterBody baseCharacter, CharacterAbilityStatVariable casv, ModifiableStat modStat)
        {
            if (casv == CharacterAbilityStatVariable.Cooldown && baseCharacter.Inventory.AllEffectiveItemCount.TryGetValue(this, out ItemEffectModifier effects))
            {
                if (modStat is AbilityStats.CooldownStat cooldownStat)
                {
                    cooldownStat.ChangeCooldownByPercent(-0.05f * effects.Positive);
                }
            }
        }
    }

    public class EOStrength : Item<EOStrength>
    {
        public override ItemTier Tier { get; init; } = Tier0.instance;
        public override ItemSubTier SubTier { get; init; } = ItemSubTier.Essences;
        public override ItemModifier[] DefaultModifiers { get; init; } = new ItemModifier[0];
        public override ItemModifier[] PossibleModifiers { get; init; } = new ItemModifier[0];
        public override string Name { get; init; } = "Essence of Strength";
        public override string CodeName { get; init; } = "EO_Strength";
        public override string Description { get; init; } = "Slightly Increase Damage";
        public override string AdvancedDescription { get; init; } = "";
        public override bool RequiresConfirmation { get; init; } = false;
        public override bool IsSharable { get; init; } = true;
        public override Dictionary<EffectTag, int> EffectWeight { get; init; } = new Dictionary<EffectTag, int>
        {
            {EffectTag.IsAttack,3 },
            {EffectTag.IsAOEEnemyEnabler,1 },
            {EffectTag.IsDamageEnabler,2 },
            {EffectTag.IsHighDamageHitEnabler,1 }

        };

        public override void SetItemEffects(BaseCharacterBody baseCharacter, ItemEffectModifier itemsAdded, ItemEffectModifier totalItems, bool IsAdded = true)
        {
            // Ensure the event is only subscribed once
            
            AbilityStats.AbilityStatsHolder<BaseCharacterBody>.RefreshAbilityStatVariable -= ModifyDamageStat;
            AbilityStats.AbilityStatsHolder<BaseCharacterBody>.RefreshAbilityStatVariable += ModifyDamageStat;

            baseCharacter.AbilityStats.RecalculateDamage();
        }

        private void ModifyDamageStat(BaseCharacterBody baseCharacter, CharacterAbilityStatVariable casv, ModifiableStat modStat)
        {
            if (casv == CharacterAbilityStatVariable.Damage && baseCharacter.Inventory.AllEffectiveItemCount.TryGetValue(this, out ItemEffectModifier effects))
            {
                if (modStat is AbilityStats.DamageStat DamageStat)
                {
                    DamageStat.ChangeAdditionalDamage(2.5f * effects.Positive);
                }
            }
        }
    }
}
