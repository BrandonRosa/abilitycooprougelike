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

        public void InitializeBaseCharacterBody()
        {

        }


        

         


    }
    public enum CharacterTeam { Player, Enemy }

    


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
        public bool IsCrit;
        public Vector2 DirectionFrom;
        public StatsHolder Stats;
        public DamageInfo(CharacterMaster source, CharacterMaster destination, (int sourceType, int sourceIndex, int sourceEffect) key,
            float damage, bool isCrit, Vector2 directionFrom=default,StatsHolder stats=null)
            : base(source, destination, key) =>
            (Damage, IsCrit, DirectionFrom,Stats) = (damage, isCrit, directionFrom,stats);

    }

    //Use this when LAUNCHING an attack. For example, launching a rocket
    public class AttackInfo: EventInfo
    {
        public bool IsCrit;
        public Vector2 Origin;
        public Vector2 DirectionTo;

        public StatsHolder Stats;
        public AttackInfo(CharacterMaster source, CharacterMaster destination, (int sourceType, int sourceIndex, int sourceEffect) key,
            bool isCrit, StatsHolder stats=null,Vector2 directionFrom=default, Vector2 directionTo= default)
            : base(source, destination, key) =>
            (IsCrit, Stats,Origin, DirectionTo) = (isCrit, stats,directionFrom,directionTo);
    }

    public class HealingInfo : EventInfo
    {
        float HealingAmount;
        EffectivenessStat AdditionalHealingEfficiency = null;
        HealthCategories Catagory = HealthCategories.Health;

        public HealingInfo(CharacterMaster source, CharacterMaster destination, (int, int, int) key,
            float healingAmount, EffectivenessStat additionalHealingEffeciency = null, HealthCategories catagory = HealthCategories.Health)
            : base(source, destination, key) => (HealingAmount, AdditionalHealingEfficiency, Catagory) = (healingAmount, additionalHealingEffeciency, catagory);
    }

    public class HealthChangeInfo: EventInfo
    {
        public float Change;
        public EffectivenessStat AdditionalChangeEffectiveness;
        public HealthType HealthType;

        public HealthChangeInfo(CharacterMaster source, CharacterMaster destination, (int, int, int) key,
            float change, HealthType healthType, EffectivenessStat additionalChangeEffectiveness)
            : base(source, destination, key) => (Change, HealthType, AdditionalChangeEffectiveness) = (change, healthType, additionalChangeEffectiveness);
    }

    

    public enum StatModTarget
    {
        All,Primary,Secondary,Utility,Special,Ult, Equipment
    }

    

	
}
