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
		private float MaxHealth;
		private float CurrentHealth;
		private float MaxShield;
		private float CurrentShield;
		private float CurrentArmor;
		private float CurrentBarrier;
		private float BarrierLossRate;
		private float BaseResistance;
		private float BaseDamage;
		private float BaseRange;
		private float BaseDuration;
		private float BaseSpeed;
		private float BaseCritChance;
		private float BaseCritDamage;

		private Dictionary<string, Ability> Abilities;
		private Inventory Inventory;


	}

	public class Inventory
	{
		private Dictionary<Item,float> PermanentItems;
		private List<TemporaryItem> TemporaryItems;

		public Dictionary<Item, float> TotalEffectiveItems()
		{
			return null;
		}
	}
}
