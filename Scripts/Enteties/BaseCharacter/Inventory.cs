using BrannPack.ItemHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.ItemHandling
{
    public class Inventory
    {
        protected InventoryPartition _allEffectivePartitions;
        public InventoryPartition AllEffectivePartitions
        {
            get => _allEffectivePartitions;
            protected set => _allEffectivePartitions = value;
        }

        public List<InventoryPartition> HighlanderPartitions = new List<InventoryPartition>() { };
        public InventoryPartition StandardPartition;
        public InventoryPartition ActiveItemPartition;
        public InventoryPartition ConfirmationPartition;

        public void RefreshAllEffectivePartitions()
        {
            _allEffectivePartitions = InventoryPartition.ForceMergePartitions(new List<InventoryPartition> { StandardPartition, ActiveItemPartition, ConfirmationPartition }.Concat(HighlanderPartitions).ToList(), this);
        }



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

    public abstract class InventoryBehaviorData { }

    public class PermanentItemBehavior : InventoryBehavior<PermanentItemBehavior>
    {

    }

    public class InventoryPartition
    {
        private const float InfiniteStackValue = -99f;

        public Inventory PartitionOf;
        public Dictionary<Item, List<InventoryItemStack>> ItemsInPartition;
        public ItemFilter AllowedInPartition;
        public float MaxCount;
        public float CurrentCount;
        public bool NeedsConfirmation;

        public InventoryPartition(Inventory inventory, ItemFilter allowedInPartition = null, float maxCount = InfiniteStackValue, bool needsConfirmation = false)
        {
            PartitionOf = inventory;
            ItemsInPartition = new Dictionary<Item, List<InventoryItemStack>>();
            AllowedInPartition = allowedInPartition;
            MaxCount = maxCount;
            CurrentCount = 0;
            NeedsConfirmation = false;
        }

        public bool TryAddToPartition(Item item, HashSet<ItemModifier> itemModifiers = null, float count = 1, HashSet<InventoryBehavior> inventoryBehaviors = null) { return TryAddToPartition(new InventoryItemStack(item, itemModifiers, count, inventoryBehaviors)); }

        public bool TryAddToPartition(InventoryItemStack inventoryItemStack)
        {
            if (IsItemAllowed(inventoryItemStack))
            {
                if (!ItemsInPartition.ContainsKey(inventoryItemStack.Item))
                {
                    ItemsInPartition.Add(inventoryItemStack.Item, new List<InventoryItemStack>() { inventoryItemStack });
                    return true;
                }
                foreach (InventoryItemStack iis in ItemsInPartition[inventoryItemStack.Item])
                {
                    if (iis.TryAddToStack(inventoryItemStack))
                        return true;
                }

                //If you made it this far, there is no home for this item. So we have to make one for it
                ItemsInPartition[inventoryItemStack.Item].Add(inventoryItemStack);
            }
            return false;
        }

        public bool IsItemAllowed(InventoryItemStack itemStack)
        {
            return !IsFull() && IsItemApplicable(itemStack.Item);
        }

        public bool IsFull() { return (MaxCount != InfiniteStackValue && CurrentCount >= MaxCount); }
        public bool IsItemApplicable(Item item) { return AllowedInPartition != null && !AllowedInPartition.IsItemApplicable(item); }

        public static InventoryPartition ForceMergePartitions(List<InventoryPartition> inventoryPartitions, Inventory inventory)
        {
            Dictionary<Item, List<InventoryItemStack>> items = new Dictionary<Item, List<InventoryItemStack>>();
            foreach (InventoryPartition ip in inventoryPartitions)
                foreach (var kvp in ip.ItemsInPartition)
                {
                    if (!items.TryAdd(kvp.Key, kvp.Value))
                        items[kvp.Key].AddRange(kvp.Value);
                }
            InventoryPartition ans = new InventoryPartition(inventory);
            ans.ItemsInPartition = items;
            return ans;
        }



        public enum FromCondition
        {
            Primary, Secondary, Utility, Special, Ult,
            Boss, NonBoss, Elite, Player
        }
    }
    public class InventoryItemStack
    {
        public Item Item;
        public HashSet<ItemModifier> ItemModifiers;
        public HashSet<InventoryBehavior> InventoryBehaviors;
        public float Count;
        public bool NeedsConfirmation;

        public InventoryItemStack(Item item, HashSet<ItemModifier> itemModifiers = null, float count = 0, HashSet<InventoryBehavior> inventoryBehaviors = null)
        {
            if (itemModifiers == null)
                itemModifiers = new HashSet<ItemModifier>() { };

            if (inventoryBehaviors == null)
                inventoryBehaviors = new HashSet<InventoryBehavior>() { PermanentItemBehavior.instance };

            Item = item;
            ItemModifiers = itemModifiers;
            InventoryBehaviors = inventoryBehaviors;
            Count = count;
        }

        public bool TryAddToStack(Item otherItem, HashSet<ItemModifier> otherItemModifiers, float otherCount = 1f, HashSet<InventoryBehavior> otherInventoryBehavior = null)
        {
            if (otherInventoryBehavior == null)
                otherInventoryBehavior = new HashSet<InventoryBehavior>() { PermanentItemBehavior.instance };

            if (!CanItemStack(otherItem, otherItemModifiers, otherInventoryBehavior))
                return false;

            Count += otherCount;
            return true;
        }

        public bool TryAddToStack(InventoryItemStack otherItemStack) { return TryAddToStack(otherItemStack.Item, otherItemStack.ItemModifiers, otherItemStack.Count, otherItemStack.InventoryBehaviors); }
        public bool CanItemStack(Item otherItem, HashSet<ItemModifier> otherItemModifiers, HashSet<InventoryBehavior> otherInventoryBehavior) { return otherItem == Item && otherItemModifiers.SetEquals(ItemModifiers) && otherInventoryBehavior.SetEquals(InventoryBehaviors); }

        public bool CanItemStack(InventoryItemStack otherItemStack) { return CanItemStack(otherItemStack.Item, otherItemStack.ItemModifiers, otherItemStack.InventoryBehaviors); }
    }
}
