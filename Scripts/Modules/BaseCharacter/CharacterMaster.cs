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

        private Dictionary<(StatModTarget, Stat), ModifiableStat> AbilityStatModifiers;
        private Dictionary<(ItemStackFilter, Stat), ModifiableStat> ItemStatModifiers;

        public Inventory Inventory;
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

        public StatsHolder<BaseCharacterBody> Stats;

        public static event Action<CharacterMaster,CharacterMaster, DamageInfo, EventChain> BeforeDealDamage;

        public void DealDamage(CharacterMaster victim, DamageInfo damageInfo,EventChain eventChain)
        {
            BeforeDealDamage?.Invoke(this, victim, damageInfo, eventChain);

            //Do damage stuff

            //AfterDealDamage

        }
    }
}
