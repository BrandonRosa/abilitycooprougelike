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
using BrannPack.CooldownHandling;
using AbilityCoopRougelike.Items;


namespace BrannPack.Character
{

    public partial class BaseCharacterBody : CharacterBody2D
    {
        public static List<BaseCharacterBody> AllCharacters = new List<BaseCharacterBody>();

        public override void _Ready()
        {
            base._Ready();
            AllCharacters.Add(this);
        }

        public override void _ExitTree()
        {
            AllCharacters.Remove(this);
        }

        [Export] public string CharacterName;
        [Export] public CharacterMaster CharacterMaster;

        [Export] public float Acceleration = 1000f;  // How fast the character accelerates
        [Export] public float Deceleration = 800f;  // How fast the character decelerates when no input is given
        [Export] public MoveSpeedStat MoveSpeed;
       

        //Players are 1
        //Bosses are around a 5 or 10
        //Swarmers are like a .25

        [Export] private float AbilityScale;
        [Export] private float HealthScale;
        [Export] private float MoveSpeedScale;
        [Export] private float SizeScale;
        [Export] private bool IsPlayerControlled;
        [Export] public CharacterTeam Team;

        


        public HealthBar StartingHealthBar;

        [Export] public AnimatedSprite2D AnimSprite;

        public StatsHolder StartingStats;

        

        public Vector2 AttackDirection;
        public Vector2 MoveDirection;



        //public static event Action<BaseCharacterBody, Stat, ModifiableStat> RefreshAbilityStatVariable;
        public static event Action<BaseCharacterBody, float> BeforeMovementRestricted;
        public static event Action<BaseCharacterBody, float> AfterMovementRestricted;

        public bool IsMovementRestricted()
        {
            return false;
        }

        public void Move(Vector2 direction)
        {

        }
        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);

            float CalculatedSpeed=MoveSpeed.Total;
            
            // Get input vector
            Vector2 inputDirection = MoveDirection;
            

            // Normalize the direction to ensure consistent speed in all directions
            if (inputDirection.Length() > 0)
                inputDirection = inputDirection.Normalized();

            // Calculate target velocity based on input
            Vector2 targetVelocity = inputDirection * CalculatedSpeed;

            // Gradual acceleration: Lerp towards target velocity
            Velocity = Velocity.Lerp( targetVelocity, Acceleration * (float)delta);

            // Gradual deceleration when no input is given
            if (inputDirection == Vector2.Zero)
            {
                Velocity = Velocity.Lerp(Vector2.Zero, Deceleration * (float)delta);
            }

            // Apply movement
            MoveAndSlide();
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
        }


        public enum Stat
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
            Lifesteal,
            MoveSpeed
        }

         


    }
    public enum CharacterTeam { Player, Enemy }

    public partial class HealthBar: GodotObject
    {

        public BaseCharacterBody Owner;
        [Export] public List<HealthType> HealthTypes = new List<HealthType>() { };
        protected List<(HealthCatagory, float)> CurrentHealth;
        [Export] public EffectivenessStat DamageResistance;
        [Export] public EffectivenessStat HealingEffectiveness;

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
        public HealthBar(BaseCharacterBody owner, List<HealthType> healthTypes, EffectivenessStat damageResistance, EffectivenessStat movementSlow, EffectivenessStat healingEffectiveness)
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

        public static event Action<BaseCharacterBody, DamageInfo> BeforeTakingDamage;
        public static event Action<BaseCharacterBody, DamageInfo, List<(HealthCatagory, float)>, float> AfterTakingDamage;

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


        public float Heal(HealingInfo healingInfo,EventChain eventChain)
        {

        }

        public static event Action<BaseCharacterBody, HealthType, float, float> AfterMaxHealthChange;

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
        public CharacterMaster Source;
        public CharacterMaster Destination;
        public (int SourceType,int SourceIndex,int SourceEffect) Key;

        public EventInfo(CharacterMaster source, CharacterMaster destination, (int,int,int) key) => (Source, Destination, Key) = (source, destination, key);

        public virtual bool IsSimilarEvent(EventInfo other)
        {
            return (Source == other.Source && Destination == other.Destination && SourceEffect == other.SourceEffect && SourceIndex==other.SourceIndex) ;
        }
    }
    //Use this when dealing damage from an attack. For example, the explosion from a rocket
    public class DamageInfo: EventInfo
    {
        public float Damage;
        bool IsCrit;
        Vector2 DirectionFrom;

        public DamageInfo(CharacterMaster source, CharacterMaster destination, (int sourceType, int sourceIndex, int sourceEffect) key,
            float damage, bool isCrit, Vector2 directionFrom)
            : base(source, destination, key) =>
            (Damage, IsCrit, DirectionFrom) = (damage, isCrit, directionFrom);

    }

    //Use this when LAUNCHING an attack. For example, launching a rocket
    public class AttackInfo: EventInfo
    {
        public float Damage;
        public bool IsCrit;
        public Vector2 DirectionFrom;
        public Vector2 DirectionTo;

        public StatsHolder Stats;
        public AttackInfo(CharacterMaster source, CharacterMaster destination, (int sourceType, int sourceIndex, int sourceEffect) key,
            float damage, bool isCrit, StatsHolder stats=null,Vector2 directionFrom=default, Vector2 directionTo= default)
            : base(source, destination, key) =>
            (Damage, IsCrit, Stats,DirectionFrom, DirectionTo) = (damage, isCrit, stats,directionFrom,directionTo);
    }

    public class HealingInfo : EventInfo
    {
        float HealingAmount;
        EffectivenessStat AdditionalHealingEfficiency = null;
        HealthCatagory Catagory = HealthCatagory.Health;

        public HealingInfo(CharacterMaster source, CharacterMaster destination, (int, int, int) key,
            float healingAmount, EffectivenessStat additionalHealingEffeciency = null, HealthCatagory catagory = HealthCatagory.Health)
            : base(source, destination, key) => (HealingAmount,AdditionalHealingEfficiency,Catagory)=(healingAmount,additionalHealingEffeciency,catagory)
    }

    

    public enum StatModTarget
    {
        All,Primary,Secondary,Utility,Special,Ult, Equipment
    }

    

	
}
