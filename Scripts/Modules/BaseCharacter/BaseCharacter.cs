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
using BrannPack.AbilityHandling;
using static BrannPack.ModifiableStats.CharacterStats;
using static BrannPack.ModifiableStats.AbilityStats;
using BrannPack.ModifiableStats;
using System.Reflection.Metadata.Ecma335;


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

        //Players are 1
        //Bosses are around a 5 or 10
        //Swarmers are like a .25
        private float AbilityScale;

        public ChanceStat Chance;
        public DamageStat Damage;
        public FireRateStat FireRate;
        public ProjectileSpeedStat ProjectileSpeed;
        public ChanceStat ProcChance;
        public DamageStat CritDamage;
        public ChargeStat Charges;
        public CooldownStat Cooldown;
        public CooldownStat SpamCooldown;
        public RangeStat Range;
        public DurationStat Duration;
        public ChanceStat Luck;





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
        //private float CurrentArmor;
        private float CurrentBarrierGainMult;
        private float CurrentBarrier;
        private float CurrentBarrierLossRate;
        private float CurrentSpeed => CurrentTopSpeed + CurrentTopSpeed * CurrentSpeedReduction;
        private float CurrentTopSpeed;
        private float CurrentSpeedReduction => Mathf.Min(0f, SpeedReductionResistance - SpeedReductionPercent);
        private float SpeedReductionResistance;
        private float SpeedReductionPercent;

        private float CurrentDamageResistance => PositiveDamageResistance - NegativeDamageResistance;
        private float PositiveDamageResistance;
        private float NegativeDamageResistance;


        private Dictionary<(StatModTarget,CharacterAbilityStatVariable), ModifiableStat> AbilityStatModifiers;
        private Dictionary<(ItemStackFilter, CharacterAbilityStatVariable), ModifiableStat> ItemStatModifiers;


        private Dictionary<string, Ability> Abilities;
        public Inventory Inventory;
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

        public static event Action<BaseCharacter, CharacterAbilityStatVariable,ModifiableStat> RefreshAbilityStatVariable;

        public enum CharacterAbilityStatVariable
        {
            Chance,
            Damage,
            FireRate,
            ProjectileSpeed,
            ProcChance,
            CritDamage,
            Charges,
            Cooldown,
            SpamCooldown,
            Range,
            Duration,
            Luck
        }

        public void RecalculateStatsOld()
        {
            // Step 1: Create a new event argument object to hold stat modifications
            StatHookEventArgs statArgs = new StatHookEventArgs();

            // Step 2: Invoke the event for all listeners (items, buffs, etc.)
            OnStatCalculation?.Invoke(this, statArgs);

            // Step 3: Apply stat modifications
            ApplyStatChanges(statArgs);
        }

        public void RecalculateAllStats()
        {
            /*
             Chance,
            Damage,
            FireRate,
            ProjectileSpeed,
            ProcChance,
            CritDamage,
            Charges,
            Cooldown,
            SpamCooldown,
            Range,
            Duration,
            Luck
             */
            RecalculateChance();
            RecalculateDamage();
            RecalculateFireRate();
            RecalculateProjectileSpeed();
            RecalculateProcChance();
            RecalculateCritDamage();
            RecalculateCharges();
            RecalculateCooldown();
            RecalculateSpamCooldown();
            RecalculateRange();
            RecalculateDuration();
            RecalculateLuck();

        }
        public void RecalculateChance() { Chance.ResetModifiedValues(); RefreshAbilityStatVariable?.Invoke(this, CharacterAbilityStatVariable.Chance, Chance); }
        public void RecalculateDamage() { Damage.ResetModifiedValues(); RefreshAbilityStatVariable?.Invoke(this, CharacterAbilityStatVariable.Damage, Damage); }
        public void RecalculateFireRate() { FireRate.ResetModifiedValues(); RefreshAbilityStatVariable?.Invoke(this, CharacterAbilityStatVariable.FireRate, FireRate); }
        public void RecalculateProjectileSpeed() { ProjectileSpeed.ResetModifiedValues(); RefreshAbilityStatVariable?.Invoke(this, CharacterAbilityStatVariable.ProjectileSpeed, ProjectileSpeed); }
        public void RecalculateProcChance() { ProcChance.ResetModifiedValues(); RefreshAbilityStatVariable?.Invoke(this, CharacterAbilityStatVariable.ProcChance, ProcChance); }
        public void RecalculateCritDamage() { CritDamage.ResetModifiedValues(); RefreshAbilityStatVariable?.Invoke(this, CharacterAbilityStatVariable.CritDamage, CritDamage); }
        public void RecalculateCharges() { Charges.ResetModifiedValues(); RefreshAbilityStatVariable?.Invoke(this, CharacterAbilityStatVariable.Charges, Charges); }
        public void RecalculateCooldown() { Cooldown.ResetModifiedValues(); RefreshAbilityStatVariable?.Invoke(this, CharacterAbilityStatVariable.Cooldown, Cooldown); }
        public void RecalculateSpamCooldown() { SpamCooldown.ResetModifiedValues(); RefreshAbilityStatVariable?.Invoke(this, CharacterAbilityStatVariable.SpamCooldown, SpamCooldown); }
        public void RecalculateRange() { Range.ResetModifiedValues(); RefreshAbilityStatVariable?.Invoke(this, CharacterAbilityStatVariable.Range, Range); }
        public void RecalculateDuration() { Duration.ResetModifiedValues(); RefreshAbilityStatVariable?.Invoke(this, CharacterAbilityStatVariable.Duration, Duration); }
        public void RecalculateLuck() { Luck.ResetModifiedValues(); RefreshAbilityStatVariable?.Invoke(this, CharacterAbilityStatVariable.Luck, Luck); }

        protected void ApplyStatChanges(StatHookEventArgs statArgs)
        {
            //Add Logic for health scaling for adding max health 
            CurrentMaxHealth = (BaseHealth + statArgs.BaseMaxHealthAdd) * (1f + statArgs.MaxHealthMultAdd) + statArgs.MaxHealthFlatAdd;

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

        public ItemEffectModifier GetItemCount(Item item, bool withEffectiveness)
        {

        }

    }

    public class HealthBar
    {
        public List<HealthType> HealthTypes = new List<HealthType>() {};
        protected List<(HealthCatagory,float)> CurrentHealth;

        public static float GetTotalCurrentHealth(List<(HealthCatagory, float)> currentHealth)
        {
            return currentHealth.Sum(curCat => curCat.Item2);
        }
        public HealthBar(List<HealthType> healthTypes, EffectivenessStat damageResistance, EffectivenessStat movementSlow, EffectivenessStat healingEffectiveness)
        {
            HealthTypes = healthTypes;
            DamageResistance = damageResistance;
            MovementSlow = movementSlow;
            HealingEffectiveness = healingEffectiveness;
        }

        public List<(HealthCatagory, float)> CalculateCurrentHealth()
        {
            List<(HealthCatagory, float)> temp = new List<(HealthCatagory, float)>();
            HealthTypes.ForEach(health=>temp.Add((health.Catagory,health.CurrentValue)));
            return temp;
            
        }
        public float UpdateCurrentHealth()
        {
            List<(HealthCatagory, float)> oldHealth = CurrentHealth;
            CurrentHealth = CalculateCurrentHealth();
            return GetTotalCurrentHealth(CurrentHealth) - GetTotalCurrentHealth(oldHealth);
        }

        //Returns the amount of ActualDamage taken.
        public float TakeDamage(float damageTaken)
        {
            HealthTypes.AsEnumerable().Reverse().Aggregate(damageTaken, (leftoverDamge, currentType) => currentType.TakeDamage(damageTaken));
            return UpdateCurrentHealth();
        }

        public float Heal()
        {

        }

        public float GetMaxHealth()
        {

        }

        public float GetMaxShield()
        {

        }
    }

    

    public enum StatModTarget
    {
        All,Primary,Secondary,Utility,Special,Ult, Equipment
    }

    

	
}
