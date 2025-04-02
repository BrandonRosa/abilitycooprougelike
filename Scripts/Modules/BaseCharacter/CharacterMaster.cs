using BrannPack.AbilityHandling;
using BrannPack.CooldownHandling;
using BrannPack.ItemHandling;
using BrannPack.ModifiableStats;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BrannPack.Character.BaseCharacterBody;
using static BrannPack.ModifiableStats.AbilityStats;

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

        public AbilitySlot Primary;
        public AbilitySlot Secondary;
        public AbilitySlot Utility;
        public AbilitySlot Special;
        public AbilitySlot Ult;
        public AbilitySlot Equipment;

        public HealthBar HealthBar;

        public CooldownHandler Cooldowns;

        public StatsHolder<CharacterMaster> Stats;

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
