using BrannPack.Character;
using BrannPack.CooldownHandling;
using BrannPack.Helpers.Initializers;
using BrannPack.InputHelpers;
using BrannPack.ItemHandling;
using BrannPack.ModifiableStats;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static BrannPack.ModifiableStats.AbilityStats;

namespace BrannPack.AbilityHandling
{
    public struct AIAbilityHint
    {
        public (float Min, float Max)? UseRangeOverrideBounds;   // If set, overrides the activation range
        public (float Min, float Max)? RangeUseMultiplierBounds; // Multiply actual ability range for looser AI use
        public (float Min, float Max)? HealthPercentBounds;      // Only use when AI HP is above this

        public bool RequiresLOS;             // If true, Only uses the ability when target is in LOS
        public bool IsPanicButton;           // Use when AI is panicking or fleeing
		public bool ContinueWindupIfTargetLost;
    }

    public class AbilitySlot
	{
		public CharacterMaster Owner;

		public AbilitySlotType SlotType;
		public Ability AbilityInstance;
		public HashSet<AbilityUpgrade> CurrentUpgrades;

		public AbilityStats.StatsHolder<AbilitySlot> ThisAbilityStats;

		public ChargedCooldown CCooldown;
		public Windup Windup;
		public float CurrentCharges=> CCooldown.CurrentCharges;

		public Vector2? WindupAttackDirection;
		public Vector2? WindupAttackLocation;
		public Windup StoredWindup;

		public bool IsUsable=>CurrentCharges>0;

		public static event Action<AbilitySlot> BeforeAbilitySlotUse;
		public static event Action<AbilitySlot> AfterAbilitySlotUse;

		public AbilitySlot(CharacterMaster owner,string abilityCodeName, AbilitySlotType slotType)
		{
			Owner = owner;
			AbilityInstance = Ability.AbilityRegistry.Get(abilityCodeName);
			SlotType = slotType;
		}

		public void SetAbilityStats()
		{
			var abilityDefaultStats = AbilityInstance.Stats.CopyAndGetStatsByCriterea(CurrentUpgrades);
			ThisAbilityStats = abilityDefaultStats.ToGlobalStatsHolder<AbilitySlot>(this);

			var cooldown = ThisAbilityStats.GetStatByVariable<CooldownStat>(Stat.Cooldown)??StatsHolder.ZeroStatHoler.GetStatByVariable<CooldownStat>(Stat.Cooldown);
			var charges = ThisAbilityStats.GetStatByVariable<ChargeStat>(Stat.Charges) ?? StatsHolder.ZeroStatHoler.GetStatByVariable<ChargeStat>(Stat.Charges);
			CCooldown = new ChargedCooldown(cooldown, charges);
			Owner.Body.CooldownHandler.AddCooldown((-1, (int)SlotType,1 ), CCooldown);

			var fireRate = ThisAbilityStats.GetStatByVariable<FireRateStat>(Stat.FireRate);
			if (fireRate != null)
			{
				Windup = new Windup(1f / fireRate.CalculateTotal(), Mathf.Clamp(1f / fireRate.CalculateTotal() * .1f, .01f, .65f), false,true);
				Owner.Body.CooldownHandler.AddCooldown((-1, (int)SlotType, 2), Windup);
				fireRate.ChangedTotal += UpdateWindupDuration;
			}
			else
			{
				Owner.Body.CooldownHandler.RemoveCooldown((-1, (int)SlotType, 2), false);
			}
		}


		private void UpdateWindupDuration(float oldvalue,float newvalue)
		{
			Windup.Duration = newvalue;
		}

        public void SetAbilityUpgrade(AbilityUpgrade abilityUpgrade,bool enabled)
		{
			if (enabled)
			{
				CurrentUpgrades.Add(abilityUpgrade);
				var stats = AbilityInstance.Stats.GetCritereaSpecificStats(abilityUpgrade);
				ThisAbilityStats.SetStatBaseValues(stats);
			}
			else
			{
				if (!CurrentUpgrades.Remove(abilityUpgrade))
					return;
				var allStats = AbilityInstance.Stats.CopyAndGetStatsByCriterea(CurrentUpgrades);
				//ThisAbilityStats.SetAllStats(allStats.)
			}
			

			//Update ThisAbilityStats BaseStats with stats
			
		}
		public bool TryUseAbility(InputPressState pressState)
		{
			if(IsUsable)
			{
				var info = new AbilityUseInfo(Owner, Owner, (-1, -1, -1), pressState);
				BeforeAbilitySlotUse?.Invoke(this);
				AbilityInstance.UseAbility(Owner, this, info,null);
				AfterAbilitySlotUse?.Invoke(this);
				return true;
			}
			return false;
		}

		public (float secondsOnCooldown, float cooldownPercent, float charges, float maxCharges, int abilityIndex) GetSimpleCooldownInfo()
		{
			return (0, 0, 0, 0, 0);
		}

		public void Update(float deltaTime)
		{

		}
	}

	public abstract class Ability<T> : Ability where T : Ability<T>
	{
		public static T instance { get; private set; }


		public Ability()
		{
			if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
			instance = this as T;
			instance.SetIndex();
			Ability.AbilityRegistry.Register(instance);
			
		}
	}
	public abstract class Ability : IIndexable
	{
		protected static int NextIndex = 0;
		public static Registry<Ability> AbilityRegistry = new Registry<Ability>();
		public int Index { get; protected set; } = -1;

		public void SetIndex() { if (Index != -1) Index = NextIndex++; }

		public abstract StatsByCritera<AbilityUpgrade> Stats { get; protected set; }
		public abstract string Name { get; protected set; }
		public abstract string CodeName { get; protected set; }
		public abstract string Description { get; protected set; }

		public abstract string AdvancedDescription { get; protected set; }

		public virtual Texture2D Icon { get; protected set; } = GD.Load<Texture2D>("res://Assets/PlaceholderAssets/AbilityIcons/none.png");

		public virtual AIAbilityHint AbilityHint { get; set; } = new AIAbilityHint { };

		private bool CanCharge;
		private bool IsInfiniteUse;
		private bool IsMultiPrompt;

		private AbilityUpgradeTree UpgradeTree;

		//private image ArtWork;

		private static List<AbilityUpgrade> AbilityUpgrades;

		public abstract void UseAbility(CharacterMaster master,AbilitySlot abilitySlot, AbilityUseInfo abilityUseInfo, EventChain eventChain);
		public abstract BaseCharacterBody UpdateTarget();

	}

	public class EmptyAbility : Ability<EmptyAbility>
	{
		public override StatsByCritera<AbilityUpgrade> Stats { get; protected set; } = new StatsByCritera<AbilityUpgrade>(new Dictionary<Stat, ModifiableStat>() { { Stat.Cooldown,new CooldownStat(.1f)} },new Dictionary<AbilityUpgrade, Dictionary<Stat, ModifiableStat>>());
		public override string Name { get; protected set; } = "None";
		public override string CodeName { get; protected set; } = "NONE";
		public override string Description { get; protected set; } = "";
		public override string AdvancedDescription { get; protected set; } = "";

		public override BaseCharacterBody UpdateTarget()
		{
			throw new NotImplementedException();
		}

		public override void UseAbility(CharacterMaster master, AbilitySlot abilitySlot,  AbilityUseInfo abilityUseInfo= null, EventChain eventChain=null)
		{
		}
	}

	public class AbilityUpgradeTree
	{
		public Dictionary<AbilityUpgrade, bool> IsUpgraded;
	}

	[GlobalClass]  // Enables it to be created in the Godot Editor
	public partial class AbilityUpgrade : Resource, IComparable<AbilityUpgrade>
	{
		[Export] public string Name;
		[Export] public string Description;
		[Export] public string AdvancedDescription;
		[Export] public int APCost;
		[Export] public int LockCost;
		[Export] public AbilityUpgrade[] Requirements;
		[Export] public int Height;
		[Export] public int Column;

		public int CompareTo(object compareTo)
		{
			if (compareTo is not AbilityUpgrade other)
				throw new ArgumentException("Object is not an AbilityUpgrade");

			// If 'other' is a requirement of this, 'this' is always greater
			if (Requirements.Contains(other))
				return 1;

			// If 'this' is a requirement of 'other', 'this' is always smaller
			if (other.Requirements.Contains(this))
				return -1;

			// Compare by height (higher values are considered "greater")
			return Height.CompareTo(other.Height);
		}

		public int CompareTo(AbilityUpgrade other)
		{
			// If 'other' is a requirement of this, 'this' is always greater
			if (Requirements.Contains(other))
				return 1;

			// If 'this' is a requirement of 'other', 'this' is always smaller
			if (other.Requirements.Contains(this))
				return -1;

			// Compare by height (higher values are considered "greater")
			return Height.CompareTo(other.Height);
		}
	}



	public enum AbilitySlotType
	{
		Primary,Secondary,Utility,Special,Ult, Equipment
	}

	public enum AbilityStat
	{
		Damage, CritChance, CritDamage, FireRate, ProjectileSpeed, Charges, Range, Duration, Chance, ProcChance, PositiveEffect,NegativeEffect
	}
}
