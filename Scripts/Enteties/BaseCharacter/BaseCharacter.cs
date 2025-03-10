using Godot;
using System;
using System.Collections.Generic;
using BrannPack;
using System.ComponentModel;
using System.Linq;
using BrannPack.ItemHandling;
using System.Threading;
using BrannPack.Tiers;
using BrannPack.ItemHandling;
using System.Security.Cryptography.X509Certificates;

namespace BrannPack.Character
{
	public partial class BaseCharacter : CharacterBody2D
	{
		private static float DefaultMaxHealth;
		private static float DefaultMaxShield;
		private static float DefaultRegen;
		private static float DefaultBarrierLossRate;
		private static float DefaultDamage;
		private static float DefaultRange;
		private static float DefaultDuration;
        private static float DefaultSpeed;
		private static float DefaultCritChance;
		private static float DefaultCritDamage;

        private float AbilityDamageScale;
        private float 

        private float BaseHealth;
        private float BaseRegen;
        private float BaseMaxShield;
        private float BaseShieldRegenDelay;
        private float BaseShieldRegenRate;
        private float BaseBarrierLossRate;
        private float BaseTopSpeed;
        private float MinimumSpeed;

        private float CurrentMaxHealth;
		private float CurrentHealth;
		private float CurrentRegen;
		private float CurrentMaxShield;
		private float CurrentShieldRegenDelay;
		private float CurrentShieldRegenRate;
		private float CurrentShield;
		private float CurrentArmorGainMult;
		private float CurrentArmor;
		private float CurrentBarrierGainMult;
		private float CurrentBarrier;
		private float CurrentBarrierLossRate;
        private float CurrentSpeed => CurrentTopSpeed+CurrentTopSpeed*CurrentSpeedReduction;
        private float CurrentTopSpeed;
        private float CurrentSpeedReduction => Mathf.Min(0f, SpeedReductionResistance - SpeedReductionPercent);
        private float SpeedReductionResistance;
        private float SpeedReductionPercent;

        private float CurrentDamageResistance => PositiveDamageResistance - NegativeDamageResistance;
        private float PositiveDamageResistance;
        private float NegativeDamageResistance;

        private AttackStats BaseCharacterAttackStats;
        private Dictionary<AttackStatModSource, AttackStats> AbilityStatModifiers;
        private Dictionary<ItemFilter, AttackStats> ItemStatModifiers;
         

		private Dictionary<string, Ability> Abilities;
		private Inventory Inventory;
		private List<BaseCharacter> Minions;
		private List<BaseCharacter> Familiars;

        public partial class StatHookEventArgs : EventArgs
        {
            //Health
            public float MaxHealthMultAdd = 0f;
            public float MaxHealthFlatAdd = 0f;
            public float BaseMaxHealthAdd = 0f;

            //Regen
            public float RegenMultAdd = 0f;
            public float RegenFlatAdd = 0f;
            public float BaseRegenAdd = 0f;

            //Shield
            public float MaxShieldMultAdd = 0f;
            public float MaxShieldFlatAdd = 0f;
            public float BaseMaxShieldAdd = 0f;

            public float ShieldRegenDelayMultAdd = 0f;
            public float ShieldRegenDelayFlatAdd = 0f;
            public float BaseShieldRegenDelayAdd = 0f;

            public float ShieldRegenRateMultAdd = 0f;
            public float ShieldRegenRateFlatAdd = 0f;
            public float BaseShieldRegenRateAdd = 0f;

            //Armor
            public float ArmorGainMultAdd = 0f;
            public float ArmorGain = 0f;

            //Barrier
            public float BarrierGainMultAdd = 0f;

            public float BarrierLossRateMultAdd = 0f;
            public float BarrierLossRateFlatAdd = 0f;
            public float BaseBarrierLossRateAdd = 0f;

            //Speed
            public float TopSpeedMultAdd = 0f;
            public float TopSpeedGainFlatAdd = 0f;
            public float BaseTopSpeedAdd = 0f;

            public List<float> SpeedReductionResistance = new List<float>();
            public List<float> SpeedReductionPercent = new List<float>();

            //Resistance (-100%,100%)
            //[1-(1-PositiveResist1)(1-PositiveResist2)...]-[1-(1-NegativeResist1)(1-NegativeResist2)...]
            public List<float> ResistanceMultAdd = new List<float>();

            public Dictionary<string, float> DamageReductionFlatAdd = new Dictionary<string, float>();

            //Damage
            public Dictionary<string, float> DamageDeltMultAdd = new Dictionary<string, float>();
            public Dictionary<string, float> DamageDeltFlatAdd = new Dictionary<string, float>();

            //Range
            public Dictionary<string, float> RangeMultAdd = new Dictionary<string, float>();
            public Dictionary<string, float> RangeFlatAdd = new Dictionary<string, float>();

            //Duration
            public Dictionary<string, float> DurationMultAdd = new Dictionary<string, float>();
            public Dictionary<string, float> DurationFlatAdd = new Dictionary<string, float>();

            //CritChance
            public Dictionary<string, float> CritChanceMultAdd = new Dictionary<string, float>();
            public Dictionary<string, float> CritChanceFlatAdd = new Dictionary<string, float>();

            //CritDamage
            public Dictionary<string, float> CritDamageMultAdd = new Dictionary<string, float>();
            public Dictionary<string, float> CritDamageFlatAdd = new Dictionary<string, float>();

        }

        public event EventHandler<StatHookEventArgs> OnStatCalculation;

        public void RecalculateStats()
        {
            // Step 1: Create a new event argument object to hold stat modifications
            StatHookEventArgs statArgs = new StatHookEventArgs();

            // Step 2: Invoke the event for all listeners (items, buffs, etc.)
            OnStatCalculation?.Invoke(this, statArgs);

            // Step 3: Apply stat modifications
            ApplyStatChanges(statArgs);
        }

		protected void ApplyStatChanges(StatHookEventArgs statArgs)
		{
            //Add Logic for health scaling for adding max health 
            CurrentMaxHealth = (BaseHealth+statArgs.BaseMaxHealthAdd)*(1f + statArgs.MaxHealthMultAdd) + statArgs.MaxHealthFlatAdd;

            CurrentRegen = (BaseRegen + statArgs.BaseRegenAdd) * (1f + statArgs.RegenMultAdd) + statArgs.RegenFlatAdd;

            CurrentMaxShield = (BaseMaxShield + statArgs.BaseMaxShieldAdd) * (1f + statArgs.MaxShieldMultAdd) + statArgs.MaxShieldFlatAdd;

            CurrentShieldRegenDelay = (BaseShieldRegenDelay + statArgs.BaseShieldRegenDelayAdd) * (1f + statArgs.ShieldRegenDelayMultAdd) + statArgs.ShieldRegenRateFlatAdd;

            CurrentShieldRegenRate = (BaseShieldRegenRate + statArgs.BaseShieldRegenRateAdd) * (1f + statArgs.ShieldRegenRateMultAdd) + statArgs.ShieldRegenRateFlatAdd;

            CurrentArmorGainMult = (1f + statArgs.ArmorGainMultAdd);

            CurrentBarrierGainMult = (1f + statArgs.BarrierGainMultAdd);

            CurrentBarrierLossRate = (BaseBarrierLossRate + statArgs.BaseBarrierLossRateAdd) * (1f + statArgs.BarrierLossRateMultAdd) + statArgs.BarrierLossRateFlatAdd;

            CurrentTopSpeed = (BaseTopSpeed + statArgs.BaseTopSpeedAdd) * (1f + statArgs.TopSpeedGainFlatAdd) + statArgs.TopSpeedGainFlatAdd;

            SpeedReductionResistance = 1f - statArgs.SpeedReductionResistance.Aggregate(1f, (acc, val) => acc * (1f - val));

            SpeedReductionPercent = 1f - statArgs.SpeedReductionPercent.Aggregate(1f, (acc, val) => acc * (1f - val));

        }



    }

    public enum AttackStatModSource
    {
        All,Primary,Secondary,Utility,Special,Ult, Equipment
    }

    public struct AttackStats
    {
        //Damage is Calculated like this: Total Damage= (BaseDamage+AdditionalDamage*DamageScaling)*(1f+DamagePercentIncrease-DamagePercentDecrease);
        public float TotalDamage;
        public float BaseDamage;
        public float DamageScaling;
        public float AdditionalDamage;
        public float DamagePercentIncrease;
        public float DamagePercentDecrease;

        public float CalculateTotalDamage() { return TotalDamage=(BaseDamage + AdditionalDamage * DamageScaling) * (1f + DamagePercentIncrease - DamagePercentDecrease); }
        public void ChangeAdditionalDamage(float addDamage) { DamageScaling += addDamage; }
        public void ChangeDamageByPercent(float percentChange) 
        { 
            if(percentChange >= 0)DamagePercentIncrease += percentChange; 
            else DamagePercentDecrease*=-1f*percentChange; 
        }


        // FireRate*(1f+FireRateUpPercentage)
        public float TotalFireRate;
        public float BaseFireRate;
        public float FireRateUpPercentage;
        public float FireRateDownPercentage;
        public float FireRateMinimum;

        public float CalculateTotalFireRate() { return TotalFireRate = Mathf.Max(FireRateMinimum,(BaseFireRate * (1f + FireRateUpPercentage - FireRateDownPercentage))); }
        public void ChangeFireRatePercentage(float percentChange)
        {
            if (percentChange >= 0) FireRateUpPercentage += percentChange;
            else DamagePercentDecrease *= -1f * percentChange;
        }

        //Velocity*(1f+VelocityUpPercentage);
        public float TotalVelocity;
        public float BaseVelocity;
        public float VelocityUpPercentage;

        //ProcScale*(1f+ProcScaleUpPercentage);
        public float ProcScale;
        public float ProcScaleUpPercentage;

        //Chance*(1f+ChanceUpPercentage)
        public float Chance;
        public float ChanceUpPercentage;

        //Charges+AdditionalCharges
        public float Charges;
        public float AdditionalCharges;

        //Cooldown*(1f+CooldownIncreasedPercentage-CooldownReductionPercentage);
        public float Cooldown;
        //CooldownReductionPercentage HAS to be handled CAREFULLY. I want it to range from (0 to inf) but I want the scaling to be slow.
        //So Having to 50% reductions wont make it 100% itl be 50%*50%=25%.. So any modifications to CooldownReductionPercentage MUST be multiplied
        public float CooldownReductionPercentage;
        //CooldownIncreased Is different though. Instead well do it like this: 50%+50%=100%
        public float CooldownIncreasedPercentage;

        //SpamCooldown*(1f-SpamCooldwonIncreasedPercentageSpamCooldownReductionPercentage);
        public float SpamCooldown;
        public float SpamCooldownReductionPercentage;//Multiply to this one
        public float SpmCooldownIncreasedPercentage; //Add to this one

        //Range*(1f+RangeUpPercentage-RangeDownPercentage);
        public float Range;
        public float RangeUpPercentage; //Add to this one
        public float RangeDownPercentage; //Multiply to this one

        //Duration*(1f+DurationUpPercentage);
        public float Duration; 
        public float DurationUpPercentage; //Add to this one
        public float DurationDownPercentage; //Multiply to this one

        //CritChance+CritChanceIncrease;
        public float CritChance; //Add to this one
        public float CritChanceIncrease;
        
        //CritDamageMultiplier+CritDamageMultiplierIncrease
        public float CritDamageMultiplier; //Add to this one
        public float CritDamageMultiplierIncrease;



    }

	
}
