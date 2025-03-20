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

        //public ChanceStat Chance;
        //public DamageStat Damage;
        //public FireRateStat FireRate;
        //public ProjectileSpeedStat ProjectileSpeed;
        //public ChanceStat ProcChance;
        //public DamageStat CritDamage;
        //public ChargeStat Charges;
        //public CooldownStat Cooldown;
        //public CooldownStat SpamCooldown;
        //public RangeStat Range;
        //public DurationStat Duration;
        //public ChanceStat Luck;

        public AbilityStatsHolder<BaseCharacter> AbilityStats;

        AbilitySlot Primary;
        AbilitySlot Secondary;
        AbilitySlot Utility;
        AbilitySlot Special;
        AbilitySlot Ult;

        public Vector2 AttackDirection;
        public Vector2 MoveDirection;



        //private float BaseHealth;
        //private float BaseRegen;
        //private float BaseMaxShield;
        //private float BaseShieldRegenDelay;
        //private float BaseShieldRegenRate;
        //private float BaseBarrierLossRate;
        //private float BaseTopSpeed;
        //private float MinimumSpeed;

        //private float CurrentMaxHealth;
        //private float CurrentHealth;
        //private float CurrentRegen;
        //private float CurrentMaxShield;
        //private float CurrentShieldRegenDelay;
        //private float CurrentShieldRegenRate;
        //private float CurrentShield;
        //private float CurrentArmorGainMult;
        ////private float CurrentArmor;
        //private float CurrentBarrierGainMult;
        //private float CurrentBarrier;
        //private float CurrentBarrierLossRate;
        //private float CurrentSpeed => CurrentTopSpeed + CurrentTopSpeed * CurrentSpeedReduction;
        //private float CurrentTopSpeed;
        //private float CurrentSpeedReduction => Mathf.Min(0f, SpeedReductionResistance - SpeedReductionPercent);
        //private float SpeedReductionResistance;
        //private float SpeedReductionPercent;

        //private float CurrentDamageResistance => PositiveDamageResistance - NegativeDamageResistance;
        //private float PositiveDamageResistance;
        //private float NegativeDamageResistance;


        private Dictionary<(StatModTarget, CharacterAbilityStatVariable), ModifiableStat> AbilityStatModifiers;
        private Dictionary<(ItemStackFilter, CharacterAbilityStatVariable), ModifiableStat> ItemStatModifiers;


        private Dictionary<string, Ability> Abilities;
        public Inventory Inventory;
        private List<BaseCharacter> Minions;
        private List<BaseCharacter> Familiars;



        public static event Action<BaseCharacter, CharacterAbilityStatVariable, ModifiableStat> RefreshAbilityStatVariable;
        public static event Action<BaseCharacter, float> IsMovementRestricted;

        public bool IsMovementRestricted()
        {
            return false;
        }

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
            Luck,
            Lifesteal
        }


    }

    public class HealthBar
    {
        public BaseCharacter Owner;
        public List<HealthType> HealthTypes = new List<HealthType>() { };
        protected List<(HealthCatagory, float)> CurrentHealth;
        public EffectivenessStat DamageResistance;
        public EffectivenessStat HealingEffectiveness;

        public void Init()
        {
            Health health = new Health(0f, new MaxHealthStat(100f));

            Armor armor = new Armor(0f, new MaxHealthStat(0f));
            armor.MaxValue.AddFollowingMaxHealth(health.MaxValue);

            Shield shield = new Shield(0f, new MaxHealthStat(0f));

            BarrierHealth barrier = new BarrierHealth(0f, new MaxHealthStat(0f));
            barrier.MaxValue.AddFollowingMaxHealth(health.MaxValue);
            armor.AfterCurrentValueChange += (float currentValue, float valueadded, float overvalue) =>
            {
                if (currentValue > health.MaxValue.Total)
                    barrier.MaxValue.ChangeAdditionalMaxHealth(Math.Max(0f, currentValue - health.MaxValue.Total));
            };
            barrier.MaxValue.AddFollowingMaxHealth(shield.MaxValue);


            AddHealthType(health, armor, shield, barrier);

        }

        public void AddHealthType(params HealthType[] healthTypes)
        {
            foreach (HealthType healthType in healthTypes)
                healthType.MaxValue.ChangedTotal += (float newValue, float oldValue) => AfterMaxHealthChange?.Invoke(Owner, healthType, newValue, oldValue);
        }

        public static float GetTotalCurrentHealth(List<(HealthCatagory, float)> currentHealth)
        {
            return currentHealth.Sum(curCat => curCat.Item2);
        }
        public HealthBar(BaseCharacter owner, List<HealthType> healthTypes, EffectivenessStat damageResistance, EffectivenessStat movementSlow, EffectivenessStat healingEffectiveness)
        {
            Owner = owner;
            HealthTypes = healthTypes;
            DamageResistance = damageResistance;
            MovementSlow = movementSlow;
            HealingEffectiveness = healingEffectiveness;
        }

        public List<(HealthCatagory, float)> CalculateCurrentHealth()
        {
            List<(HealthCatagory, float)> temp = new List<(HealthCatagory, float)>();
            HealthTypes.ForEach(health => temp.Add((health.Catagory, health.CurrentValue)));
            return temp;

        }
        public float UpdateCurrentHealth()
        {
            List<(HealthCatagory, float)> oldHealth = CurrentHealth;
            CurrentHealth = CalculateCurrentHealth();
            return GetTotalCurrentHealth(CurrentHealth) - GetTotalCurrentHealth(oldHealth);
        }

        public static event Action<BaseCharacter, DamageInfo> BeforeTakingDamage;
        public static event Action<BaseCharacter, DamageInfo, List<(HealthCatagory, float)>, float> AfterTakingDamage;

        //Returns the amount of ActualDamage taken.
        public float TakeDamage(DamageInfo damageInfo)
        {
            BeforeTakingDamage?.Invoke(Owner, damageInfo);

            float damageTaken = damageInfo.Damage;
            float totalDamageTaken = 0f;

            List<(HealthCatagory, float)> damageTakenByType = new List<(HealthCatagory, float)>();
            HealthTypes.AsEnumerable().Reverse().Aggregate(damageTaken, (leftoverDamge, currentType) =>
            { float damage = currentType.TakeDamage(damageTaken); totalDamageTaken += damage; damageTakenByType.Add((currentType.Catagory, damage)); return damage; });

            AfterTakingDamage?.Invoke(Owner, damageInfo, damageTakenByType, totalDamageTaken);
            return UpdateCurrentHealth();
        }


        public float Heal(float healingAmount, EffectivenessStat additionalHealingEfficiency = null, HealthCatagory catagory = HealthCatagory.Health)
        {

        }

        public static event Action<BaseCharacter, HealthType, float, float> AfterMaxHealthChange;

        public float GetMaxHealth()
        {

        }

        public float GetMaxShield()
        {

        }
    }

    public class EventChain
    {
        List<EventInfo> EventInfos=new List<EventInfo>();

        public bool TryAddEventInfo(EventInfo eventInfo)
        {
            if (!EventInfos.Any(info => info.IsSimilarEvent(eventInfo)))
            {
                EventInfos.Add(eventInfo);
                return true;
            }
            return false;
        }
    }
    public abstract class EventInfo
    {
        public BaseCharacter Source;
        public BaseCharacter Destination;
        public int SourceEffect;

        public virtual bool IsSimilarEvent(EventInfo other)
        {
            return (Source == other.Source && Destination == other.Destination && SourceEffect == other.SourceEffect) ;
        }
    }
    public class DamageInfo: EventInfo
    {
        public float Damage;
        bool IsCrit;
        Vector2 DirectionFrom;
    }

    public class HealingInfo:EventInfo
    {
        public float Amount;
    }

    

    public enum StatModTarget
    {
        All,Primary,Secondary,Utility,Special,Ult, Equipment
    }

    

	
}
