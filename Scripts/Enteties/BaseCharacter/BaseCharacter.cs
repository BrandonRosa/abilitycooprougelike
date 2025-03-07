using Godot;
using System;
using System.Collections.Generic;
using BrannPack;
using System.ComponentModel;
using System.Linq;
using BrannPack.ItemHandling;
using System.Threading;
using BrannPack.Tiers;
using BrannPack.ItemHandling;
using System.Security.Cryptography.X509Certificates;

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
		private List<BaseCharacter> Minions;
		private List<BaseCharacter> Familiars;

        public partial class StatHookEventArgs : EventArgs
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

        public event EventHandler<StatHookEventArgs> OnStatCalculation;

        public void RecalculateStats()
        {
            // Step 1: Create a new event argument object to hold stat modifications
            StatHookEventArgs statArgs = new StatHookEventArgs();

            // Step 2: Invoke the event for all listeners (items, buffs, etc.)
            OnStatCalculation?.Invoke(this, statArgs);

            // Step 3: Apply stat modifications
            ApplyStatChanges(statArgs);
        }

		private void ApplyStatChanges(StatHookEventArgs statArgs)
		{
		}

        

    }

	public class Inventory
	{
        public List<InventoryPartition> AllEffectivePartitions;
        public List<InventoryPartition> HighlanderPartitions= new List<InventoryPartition>() { };
        public InventoryPartition StandardPartition = new InventoryPartition();
        public InventoryPartition ActiveItemPartition = new InventoryPartition();
        public InventoryPartition ConfirmationPartition = new InventoryPartition();

        private Dictionary<Item, List<InventoryItemStack>> _inventoryItems;

        public IReadOnlyDictionary<Item, List<InventoryItemStack>> InventoryItems => _inventoryItems;


        //MAKE THIS SUBSCRIBABLE
        //Add's item to its ItemBehavior holder and to the TotalEffectiveItems dictionary (To avoid using the slow Refresh method)
        public bool AddItemToInventory(Item item, HashSet<ItemModifier> modifiers = null, float count = 1f, HashSet<InventoryBehavior> inventoryBehaviors = null) { return AddItemToInventory(new InventoryItemStack(item, modifiers, count, inventoryBehaviors)); }
        public bool AddItemToInventory(InventoryItemStack inventoryItemStack)
        {

            if (!_inventoryItems.ContainsKey(inventoryItemStack.Item))
            {
                _inventoryItems.Add(inventoryItemStack.Item, new List<InventoryItemStack>() { inventoryItemStack });
                return true;
            }

            bool added = false;
            foreach (InventoryItemStack itemStack in _inventoryItems[inventoryItemStack.Item])
                if (itemStack.TryAddToStack(inventoryItemStack))
                {
                    added = true;
                    break;
                }

            return added;
        }

        //Use this to avoid needing to refresh the TotalEffectiveItems list
        private bool AddItemToTotalEffectiveItems(Item item, ItemModifier modifier, float count = 1)
        {

        }
    }

    public abstract class InventoryBehavior<T> : InventoryBehavior where T : InventoryBehavior<T>
    {
        public static T instance { get; private set; }

        public InventoryBehavior()
        {
            if (instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ItemBase was instantiated twice");
            instance = this as T;
        }
    }

    //This will be for Permanent/Temporary/Fragile Items!
    public abstract class InventoryBehavior
    {

    }

    public class PermanentItemBehavior: InventoryBehavior<PermanentItemBehavior>
    {

    }

    public class InventoryPartition
    {
        private const float InfiniteStackValue=-99f;

        public Dictionary<Item, List<InventoryItemStack>> ItemsInPartition;
        public ItemFilter AllowedInPartition;
        public float MaxCount;
        public float CurrentCount;

        public InventoryPartition(ItemFilter allowedInPartition=null,float maxCount=InfiniteStackValue)
        {
            ItemsInPartition = new Dictionary<Item, List<InventoryItemStack>>();
            AllowedInPartition = allowedInPartition;
            MaxCount = maxCount;
            CurrentCount = 0;
        }

        public bool IsItemAllowed(InventoryItemStack itemStack)
        {
            if (!IsFull())
                return false;

            if (AllowedInPartition != null && !AllowedInPartition.IsItemApplicable(itemStack.Item))
                return false;

            return true;
        }

        public bool IsFull() { return (MaxCount != InfiniteStackValue && CurrentCount >= MaxCount); }

    public class InventoryItemStack
    {
        public Item Item;
        public HashSet<ItemModifier> ItemModifiers;
        public HashSet<InventoryBehavior> InventoryBehaviors;
        public float Count;

        public InventoryItemStack(Item item, HashSet<ItemModifier> itemModifiers=null, float count = 0, HashSet<InventoryBehavior> inventoryBehaviors=null)
        {
            if (itemModifiers== null)
                itemModifiers = new HashSet<ItemModifier>() {};

            if (inventoryBehaviors == null)
                inventoryBehaviors = new HashSet<InventoryBehavior>() { PermanentItemBehavior.instance };

            Item = item;
            ItemModifiers = itemModifiers;
            InventoryBehaviors = inventoryBehaviors;
            Count = count;
        }

        public bool TryAddToStack(Item otherItem, HashSet<ItemModifier> otherItemModifiers, float otherCount=1f, HashSet<InventoryBehavior> otherInventoryBehavior= null) 
        {
            if (otherInventoryBehavior == null)
                otherInventoryBehavior = new HashSet<InventoryBehavior>() { PermanentItemBehavior.instance };

            if (!CanItemStack(otherItem, otherItemModifiers,otherInventoryBehavior))
                return false;

            Count += otherCount;
            return true;
        }

        public bool TryAddToStack(InventoryItemStack otherItemStack) { return TryAddToStack(otherItemStack.Item, otherItemStack.ItemModifiers, otherItemStack.Count, otherItemStack.InventoryBehaviors); }
        public bool CanItemStack(Item otherItem, HashSet<ItemModifier> otherItemModifiers, HashSet<InventoryBehavior> otherInventoryBehavior) { return otherItem == Item && otherItemModifiers.SetEquals(ItemModifiers) && otherInventoryBehavior.SetEquals(InventoryBehaviors); }

        public bool CanItemStack(InventoryItemStack otherItemStack) { return CanItemStack(otherItemStack.Item, otherItemStack.ItemModifiers, otherItemStack.InventoryBehaviors); }
    }

	public enum FromCondition
	{
		Primary, Secondary, Utility, Special, Ult,
		Boss, NonBoss, Elite, Player
	}
}
