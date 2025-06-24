using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static BrannPack.ModifiableStats.CharacterStats;
using static BrannPack.ModifiableStats.AbilityStats;
using BrannPack.ModifiableStats;
using BrannPack.UI;
using BrannPack.CooldownHandling;
using BrannPack.InputHelpers;
using System.Security.Cryptography.X509Certificates;
using BrannPack.Helpers.RecourcePool;
using BrannPack.Helpers.Initializers;
using BrannPack.AbilityHandling;




namespace BrannPack.Character
{
	[GlobalClass]
	public partial class BaseCharacterBody : CharacterBody2D, IPoolable
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
		public override void _Draw()
		{
			DrawRect(new Rect2(-16, -16, 32, 32), new Color(1, 0, 0, 0.5f));
		}

		[Export] public string CharacterName;
		public CharacterMaster CharacterMaster;

		[Export] public float Acceleration = 100f;  // How fast the character accelerates
		[Export] public float Deceleration = 80f;  // How fast the character decelerates when no input is given
		public MoveSpeedStat MoveSpeed;


		//Players are 1
		//Bosses are around a 5 or 10
		//Swarmers are like a .25


		[Export] public string StartingPrimary;
		[Export] public string StartingSecondary;
		[Export] public string StartingUtility;
		[Export] public string StartingSpecial;
		[Export] public string StartingUlt;
		[Export] public string StartingEquipment;

		[Export] private float AbilityScale;
		[Export] private float HealthScale;
		[Export] private float MoveSpeedScale;
		[Export] private float SizeScale;
		[Export] private float MeleeDistance;

		[Export] public float StartingMoveSpeed;
		[Export] public float MoveSpeedMax;

		[Export] public float StartingMaxHealth;
		[Export] public float StartingHealthRegen;
		[Export] public float HealthRegenScaling;




		public HealthBar HealthBar;

		[Export] public AnimatedSprite2D AnimSprite;

		private CooldownHandler _cooldownHandler;

		public CooldownHandler CooldownHandler 
		{ 
			get
			{
				if (_cooldownHandler == null)
				{
					_cooldownHandler = new CooldownHandler();
					this.AddChild(_cooldownHandler);
				}
				return _cooldownHandler;
			}

			set
			{
				_cooldownHandler = value;
			}
		}

		private FloatingHealthBar _floatingHealthbar;

		public FloatingHealthBar FloatingHealthBar
		{
			get
			{
				if (_floatingHealthbar == null)
				{
					_floatingHealthbar = new FloatingHealthBar();
					
					this.AddChild(_floatingHealthbar);
					_floatingHealthbar.Owner = this;
				}
				return _floatingHealthbar;
			}
			set
			{
				_floatingHealthbar = value;
			}
		}

		public bool IsInUse { get; set; } = false;

		public virtual void OnActivate()
		{
			IsInUse = true;
		}

		public virtual void OnDeactivate()
		{
			IsInUse = false;
		}

		public StatsHolder StartingStats;


		public EntityController Controller;
		public List<Vector2> ExternalVelocityInput=new();
		public Vector2 AimDirection;
		public Vector2 MoveDirection;
		private Vector2 _InputVelocity = Vector2.Zero;



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

		public void AddCooldown((int indexType,int souceIndex,int cooldownSource) key,Cooldown cooldown)
		{
			CooldownHandler.AddCooldown(key,cooldown);
		}
		public override void _PhysicsProcess(double delta)
		{
			base._PhysicsProcess(delta);
			float CalculatedSpeed=MoveSpeed!=null? (MoveSpeed.CalculateTotal() * 100f ): 0f;
			GD.Print("MS:" + CalculatedSpeed);
			// Get input vector
			Vector2 inputDirection = MoveDirection;
			

			// Normalize the direction to ensure consistent speed in all directions
			if (inputDirection.Length() > 0)
				inputDirection = inputDirection.Normalized();
			if (inputDirection != Vector2.Zero)
			{
				// Calculate target velocity based on input
				Vector2 targetVelocity = inputDirection * CalculatedSpeed;

				var oldInputVel = _InputVelocity;
				float t = 1f - Mathf.Exp(-Acceleration * (float)delta); // exponential smoothing
				_InputVelocity = _InputVelocity.Lerp(targetVelocity, t);
				var inputDiff = _InputVelocity - oldInputVel;

				Velocity += inputDiff;

			}
			// Gradual deceleration when no input is given
			else
			{

				t = 1f - Mathf.Exp(-Deceleration * (float)delta);
				Velocity = Velocity.Lerp(Vector2.Zero, t);
			}

            // Accumulate external forces
            Vector2 totalExternalVelocity = Vector2.Zero;
            foreach (var force in ExternalVelocityInput)
            {
                totalExternalVelocity += force;
            }

            // Apply blended external velocity
            Velocity += totalExternalVelocity * 1f;

            // Clear the external forces for the next frame
            ExternalVelocityInput.Clear();

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
			{
				CharacterMaster.Controller.UpdateInput();
				HealthBar.UpdateUIHealthInfo();
			   FloatingHealthBar.UpdateHealthBar();
			}
		}

		public void InitializeBaseCharacterBody()
		{

		}

		public void SetPool(PoolManager pool, string poolKey)
		{
			throw new NotImplementedException();
		}
	}
	public enum CharacterTeam { Player, Enemy }
    [GlobalClass]
    public partial class BodyPrefabInfo : Resource, IIndexable
    {
        protected static int NextIndex = 0;
        public static Registry<BodyPrefabInfo> BodyPrefabRegistry = new Registry<BodyPrefabInfo>();

        public int Index { get; protected set; } = -1;

        public void SetIndex() { if (Index != -1) Index = NextIndex++; }
		[Export] public string Name;
		[Export] public string CodeName;
		[Export] public PackedScene BodyScene;
        [Export] public string Description { get; set; } = "";
        [Export] public Texture2D Icon { get; set; }

        string IIndexable.CodeName => CodeName;
    }

    public partial class HealthBar : GodotObject
	{

		public CharacterMaster Master;

		public Dictionary<HealthType, HealthBehavior> HealthList = new Dictionary<HealthType, HealthBehavior> { };
		public List<(HealthType type, float startPosition, float width, bool isOverHealth)> UIInfo=new();

		public float HealthNumerator;
		public float HealthDenominator;
		public float CurrentValueVisible;
		public float CurrentMaxVisible;

		[Export] public EffectivenessStat DamageResistance;
		[Export] public EffectivenessStat HealingEffectiveness;


		public HealthBar(CharacterMaster master, MaxHealthStat healthMax, MaxHealthStat armorMax, MaxHealthStat shieldMax, MaxHealthStat barrierMax)
		{
			Master = master;
			Health health = new Health(healthMax.CalculateTotal(), healthMax);
			AddHealthType(health, HealthType.Health);

			Armor armor = new Armor(0f, armorMax);
			armor.MaxValue.AddFollowingMaxHealth(health.MaxValue);
			AddHealthType(armor, HealthType.Armor);
			GD.Print(armor.GetCurrentValue());

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

				//if(currentValue+overValue<=0)
				//    continue;

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

						//regular armor
						temp.Add((type, currentHbarPosition - behavior.CurrentValue, behavior.CurrentValue, false));
						//overarmor
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
				return (0f, 0f);



			(float, float) result = (0f, 0f);
			if (changeInfo.Change < 0)
				result = HealthList[changeInfo.HealthType].TakeDamage(changeInfo);
			else
				result = HealthList[changeInfo.HealthType].AddCurrentValue(changeInfo);

			

			AfterHealthChange?.Invoke(this, changeInfo);

			return result;

		}

		public static event Action<HealthBar, HealingInfo> BeforeHealing;
		public static event Action<HealthBar, HealingInfo, (HealthType, float)[], float, float> AfterHealing;
		public float Heal(HealingInfo healingInfo, EventChain eventChain)
		{
			BeforeHealing?.Invoke(this, healingInfo);

			float healingAmount = healingInfo.HealingAmount;
			float leftoverHealing = healingAmount;
			float totalHealingDone = 0f;
			HealthChangeInfo changeInfo = new HealthChangeInfo(healingInfo.Source, healingInfo.Destination, healingInfo.Key,  healingInfo.HealingAmount, HealthType.Health, null);

			List<(HealthType, float)> healingAmountByType = new List<(HealthType, float)>();
			foreach (var type in HealthTypeOrder)
			{
				if (HealthTypeInfo[type].Category != healingInfo.Catagory)
					continue;
				changeInfo.Change = leftoverHealing;
				changeInfo.HealthType = type;
				var result = ChangeHealth(changeInfo);
				
				totalHealingDone += result.change;
				if (result.change != 0f)
					healingAmountByType.Add((type, result.change));

				leftoverHealing = result.leftOverChange;
				if (leftoverHealing <= 0f)
					break;
			}
			AfterHealing?.Invoke(this, healingInfo, healingAmountByType.ToArray(), totalHealingDone, leftoverHealing);
			UpdateUIHealthInfo();
			return totalHealingDone;
		}

		public static event Action<HealthBar, HealthBehavior, float, float> AfterMaxHealthChange;

		public HealthBehavior GetHealthBehavior(HealthType healthType)
		{
			return HealthList[healthType];
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
			bool isCrit, StatsHolder stats=null,Vector2 origin=default, Vector2 directionTo= default)
			: base(source, destination, key) =>
			(IsCrit, Stats,Origin, DirectionTo) = (isCrit, stats,origin,directionTo);
	}

	public class HealingInfo : EventInfo
	{
		public float HealingAmount;
		public EffectivenessStat AdditionalHealingEfficiency = null;
		public HealthCategories Catagory = HealthCategories.Health;
		public HealthType HealthType = default;

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

	public class AbilityUseInfo: EventInfo
	{
		public InputPressState PressState;
        public Vector2? Origin;
        public Vector2? DirectionTo;
        public AbilityUseInfo(CharacterMaster source, CharacterMaster destination, (int, int, int) key,
			InputPressState pressState, Vector2? origin = null, Vector2? directionTo = null)
			: base(source, destination, key) => (PressState,Origin,DirectionTo) = (pressState,origin,directionTo);
	}

	

	public enum StatModTarget
	{
		All,Primary,Secondary,Utility,Special,Ult, Equipment
	}

	

	
}
