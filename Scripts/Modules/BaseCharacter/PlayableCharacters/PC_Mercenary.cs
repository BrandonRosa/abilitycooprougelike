using BrannPack.AbilityHandling;
using BrannPack.CooldownHandling;
using BrannPack.Forces;
using BrannPack.Helpers.Attacks;
using BrannPack.Helpers.RecourcePool;
using BrannPack.InputHelpers;
using BrannPack.ModifiableStats;
using BrannPack.Projectile;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static BrannPack.ModifiableStats.AbilityStats;
using static System.Net.Mime.MediaTypeNames;

namespace BrannPack.Character.Playable
{
	

	public static class PC_Mercenary
	{
		//public static PackedScene Prefab { get; set; } = (PackedScene)ResourceLoader.Load("res://path_to_prefab.tscn");
	}

	public class SMG: Ability<SMG>
	{
		protected static RangeStat BulletRange = new RangeStat(400f, 600f, 250f);
		protected static DamageStat Damage = new DamageStat(1.5f, .45f);
		protected static FireRateStat FireRate = new FireRateStat(8f);

		public override StatsByCritera<AbilityUpgrade> Stats { get; protected set; } = new StatsByCritera<AbilityUpgrade>(new Dictionary<Stat, ModifiableStat>()
		{
			{ Stat.Range, BulletRange},
			{Stat.Damage, Damage },
			{Stat.FireRate, FireRate }
		},
			new Dictionary<AbilityUpgrade, Dictionary<Stat, ModifiableStat>>())
		{ };

		public override string Name { get; protected set; } = "SMG";
		public override string CodeName { get; protected set; } = "Mercenary_SMG";
		public override string Description { get; protected set; }
		public override string AdvancedDescription { get; protected set; }
		public override Texture2D Icon { get; protected set; } = GD.Load<Texture2D>("res://Assets/PlaceholderAssets/AbilityIcons/Phase_Blast.png");

		//public AbilityUpgrade SSG_U1_Cooldown=
		public override BaseCharacterBody UpdateTarget()
		{
			return null;
		}

		public override void UseAbility(CharacterMaster master, AbilitySlot abilitySlot, AbilityUseInfo abilityUseInfo = null, EventChain eventChain = null)
		{
			//Damage Scaling Per Charge
			//Every Charged shot deals 50% more damage than the last
			//=x+(x-1)*.5 =1.5x-.5   =.5(3x-1)
			Vector2 fireDirection= abilityUseInfo.DirectionTo?? master.Body.AimDirection;
			if (master.Body.CooldownHandler.IsOnCooldown((1, this.Index, 0)))
				return;
			float consumedCharges = abilitySlot.CCooldown.CurrentCharges;
			var bullet=PoolManager.PoolManagerNode.Spawn<BaseProjectile>("BasicEnemyBullet", PoolManager.ProjectilesNode);
			var damagestat = Damage.GetCombinedStat(abilitySlot.ThisAbilityStats.GetStatByVariable<DamageStat>(Stat.Damage));
			var rangestat = BulletRange.GetCombinedStat(abilitySlot.ThisAbilityStats.GetStatByVariable<RangeStat>(Stat.Range));
			var fireratestat = FireRate.GetCombinedStat(abilitySlot.ThisAbilityStats.GetStatByVariable<FireRateStat>(Stat.FireRate));
			bullet.Initialize(new ProjectileInfo(master, null, (1, this.Index, 0), damagestat.CalculateTotal(), false, actionSourceType:abilityUseInfo.AbilitySlot.SlotType,isSourcePsudo:false,projectileName: "BasicEnemyBullet", direction: fireDirection, position: master.Body.GlobalPosition, duration: float.MaxValue, range: rangestat.CalculateTotal(),speed:1500f));
			float cooldown=1f / (fireratestat.CalculateTotal());
			master.Body.CooldownHandler.AddCooldown((1, this.Index, 0), cooldown);
		}
	}

	public class MercenaryPierceShot : Ability<MercenaryPierceShot>
	{
		//private float BlastWidth = 15;
		protected static float CooldownReductionPerDamage = .2f/2f;
		protected static RangeStat BulletRange = new RangeStat(100f, 500f, .5f);
		protected static DamageStat Damage = new DamageStat(60, .9f);
		protected static CooldownStat Cooldown = new CooldownStat(8f,disableStandardTimeScale:true);
		protected static CooldownStat SpamCooldown = new CooldownStat(.1f);
		protected static ChargeStat Charges = new ChargeStat(2f);

		public override StatsByCritera<AbilityUpgrade> Stats { get; protected set; } = new StatsByCritera<AbilityUpgrade>(new Dictionary<Stat, ModifiableStat>()
			{
				{ Stat.Range, BulletRange },
				{ Stat.Damage, Damage },
				{ Stat.Cooldown, Cooldown },
				{ Stat.Charges, Charges },
				{Stat.SpamCooldown, SpamCooldown },
			},
		   new Dictionary<AbilityUpgrade, Dictionary<Stat, ModifiableStat>>()) {
		};
		public override string Name { get; protected set; } = "PierceShot";
		public override string CodeName { get; protected set; } = "Mercenary_PierceShot";
		public override string Description { get; protected set; }
		public override string AdvancedDescription { get; protected set; }
		public override Texture2D Icon { get; protected set; } = GD.Load<Texture2D>("res://Assets/PlaceholderAssets/AbilityIcons/Phase_Blast.png");

		public override void Init()
		{
			base.Init();
			CharacterMaster.AfterTakeDamage += OnAfterTakeDamage;
		}

		
		private void OnAfterTakeDamage(CharacterMaster victim, CharacterMaster attacker, DamageInfo info, EventChain chain)
		{
			if(info.ActionSourceType==EffectSourceType.Primary)
			{
				AbilitySlot foundAbility = attacker.LocateAbility(this);

				if (foundAbility != null)
				{
					float finalReduction = info.Damage*CooldownReductionPerDamage;
					foundAbility.CCooldown.Update(finalReduction);
					
					//NEED TO MAKE AN ATTACK EVENT AND BROADCAST IT
					//ATTACK EVENT NEEDS TO CHAIN TO END OF CHAIN

				}
			}
			
		}

		//public AbilityUpgrade SSG_U1_Cooldown=
		public override BaseCharacterBody UpdateTarget()
		{
			return null;
		}

		public override void UseAbility(CharacterMaster master, AbilitySlot abilitySlot, AbilityUseInfo abilityUseInfo = null, EventChain eventChain=null)
		{
			//GD.Print("SHOTGUN","Charge"+abilitySlot.CurrentCharges,"type"+abilityUseInfo.PressState);
			if (abilityUseInfo.PressState != InputPressState.JustPressed)
				return;
			//var slot_range = abilitySlot.ThisAbilityStats.GetStatByVariable<RangeStat>(Stat.Range);
			//float range=slot_range!=null? BlastRange.GetCombinedTotal(slot_range):BlastRange.CalculateTotal();
			//float damage = Damage.GetCombinedTotal(abilitySlot.ThisAbilityStats.GetStatByVariable<DamageStat>(Stat.Damage));
			//GD.Print(range, damage," to"+master.Body.AimDirection);
			//List<BaseCharacterBody> charactersInBlast=AttackHelper.GetCharactersInShotgunBlast(master.Body, master.Body.GetGlobalTransform(), master.Body.AimDirection.Angle(), BlastWidth, range, 8);
			//foreach(BaseCharacterBody characterBody in charactersInBlast)
			//{
			//	GD.Print(characterBody.CharacterName);
			//	if (true)//master.CanDamageTeams.Contains(characterBody.CharacterMaster.Team))
			//	{
			//		DamageInfo info = new DamageInfo(master,characterBody.CharacterMaster,(1,this.Index,0),damage,false);
			//		master.DealDamage(characterBody.CharacterMaster, info,null);
			//	}
			//}

			abilitySlot.CCooldown.TryDepleteCharges();
			abilitySlot.CCooldown.Reset();
			//GD.Print(abilitySlot.CurrentCharges, " " + abilitySlot.IsUsable, " " + abilitySlot.CCooldown.Duration, " " + abilitySlot.CCooldown.IsPaused);
		}

	}

	//public class ScoutGrappleHook : Ability<ScoutGrappleHook>
	//{
	//       protected static RangeStat Range = new RangeStat(400f, 3000f, 500f);
	//       protected static DamageStat Damage = new DamageStat(0, 0f);
	//       protected static CooldownStat Cooldown = new CooldownStat(.1f);
	//       protected static CooldownStat SpamCooldown = new CooldownStat(.5f);
	//       protected static ChargeStat Charges = new ChargeStat(1f);

	//       public override StatsByCritera<AbilityUpgrade> Stats { get; protected set; } = new StatsByCritera<AbilityUpgrade>(new Dictionary<Stat, ModifiableStat>()
	//           {
	//               { Stat.Range, Range },
	//               { Stat.Damage, Damage },
	//               { Stat.Cooldown, Cooldown },
	//               { Stat.Charges, Charges },
	//               {Stat.SpamCooldown, SpamCooldown },
	//           },
	//          new Dictionary<AbilityUpgrade, Dictionary<Stat, ModifiableStat>>())
	//       {
	//       };
	//       public override string Name { get; protected set; } = "Grapple Hook";
	//       public override string CodeName { get; protected set; } = "Scout_Grapple";
	//       public override string Description { get; protected set; }
	//       public override string AdvancedDescription { get; protected set; }
	//       public override Texture2D Icon { get; protected set; } = GD.Load<Texture2D>("res://Assets/PlaceholderAssets/AbilityIcons/Phase_Blast.png");

	//       //public AbilityUpgrade SSG_U1_Cooldown=
	//       public override BaseCharacterBody UpdateTarget()
	//       {
	//           return null;
	//       }

	//       public override void UseAbility(CharacterMaster master, AbilitySlot abilitySlot, AbilityUseInfo abilityUseInfo, EventChain eventChain)
	//       {
	//           if (abilityUseInfo.PressState != InputPressState.JustPressed)
	//               return;
	//           var bullet = PoolManager.PoolManagerNode.Spawn<BaseProjectile>("GrappleProjectile", PoolManager.ProjectilesNode);
	//           var damagestat = Damage.GetCombinedStat(abilitySlot.ThisAbilityStats.GetStatByVariable<DamageStat>(Stat.Damage));
	//           var rangestat = Range.GetCombinedStat(abilitySlot.ThisAbilityStats.GetStatByVariable<RangeStat>(Stat.Range));
	//           var cooldownStat = Cooldown.GetCombinedStat(abilitySlot.ThisAbilityStats.GetStatByVariable<CooldownStat>(Stat.Cooldown));
	//           bullet.Initialize(new ProjectileInfo(master, master, (1, this.Index, 0), damagestat.CalculateTotal(), false, projectileName: "GrappleProjectile", direction: master.Body.AimDirection, position: master.Body.GlobalPosition, duration: 15f, range: rangestat.CalculateTotal(), speed: 1500f,bodyCollideBehavior:ProjectileCollideBehavior.Pierce));

	//           abilitySlot.CCooldown.TryUseCharge();
	//           abilitySlot.CCooldown.Reset();
	//       }
	//   }

	public class RainOfBullets : Ability<RainOfBullets>
	{
		protected static RangeStat Range = new RangeStat(200f, 300f, 100f);
		protected static DurationStat Duration = new DurationStat(4f, 9f, .5f);
		protected static DamageStat Damage = new DamageStat(0, 1f);
		protected static CooldownStat Cooldown = new CooldownStat(1f);
		protected static CooldownStat SpamCooldown = new CooldownStat(1f);
		protected static ChargeStat Charges = new ChargeStat(1f);

		public override StatsByCritera<AbilityUpgrade> Stats { get; protected set; } = new StatsByCritera<AbilityUpgrade>(new Dictionary<Stat, ModifiableStat>()
			{
				{ Stat.Range, Range },
				{ Stat.Damage, Damage },
				{ Stat.Cooldown, Cooldown },
				{ Stat.Charges, Charges },
				{Stat.SpamCooldown, SpamCooldown },
			},
		   new Dictionary<AbilityUpgrade, Dictionary<Stat, ModifiableStat>>())
		{
		};
		public override string Name { get; protected set; } = "Rain Of Bulelts";
		public override string CodeName { get; protected set; } = "MERCENARY_RAINOFBULLETS";
		public override string Description { get; protected set; }
		public override string AdvancedDescription { get; protected set; }
		public override Texture2D Icon { get; protected set; } = GD.Load<Texture2D>("res://Assets/PlaceholderAssets/AbilityIcons/Phase_Blast.png");

		//public AbilityUpgrade SSG_U1_Cooldown=
		public override BaseCharacterBody UpdateTarget()
		{
			return null;
		}

		public override void UseAbility(CharacterMaster master, AbilitySlot abilitySlot, AbilityUseInfo abilityUseInfo, EventChain eventChain)
		{
			if (abilityUseInfo.PressState != InputPressState.JustPressed)
				return;
			float duration = Duration.GetCombinedTotal((abilitySlot.ThisAbilityStats.GetStatByVariable<DurationStat>(Stat.Duration)));
			Cooldown cooldown = new Cooldown(duration);
			master.Body.CooldownHandler.AddCooldown((1, this.Index, 1), cooldown);

			RainOfBulletsHandler rainOfBullets = new RainOfBulletsHandler() {
				Duration = cooldown,
				DamagePercent = 2,
				FireRatePercentIncrease = .35f,
				CooldownPercentDecrease = .35f,
				Range = Range.GetCombinedStat(abilitySlot.ThisAbilityStats.GetStatByVariable<RangeStat>(Stat.Range)).CalculateTotal(),
				AbilitySlot = master.Primary
			};
			master.Body.AddChild(rainOfBullets);
			abilitySlot.CCooldown.TryUseCharge();
			abilitySlot.CCooldown.Reset();
		}


	}

	public partial class RainOfBulletsHandler:Area2D
	{
		private readonly List<BaseCharacterBody> _enemiesInRange = new();

		public Cooldown Duration { get;  set; }
		public float DamagePercent { get;  set; }
		public float FireRatePercentIncrease { get;  set; }
		public float CooldownPercentDecrease { get;  set; }
		public float Range { get;  set; }

		public AbilitySlot AbilitySlot { get;  set; }

		public override void _Ready()
		{
			base._Ready();
			SetupEnemyDetector();
			AbilitySlot.BeforeAbilitySlotUse += AbilitySlot_BeforeAbilitySlotUse;
			AbilitySlot.ThisAbilityStats.RefreshAbilityStatVariable += ThisAbilityStats_RefreshAbilityStatVariable;
			AbilitySlot.ThisAbilityStats.RecalculateAllStats();
			Duration.CompletedCooldown += Duration_CompletedCooldown;
			
			
		}

		private void Duration_CompletedCooldown(Cooldown obj)
		{
			QueueFree();
		}

		public override void _ExitTree()
		{
			AbilitySlot.ThisAbilityStats.RefreshAbilityStatVariable -= ThisAbilityStats_RefreshAbilityStatVariable;
			AbilitySlot.BeforeAbilitySlotUse -= AbilitySlot_BeforeAbilitySlotUse;
			AbilitySlot.ThisAbilityStats.RecalculateAllStats();
			base._ExitTree();
		}

		private void SetupEnemyDetector()
		{

			var collision = new CollisionShape2D();
			var circle = new CircleShape2D
			{
				Radius = Range
			};
			
			collision.Shape = circle;
			this.AddChild(collision);
			BodyEntered += (body) =>
			{
				if (body is BaseCharacterBody enemy && AbilitySlot.Owner.CanDamageTeams.Contains(enemy.CharacterMaster.Team))
				{
					_enemiesInRange.Add(enemy);
				}
			};
			BodyExited += (body) =>
			{
				if (body is BaseCharacterBody enemy)
				{
					_enemiesInRange.Remove(enemy);
				}
			};
		}



		private void ThisAbilityStats_RefreshAbilityStatVariable(Stat arg1, ModifiableStat arg2)
		{
			switch (arg1)
			{
				case Stat.Damage:
					((DamageStat)arg2).AdditionalDamage += ((DamageStat)arg2).BaseValue * (1+DamagePercent);
					break;
				case Stat.FireRate:
					((FireRateStat)arg2).FireRateUpPercentage += FireRatePercentIncrease;
					break;
				case Stat.Cooldown:
					((CooldownStat)arg2).CooldownPercentDecreases.Add(CooldownPercentDecrease);
					break;
			}
		}

		//Redirect attack to nearby enemies. If there are no nearby enemies, do not allot ability use. 
		private void AbilitySlot_BeforeAbilitySlotUse(AbilitySlot obj,AbilityUseInfo info,EventChain chain)
		{
			if(obj==AbilitySlot)
			{
				BaseCharacterBody attacker = AbilitySlot.Owner.Body;
				if (_enemiesInRange.Count == 0)
				{
					
					return;
				}
				// Pick a random enemy
				int index = GD.RandRange(0, _enemiesInRange.Count - 1);
				BaseCharacterBody target = _enemiesInRange[index];

				if (!IsInstanceValid(target))
				{
					return;
				}

				// Redirect ability direction toward target
				Vector2 attackerPos = attacker.GlobalPosition;
				Vector2 targetPos = target.GlobalPosition;

				info.DirectionTo = (targetPos - attackerPos).Normalized();

			}
		
		}
	}


}
