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

        public CooldownHandler<Item> ItemCooldowns;


        public Inventory Inventory;
        private List<BaseCharacterBody> Minions;
        private List<BaseCharacterBody> Familiars;

        public void DealDamage(BaseCharacterBody victim, float Damage)
        {

        }
    }
}
