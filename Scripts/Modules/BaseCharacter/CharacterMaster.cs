using BrannPack.AbilityHandling;
using BrannPack.CooldownHandling;
using BrannPack.DevConsole;
using BrannPack.ItemHandling;
using BrannPack.ModifiableStats;
using BrannPack.UI;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static BrannPack.ModifiableStats.AbilityStats;
using static BrannPack.ModifiableStats.CharacterStats;

namespace BrannPack.Character
{
	[GlobalClass]
	public partial class CharacterMaster : Node
	{

		public static List<CharacterMaster> AllMasters = new List<CharacterMaster>();
		[Export] public PackedScene BodyScene;

		public override void _Ready()
		{
			base._Ready();
			AllMasters.Add(this);

			if (Body == null && BodyScene != null)
			{
				Body = BodyScene.Instantiate<BaseCharacterBody>();
				AddChild(Body);
			   // Body.GlobalPosition = GlobalPosition; // or set Transform, etc.
				Body.Init(); // Pass reference to master if needed
			}

			Init(); // Your existing setup code
		}

		public override void _ExitTree()
		{
			AllMasters.Remove(this);
		}

		//Keyboard is -1. Other controllers are 0,1,2,3
		[Export] public int ControllerID;

		

		[Export] public string[] StartingItems;

		[Export] public bool IsPlayerControlled;
		[Export] public CharacterTeam Team;
		//[Export] public CharacterTeam[] InitialCanDamageTeams;

		public HashSet<CharacterTeam> CanDamageTeams;

		[Export] public BaseCharacterBody Body;
		[Export] public Marker2D SpawnPoint;

		private Dictionary<(ItemStackFilter, Stat), ModifiableStat> ItemStatModifiers;
		public EntityController Controller;

		public Inventory Inventory;
		public bool UsingInventory;
		private List<BaseCharacterBody> Minions;
		private List<BaseCharacterBody> Familiars;

		public AbilitySlot Primary;
		public AbilitySlot Secondary;
		public AbilitySlot Utility;
		public AbilitySlot Special;
		public AbilitySlot Ult;
		public AbilitySlot Equipment;

		

		public CooldownHandler Cooldowns;

		public StatsHolder<CharacterMaster> Stats;

		public bool IsAlive=> Body.HealthBar.CurrentValueVisible > 0f;
		public HealthBar HealthBar => Body.HealthBar;



		public void Init()
		{
			//Set Stats based off of body
			Stats = new StatsHolder<CharacterMaster>(this, StatsHolder.GetZeroStatsCopy());
			Dictionary<Stat,ModifiableStat> stats=new Dictionary<Stat,ModifiableStat>()
			{
				{ Stat.MoveSpeed,new MoveSpeedStat(Body.StartingMoveSpeed,Body.MoveSpeedMax)},
				{Stat.MaxHealth, new MaxHealthStat(Body.StartingMaxHealth) },
				{Stat.HealthRegen, new RegenStat(Body.StartingHealthRegen,Body.HealthRegenScaling) },
				{Stat.MaxArmor, new MaxHealthStat(0f) },
				{Stat.MaxShield, new MaxHealthStat(0f) },
				{Stat.MaxBarrier, new MaxHealthStat(0f)}
			};
			Stats.SetAllStats(stats, false);

			//Make the body share the healthbar and movespeed
			Body.HealthBar = new HealthBar(this, (MaxHealthStat)stats[Stat.MaxHealth], (MaxHealthStat)stats[Stat.MaxArmor], (MaxHealthStat)stats[Stat.MaxShield], (MaxHealthStat)stats[Stat.MaxBarrier]);
			Body.MoveSpeed = (MoveSpeedStat)stats[Stat.MoveSpeed];

			Inventory = new Inventory(this);

			//Set the abilities to the body's starting abilities
			Primary = new AbilitySlot(this, Body.StartingPrimary, AbilitySlotType.Primary);
			Primary.SetAbilityStats();
			Secondary = new AbilitySlot(this, Body.StartingSecondary, AbilitySlotType.Secondary);
			Secondary.SetAbilityStats();
			Utility = new AbilitySlot(this, Body.StartingUtility, AbilitySlotType.Utility);
			Utility.SetAbilityStats();
			Special = new AbilitySlot(this, Body.StartingSpecial, AbilitySlotType.Special);
			Special.SetAbilityStats();
			Ult = new AbilitySlot(this, Body.StartingUlt, AbilitySlotType.Ult);
			Ult.SetAbilityStats();
			Equipment = new AbilitySlot(this, Body.StartingEquipment, AbilitySlotType.Equipment);
			Equipment.SetAbilityStats();

			Body.CharacterMaster = this;

			Controller = IsPlayerControlled ? new LocalPlayerController() : new AIController();
			Controller.OwnerMaster = this;
			Controller.OwnerBody = Body;
			Body.Controller = Controller;//Controller;

			//Set floating healthbar
			//FloatingHealthBar HB = new FloatingHealthBar();
			//Body.AddChild(HB);
			//HB.Owner = Body;

			if(SpawnPoint != null)
			{
				Body.GlobalPosition = SpawnPoint.GlobalPosition;
			}
		}

		public static event Action<CharacterMaster,CharacterMaster, DamageInfo, EventChain> BeforeDealDamage;

		public void DealDamage(CharacterMaster victim, DamageInfo damageInfo,EventChain eventChain)
		{
			BeforeDealDamage?.Invoke(this, victim, damageInfo, eventChain);

			//Do damage stuff
			victim.TakeDamage(this, damageInfo, eventChain);
			//AfterDealDamage

			///Lifesteal Notes
			///- When an attack is caused by something, ALWAYS add the chainlifesteal of the cause to the new attack (assuming the attack can deal damage). 


			//Adds the chainlifesteal/lifesteal of the damage to the chain lifesteal of the player.
			//EffectivenessStat totalLifesteal = Stats.GetStatByVariable<EffectivenessStat>(Stat.ChainLifesteal)
			//    .GetCombinedStat(damageInfo.Stats.GetStatByVariable<EffectivenessStat>(Stat.ChainLifesteal),
			//                    damageInfo.Stats.GetStatByVariable<EffectivenessStat>(Stat.Lifesteal));
			float totalToHeal = 0f;//totalLifesteal.CalculateTotal() * damageInfo.Damage;

			if (totalToHeal > 0f)
			{
				Body.HealthBar.Heal(new HealingInfo(this,this,damageInfo.Key,totalToHeal), eventChain);
			}
		}

		public static event Action<CharacterMaster, CharacterMaster, DamageInfo, EventChain> BeforeTakeDamage;
		public static event Action<CharacterMaster, CharacterMaster, DamageInfo, EventChain> AfterTakeDamage;

		public void TakeDamage(CharacterMaster dealer,DamageInfo damageInfo, EventChain eventChain)
		{
			BeforeTakeDamage?.Invoke(this, dealer, damageInfo, eventChain);


			Body.HealthBar.TakeDamage(damageInfo);


			AfterTakeDamage?.Invoke(this, dealer, damageInfo, eventChain);
		}

		public static event Action<CharacterMaster,AttackInfo, EventChain> BeforeAttackEvent;
		public static event Action<CharacterMaster, AttackInfo, EventChain> AfterAttackEvent;
		public void BeforeAttack(AttackInfo attackInfo,EventChain eventChain)
		{
			BeforeAttackEvent?.Invoke(this,attackInfo, eventChain);
			

			//AfterAttack
		}

		public void AfterAttack(AttackInfo attackInfo, EventChain eventChain) { AfterAttackEvent?.Invoke(this, attackInfo, eventChain); }
	}

	
}
