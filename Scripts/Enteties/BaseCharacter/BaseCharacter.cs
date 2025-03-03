using Godot;
using System;
using System.Collections.Generic;
using BrannPack;
using System.ComponentModel;
using System.Linq;

namespace BrannPack
{
	public partial class BaseCharacter : CharacterBody2D
	{
		private static float DefaultMaxHealth;
		private static float DefaultMaxShield;
		private static float DefaultRegen;
		private static float DefaultBarrierLossRate;
		private static float DefaultDamage;
		private static float DefaultRange;
		private static float DefaultDuration;
        private static float DefaultSpeed;
		private static float DefaultCritChance;
		private static float DefaultCritDamage;

        private float CurrentMaxHealth;
		private float CurrentHealth;
		private float CurrentRegen;
		private float CurrentMaxShield;
		private float CurrentShieldRegenDelay;
		private float CurrentShieldRegenRate;
		private float CurrentShield;
		private float CurrentArmorGainMult;
		private float CurrentArmor;
		private float CurrentBarrierGainMult;
		private float CurrentBarrier;
		private float CurrentBarrierLossRate;
        private float CurrentSpeed;
        private Dictionary<string,float> CurrentResistance;
		private Dictionary<string,float> CurrentDamage;
		private Dictionary<string, float> CurrentRange;
		private Dictionary<string, float> CurrentDuration;
		private Dictionary<string, float> CurrentCritChance;
		private Dictionary<string, float> CurrentCritDamage;

		private Dictionary<string, Ability> Abilities;
		private Inventory Inventory;

		public void CalculateStats()
		{
			
		}

		public partial class StatHookEventArgs: EventArgs
		{
			//Health
			public float MaxHealthMultAdd = 0f;
			public float MaxHealthFlatAdd = 0f;

			//Regen
			public float RegenMultAdd = 0f;
			public float RegenFlatAdd = 0f;

			//Shield
			public float MaxShieldMultAdd = 0f;
			public float MaxShieldFlatAdd = 0f;
			public float ShieldRegenDelayMultAdd = 0f;
			public float ShieldRegenDelayFlatAdd = 0f;
			public float ShieldRegenRateMultAdd = 0f;
			public float ShieldRegenRateFlatAdd = 0f;

			//Armor
			public float ArmorGainMultAdd = 0f;

			//Barrier
			public float BarrierGainMultAdd = 0f;
			public float BarrierDecayMultAdd = 0f;

			//Speed
			public float SpeedGainMultAdd = 0f;
			public float SpeedGainFlatAdd = 0f;

            //Resistance (-100%,100%)
            //[1-(1-PositiveResist1)(1-PositiveResist2)...]-[1-(1-NegativeResist1)(1-NegativeResist2)...]
            public Dictionary<string, List<float>> ResistanceMultAdd = new Dictionary<string, List<float>>(); 

            public Dictionary<string, float> DamageReductionFlatAdd = new Dictionary<string, float>();

            //Damage
            public Dictionary<string, float> DamageDeltMultAdd = new Dictionary<string, float>();
            public Dictionary<string, float> DamageDeltFlatAdd = new Dictionary<string, float>();

            //Range
            public Dictionary<string, float> RangeMultAdd = new Dictionary<string, float>();
            public Dictionary<string, float> RangeFlatAdd = new Dictionary<string, float>();

            //Duration
            public Dictionary<string, float> DurationMultAdd = new Dictionary<string, float>();
            public Dictionary<string, float> DurationFlatAdd = new Dictionary<string, float>();

            //CritChance
            public Dictionary<string, float> CritChanceMultAdd = new Dictionary<string, float>();
            public Dictionary<string, float> CritChanceFlatAdd = new Dictionary<string, float>();

            //CritDamage
            public Dictionary<string, float> CritDamageMultAdd = new Dictionary<string, float>();
            public Dictionary<string, float> CritDamageFlatAdd = new Dictionary<string, float>();

        }

	}

	public class Inventory
	{
		private Dictionary<Item, float> PermanentItems;

		public Dictionary<Item, float> TotalEffectiveItems()
		{
			return null;
		}
	}

	public enum FromCondition
	{
		Primary, Secondary, Utility, Special, Ult,
		Boss, NonBoss, Elite, Player
	}
}
