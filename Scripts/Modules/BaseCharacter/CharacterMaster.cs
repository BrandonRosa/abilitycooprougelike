using BrannPack.AbilityHandling;
using BrannPack.CooldownHandling;
using BrannPack.ItemHandling;
using BrannPack.ModifiableStats;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using static BrannPack.ModifiableStats.AbilityStats;
using static BrannPack.ModifiableStats.CharacterStats;

namespace BrannPack.Character
{
    public partial class CharacterMaster:Node
    {

        public static List<CharacterMaster> AllMasters = new List<CharacterMaster>();

        public override void _Ready()
        {
            base._Ready();
            AllMasters.Add(this);
        }

        public override void _ExitTree()
        {
            AllMasters.Remove(this);
        }
        

        [Export] public EntityController Controller;

        [Export] public float StartingMoveSpeed;
        [Export] public float MoveSpeedMax;

        [Export] public float StartingMaxHealth;
        [Export] public float StartingHealthRegen;
        [Export] public float HealthRegenScaling;

        [Export] public List<string> StartingItems;

        [Export] private float AbilityScale;
        [Export] private float HealthScale;
        [Export] private float MoveSpeedScale;
        [Export] private float SizeScale;
        [Export] private bool IsPlayerControlled;
        [Export] public CharacterTeam Team;
        [Export] public HashSet<CharacterTeam> CanDamageTeams;

        public BaseCharacterBody Body;

        private Dictionary<(ItemStackFilter, Stat), ModifiableStat> ItemStatModifiers;

        public Inventory Inventory;
        public bool UsingInventory;
        private List<BaseCharacterBody> Minions;
        private List<BaseCharacterBody> Familiars;

        [Export] public AbilitySlot Primary;
        [Export] public AbilitySlot Secondary;
        [Export] public AbilitySlot Utility;
        [Export] public AbilitySlot Special;
        [Export] public AbilitySlot Ult;
        public AbilitySlot Equipment;

        public HealthBar HealthBar;

        public CooldownHandler Cooldowns;

        public StatsHolder<CharacterMaster> Stats;

        public void Init()
        {
            Stats = new StatsHolder<CharacterMaster>(this, StatsHolder.GetZeroStatsCopy());
            Dictionary<Stat,ModifiableStat> stats=new Dictionary<Stat,ModifiableStat>()
            {
                { Stat.MoveSpeed,new MoveSpeedStat(StartingMoveSpeed,MoveSpeedMax)},
                {Stat.MaxHealth, new MaxHealthStat(StartingMaxHealth) },
                {Stat.HealthRegen, new RegenStat(StartingHealthRegen,HealthRegenScaling) },
                {Stat.MaxArmor, new MaxHealthStat(0f) },
                {Stat.MaxShield, new MaxHealthStat(0f) },
                {Stat.MaxBarrier, new MaxHealthStat(0f)}
            };
            Stats.SetAllStats(stats, false);

            HealthBar= new HealthBar((MaxHealthStat)stats[Stat.MaxHealth], (MaxHealthStat)stats[Stat.MaxArmor], (MaxHealthStat)stats[Stat.MaxShield], (MaxHealthStat)stats[Stat.MaxBarrier])

            Inventory = new Inventory(this);
        }

        public static event Action<CharacterMaster,CharacterMaster, DamageInfo, EventChain> BeforeDealDamage;

        public void DealDamage(CharacterMaster victim, DamageInfo damageInfo,EventChain eventChain)
        {
            BeforeDealDamage?.Invoke(this, victim, damageInfo, eventChain);

            //Do damage stuff

            //AfterDealDamage

            ///Lifesteal Notes
            ///- When an attack is caused by something, ALWAYS add the chainlifesteal of the cause to the new attack (assuming the attack can deal damage). 


            //Adds the chainlifesteal/lifesteal of the damage to the chain lifesteal of the player.
            EffectivenessStat totalLifesteal = Stats.GetStatByVariable<EffectivenessStat>(Stat.ChainLifesteal)
                .GetCombinedStat(damageInfo.Stats.GetStatByVariable<EffectivenessStat>(Stat.ChainLifesteal),
                                damageInfo.Stats.GetStatByVariable<EffectivenessStat>(Stat.Lifesteal));
            float totalToHeal = totalLifesteal.CalculateTotal() * damageInfo.Damage;

            if (totalToHeal > 0f)
            {
                HealthBar.Heal(new HealingInfo(this,this,damageInfo.Key,totalToHeal), eventChain);
            }
        }

        public static event Action<CharacterMaster, CharacterMaster, DamageInfo, EventChain> BeforeTakeDamage;
        public static event Action<CharacterMaster, CharacterMaster, DamageInfo, EventChain> AfterTakeDamage;

        public void TakeDamage(CharacterMaster dealer,DamageInfo damageInfo, EventChain eventChain)
        {
            BeforeTakeDamage?.Invoke(this, dealer, damageInfo, eventChain);


            HealthBar.TakeDamage(damageInfo);


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

    public partial class HealthBar : GodotObject
    {

        public CharacterMaster Master;

        public Dictionary<HealthType,HealthBehavior> HealthList = new Dictionary<HealthType, HealthBehavior>{ };
        public List<(HealthType type, float startPosition, float width, bool isOverHealth)> UIInfo;

        public float HealthNumerator;
        public float HealthDenominator;
        public float CurrentValueVisible;
        public float CurrentMaxVisible;

        [Export] public EffectivenessStat DamageResistance;
        [Export] public EffectivenessStat HealingEffectiveness;

        //public HealthBar()
        //{
        //    Health health = new Health(0f, new MaxHealthStat(100f));

        //    Armor armor = new Armor(0f, new MaxHealthStat(0f));
        //    armor.MaxValue.AddFollowingMaxHealth(health.MaxValue);

        //    Shield shield = new Shield(0f, new MaxHealthStat(0f));

        //    BarrierHealth barrier = new BarrierHealth(0f, new MaxHealthStat(0f));
        //    barrier.MaxValue.AddFollowingMaxHealth(health.MaxValue);
        //    armor.AfterCurrentValueChange += (float currentValue, float valueadded, float overvalue) =>
        //    {
        //        if (currentValue > health.MaxValue.Total)
        //            barrier.MaxValue.ChangeAdditionalMaxHealth(Math.Max(0f, currentValue - health.MaxValue.Total));
        //    };
        //    barrier.MaxValue.AddFollowingMaxHealth(shield.MaxValue);


        //    AddHealthType(health, armor, shield, barrier);

        //}

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
        public static event Action<HealthBar, DamageInfo, (HealthType, float)[], float,float> AfterLoseHealth;



        //Returns the amount of ActualDamage taken.
        public float TakeDamage(DamageInfo damageInfo)
        {
            BeforeLoseHealth?.Invoke(this, damageInfo);

            float damageTaken = damageInfo.Damage;
            float leftoverDamage = damageTaken;
            float totalDamageTaken = 0f;
            HealthChangeInfo changeInfo = new HealthChangeInfo(damageInfo.Source, damageInfo.Destination, damageInfo.Key, -damageInfo.Damage, HealthType.Health, null);

            List<(HealthType, float)> damageTakenByType = new List<(HealthType, float)>();
            foreach(var type in HealthTypeOrder.Reverse())
            { 
                changeInfo.Change = -leftoverDamage;
                changeInfo.HealthType = type;
                var result = ChangeHealth(changeInfo); 
                totalDamageTaken += result.change; 
                if(result.change==0f)
                    damageTakenByType.Add((type, result.change));

                leftoverDamage = result.leftOverChange;
                if (leftoverDamage <= 0f)
                    break;
            }

            AfterLoseHealth?.Invoke(this, damageInfo, damageTakenByType.ToArray(), totalDamageTaken,leftoverDamage);
            UpdateUIHealthInfo();
            return totalDamageTaken;
        }
        public static event Action<HealthBar, HealthChangeInfo> BeforeHealthChange;
        public (float change,float leftOverChange) ChangeHealth(HealthChangeInfo changeInfo)
        {
            if (changeInfo.Change == 0)
                return (0f,changeInfo.Change);

            BeforeHealthChange?.Invoke(this, changeInfo);


            if (changeInfo.Change > 0)
                return HealthList[changeInfo.HealthType].TakeDamage(changeInfo);
            else
                return HealthList[changeInfo.HealthType].AddCurrentValue(changeInfo);

        }

        public float Heal(HealingInfo healingInfo, EventChain eventChain)
        {

        }

        public static event Action<HealthBar, HealthBehavior, float, float> AfterMaxHealthChange;

        public float GetMaxHealth()
        {

        }

        public float GetMaxShield()
        {

        }
    }
}
