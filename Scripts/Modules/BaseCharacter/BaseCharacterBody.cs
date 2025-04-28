using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static BrannPack.ModifiableStats.CharacterStats;
using static BrannPack.ModifiableStats.AbilityStats;
using BrannPack.ModifiableStats;



namespace BrannPack.Character
{
    [GlobalClass]
    public partial class BaseCharacterBody : CharacterBody2D
    {
        public static List<BaseCharacterBody> AllCharacters = new List<BaseCharacterBody>();

        public override void _Ready()
        {
            base._Ready();
            AllCharacters.Add(this);

            // Hitbox outline
            ColorRect debugBox = new ColorRect();
            debugBox.Color = new Color(1, 0, 0, 0.5f); // Red with transparency
            debugBox.Size = new Vector2(32, 32);
            AddChild(debugBox);
        }

        public override void _ExitTree()
        {
            AllCharacters.Remove(this);
        }
        public override void _Draw()
        {
            DrawRect(new Rect2(-16, -16, 32, 32), new Color(1, 0, 0, 0.5f));
        }

        [Export] public string CharacterName;
        public CharacterMaster CharacterMaster;

        [Export] public float Acceleration = 1000f;  // How fast the character accelerates
        [Export] public float Deceleration = 800f;  // How fast the character decelerates when no input is given
        public MoveSpeedStat MoveSpeed;


        //Players are 1
        //Bosses are around a 5 or 10
        //Swarmers are like a .25


        [Export] public string StartingPrimary;
        [Export] public string StartingSecondary;
        [Export] public string StartingUtility;
        [Export] public string StartingSpecial;
        [Export] public string StartingUlt;

        [Export] private float AbilityScale;
        [Export] private float HealthScale;
        [Export] private float MoveSpeedScale;
        [Export] private float SizeScale;

        [Export] public float StartingMoveSpeed;
        [Export] public float MoveSpeedMax;

        [Export] public float StartingMaxHealth;
        [Export] public float StartingHealthRegen;
        [Export] public float HealthRegenScaling;




        public HealthBar HealthBar;

        [Export] public AnimatedSprite2D AnimSprite;

        public StatsHolder StartingStats;


        public EntityController Controller;
        public Vector2 AttackDirection;
        public Vector2 MoveDirection;



        //public static event Action<BaseCharacterBody, Stat, ModifiableStat> RefreshAbilityStatVariable;
        public static event Action<BaseCharacterBody, float> BeforeMovementRestricted;
        public static event Action<BaseCharacterBody, float> AfterMovementRestricted;

        public void Init()
        {

        }

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

            float CalculatedSpeed=MoveSpeed.CalculateTotal();
            
            // Get input vector
            Vector2 inputDirection = MoveDirection;
            GD.Print(MoveDirection+" "+CalculatedSpeed);
            

            // Normalize the direction to ensure consistent speed in all directions
            if (inputDirection.Length() > 0)
                inputDirection = inputDirection.Normalized();

            // Calculate target velocity based on input
            Vector2 targetVelocity = inputDirection * CalculatedSpeed;
            GD.Print(targetVelocity);

            // Gradual acceleration: Lerp towards target velocity
            Velocity = Velocity.Lerp( targetVelocity, Acceleration * (float)delta);

            // Gradual deceleration when no input is given
            if (inputDirection == Vector2.Zero)
            {
                Velocity = Velocity.Lerp(Vector2.Zero, Deceleration * (float)delta);
            }

            // Apply movement
            MoveAndSlide();

            // Animate!
            HandleAnimation(inputDirection);
        }

        private void HandleAnimation(Vector2 direction)
        {
            if (AnimSprite == null) return;

            if (direction == Vector2.Zero)
            {
                AnimSprite.Play("idle");
            }
            else
            {
                AnimSprite.Play("walk");

                // Optional: flip or change direction
                if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y))
                {
                    AnimSprite.FlipH = direction.X < 0;
                }
            }
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            if (CharacterMaster?.Controller != null)
                CharacterMaster.Controller.UpdateInput();
        }

        public void InitializeBaseCharacterBody()
        {

        }


        

         


    }
    public enum CharacterTeam { Player, Enemy }

    public partial class HealthBar : GodotObject
    {

        public CharacterMaster Master;

        public Dictionary<HealthType, HealthBehavior> HealthList = new Dictionary<HealthType, HealthBehavior> { };
        public List<(HealthType type, float startPosition, float width, bool isOverHealth)> UIInfo;

        public float HealthNumerator;
        public float HealthDenominator;
        public float CurrentValueVisible;
        public float CurrentMaxVisible;

        [Export] public EffectivenessStat DamageResistance;
        [Export] public EffectivenessStat HealingEffectiveness;


        public HealthBar(CharacterMaster master, MaxHealthStat healthMax, MaxHealthStat armorMax, MaxHealthStat shieldMax, MaxHealthStat barrierMax)
        {
            Master = master;
            Health health = new Health(0f, healthMax);
            AddHealthType(health, HealthType.Health);

            Armor armor = new Armor(0f, armorMax);
            armor.MaxValue.AddFollowingMaxHealth(health.MaxValue);
            AddHealthType(armor, HealthType.Armor);

            Shield shield = new Shield(0f, shieldMax);
            AddHealthType(shield, HealthType.Shield);

            BarrierHealth barrier = new BarrierHealth(0f, barrierMax);
            barrier.MaxValue.AddFollowingMaxHealth(health.MaxValue);
            armor.AfterCurrentValueChange += (float currentValue, float valueadded, float overvalue) =>
            {
                if (currentValue > health.MaxValue.Total)
                    barrier.MaxValue.ChangeAdditionalMaxHealth(Math.Max(0f, currentValue - health.MaxValue.Total));
            };
            barrier.MaxValue.AddFollowingMaxHealth(shield.MaxValue);
            AddHealthType(barrier, HealthType.Barrier);

        }

        protected void AddHealthType(HealthBehavior behavior, HealthType healthType)
        {
            behavior.MaxValue.ChangedTotal += (float newValue, float oldValue) => AfterMaxHealthChange?.Invoke(this, behavior, newValue, oldValue);
            HealthList[healthType] = behavior;
        }


        public Action UIHealthUpdated;
        public void UpdateUIHealthInfo()
        {
            HealthType[] order = HealthTypeOrder;
            List<(HealthType type, float startPosition, float width, bool isOverHealth)> temp = new();


            HealthNumerator = 0;
            HealthDenominator = 0;
            CurrentValueVisible = 0;
            CurrentMaxVisible = 0;

            float currentHbarPosition = 0f;
            foreach (var type in order)
            {
                var behavior = HealthList[type];
                float thisPosition = currentHbarPosition;

                float overValue = behavior.GetOverValue();
                float currentValue = behavior.GetCurrentValue();

                HealthNumerator += behavior.CurrentValue;

                switch (type)
                {
                    case HealthType.Health:
                        HealthDenominator += behavior.MaxValue.Total;
                        CurrentValueVisible += currentValue;
                        CurrentMaxVisible += behavior.MaxValue.Total;
                        temp.Add((type, currentHbarPosition, currentValue, false));

                        currentHbarPosition += currentValue;
                        break;
                    case HealthType.Armor:

                        CurrentValueVisible += overValue;
                        CurrentMaxVisible += overValue;
                        temp.Add((type, currentHbarPosition - behavior.CurrentValue, currentHbarPosition, false));
                        temp.Add((type, currentHbarPosition, overValue, true));
                        currentHbarPosition += overValue;
                        break;
                    case HealthType.Shield:
                        HealthDenominator += behavior.MaxValue.Total;
                        CurrentValueVisible += behavior.GetCurrentValue();
                        CurrentMaxVisible += behavior.MaxValue.Total;
                        temp.Add((type, currentHbarPosition, behavior.GetCurrentValue(), false));

                        currentHbarPosition += currentValue;
                        break;
                    case HealthType.Barrier:

                        CurrentValueVisible += overValue;
                        CurrentMaxVisible += overValue;
                        temp.Add((type, 0, currentValue, false));
                        temp.Add((type, currentHbarPosition, overValue, true));
                        currentHbarPosition += overValue;
                        break;

                }

            }
            UIInfo = temp;
            UIHealthUpdated?.Invoke();
        }

        public static event Action<HealthBar, DamageInfo> BeforeLoseHealth;
        public static event Action<HealthBar, DamageInfo, (HealthType, float)[], float, float> AfterLoseHealth;



        //Returns the amount of ActualDamage taken.
        public float TakeDamage(DamageInfo damageInfo)
        {
            BeforeLoseHealth?.Invoke(this, damageInfo);

            float damageTaken = damageInfo.Damage;
            float leftoverDamage = damageTaken;
            float totalDamageTaken = 0f;
            HealthChangeInfo changeInfo = new HealthChangeInfo(damageInfo.Source, damageInfo.Destination, damageInfo.Key, -damageInfo.Damage, HealthType.Health, null);

            List<(HealthType, float)> damageTakenByType = new List<(HealthType, float)>();
            foreach (var type in HealthTypeOrder.Reverse())
            {
                changeInfo.Change = -leftoverDamage;
                changeInfo.HealthType = type;
                var result = ChangeHealth(changeInfo);
                totalDamageTaken += result.change;
                if (result.change == 0f)
                    damageTakenByType.Add((type, result.change));

                leftoverDamage = result.leftOverChange;
                if (leftoverDamage <= 0f)
                    break;
            }

            AfterLoseHealth?.Invoke(this, damageInfo, damageTakenByType.ToArray(), totalDamageTaken, leftoverDamage);
            UpdateUIHealthInfo();
            return totalDamageTaken;
        }
        public static event Action<HealthBar, HealthChangeInfo> BeforeHealthChange;
        public static event Action<HealthBar, HealthChangeInfo> AfterHealthChange;
        public (float change, float leftOverChange) ChangeHealth(HealthChangeInfo changeInfo)
        {
            BeforeHealthChange?.Invoke(this, changeInfo);
            if (changeInfo.Change == 0)
                return (0f, changeInfo.Change);



            (float, float) result = (0f, 0f);
            if (changeInfo.Change > 0)
                result = HealthList[changeInfo.HealthType].TakeDamage(changeInfo);
            else
                result = HealthList[changeInfo.HealthType].AddCurrentValue(changeInfo);

            AfterHealthChange?.Invoke(this, changeInfo);

            return result;

        }

        public float Heal(HealingInfo healingInfo, EventChain eventChain)
        {
            return 0;
        }

        public static event Action<HealthBar, HealthBehavior, float, float> AfterMaxHealthChange;

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
            return (Source == other.Source && Destination == other.Destination && Key==other.Key) ;
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
