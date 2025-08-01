﻿using BrannPack.ItemHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrannPack.Tiers;
using System.Collections.ObjectModel;
using BrannPack.ModifiableStats;
using BrannPack.Character;
using System.Runtime.CompilerServices;
using static BrannPack.ModifiableStats.AbilityStats;
using static BrannPack.Character.BaseCharacterBody;
using Godot;

namespace BrannPack.ItemHandling
{
    public class Inventory
    {
        public static event Action<Inventory, InventoryItemStack> ItemsAdded;
        public static event Action<Inventory, InventoryItemStack> ItemsRemoved;

        public static readonly ItemStackFilter HighlanderT0 = new ItemStackFilter.ItemStackFilterBuilder().WithAnyTiers(Tier0.instance).IsAllModifiers(Highlander.instance).Build();
        public static readonly ItemStackFilter HighlanderT1 = new ItemStackFilter.ItemStackFilterBuilder().WithAnyTiers(Tier1.instance).IsAllModifiers(Highlander.instance).Build();
        public static readonly ItemStackFilter HighlanderT2 = new ItemStackFilter.ItemStackFilterBuilder().WithAnyTiers(Tier2.instance).IsAllModifiers(Highlander.instance).Build();
        public static readonly ItemStackFilter HighlanderT3 = new ItemStackFilter.ItemStackFilterBuilder().WithAnyTiers(Tier3.instance).IsAllModifiers(Highlander.instance).Build();
        public static readonly ItemStackFilter HighlanderT4 = new ItemStackFilter.ItemStackFilterBuilder().WithAnyTiers(Tier4.instance).IsAllModifiers(Highlander.instance).Build();
        public static readonly ItemStackFilter ActiveItemFilter = new ItemStackFilter.ItemStackFilterBuilder().WithAnyTiers(TierA.instance).Build();

        protected InventoryPartition _allEffectivePartitions;
        public InventoryPartition AllEffectivePartitions
        {
            get => _allEffectivePartitions;
            protected set => _allEffectivePartitions = value;
        }
        protected Dictionary<Item, ItemEffectModifier> _allEffectiveItemCount;
        public Dictionary<Item, ItemEffectModifier> AllEffectiveItemCount => _allEffectiveItemCount;

        public List<InventoryPartition> HighlanderPartitions = new List<InventoryPartition>() 
            { 
                new InventoryPartition(null,HighlanderT0,1f),
                new InventoryPartition(null,HighlanderT1,1f),
                new InventoryPartition(null,HighlanderT2,1f),
                new InventoryPartition(null,HighlanderT3,1f),
                new InventoryPartition(null,HighlanderT4,1f)

            };
        public InventoryPartition StandardPartition= new InventoryPartition(null);
        public InventoryPartition ActiveItemPartition= new InventoryPartition(null,ActiveItemFilter , 1f);
        public InventoryPartition ConfirmationPartition= new InventoryPartition(null,null,float.MaxValue,true);

        public CharacterMaster InventoryOf;

        public StatsHolder<ItemStackFilter> AdditionalAbilityStatsByStackFilter;

        public Inventory(CharacterMaster master)
        {
            HighlanderPartitions.ForEach(part => part.PartitionOf = this);
            StandardPartition.PartitionOf = this;
            ActiveItemPartition.PartitionOf = this;
            ConfirmationPartition.PartitionOf = this;
            InventoryOf = master;
        }

        public void RefreshAllEffectivePartitions()
        {
            _allEffectivePartitions = InventoryPartition.ForceMergePartitions(new List<InventoryPartition> { StandardPartition, ActiveItemPartition }.Concat(HighlanderPartitions).ToList(), this);
            _allEffectiveItemCount = _allEffectivePartitions.GetEffectiveCount();
        }



        //MAKE THIS SUBSCRIBABLE
        //Add's item to its ItemBehavior holder and to the TotalEffectiveItems dictionary (To avoid using the slow Refresh method)
        public bool TryAddItemToInventory(Item item, HashSet<ItemModifier> modifiers = null, float count = 1f, HashSet<InventoryBehavior> inventoryBehaviors = null) { return TryAddItemToInventory(new InventoryItemStack(item, modifiers, count, inventoryBehaviors)); }
        public bool TryAddItemToInventory(InventoryItemStack inventoryItemStack)
        {
            bool ans = false;
            if (inventoryItemStack.NeedsConfirmation && ConfirmationPartition.TryAddToPartition(inventoryItemStack))
                ans = true;
            else if (inventoryItemStack.ItemModifiers.Contains(Highlander.instance) && HighlanderPartitions.Any(partition => partition.TryAddToPartition(inventoryItemStack)))
                ans= true;
            else if (inventoryItemStack.Item.Tier == TierA.instance && ActiveItemPartition.TryAddToPartition(inventoryItemStack))
                ans = true;
            else if (StandardPartition.TryAddToPartition(inventoryItemStack))
                ans = true;
            if(ans)
            {
                RefreshAllEffectivePartitions();
                ItemsAdded?.Invoke(this, inventoryItemStack);
                inventoryItemStack.Item.ItemCountChangeBehavior(this, inventoryItemStack, true);
                return true;
            }

            return false;

        }

        public bool TryRemoveItem(Item item, HashSet<ItemModifier> modifiers=null, float count = 1f, HashSet<InventoryBehavior> inventoryBehaviors= null) { return TryRemoveItem(new InventoryItemStack(item, modifiers, count, inventoryBehaviors)); }
        public bool TryRemoveItem(InventoryItemStack inventoryItemStack)
        {
            bool ans = false;
            if (inventoryItemStack.NeedsConfirmation && ConfirmationPartition.TryRemoveItem(inventoryItemStack))
                ans = true;
            else if (inventoryItemStack.ItemModifiers.Contains(Highlander.instance) && HighlanderPartitions.Any(partition => partition.TryRemoveItem(inventoryItemStack)))
                ans = true;
            else if (inventoryItemStack.Item.Tier == TierA.instance && ActiveItemPartition.TryRemoveItem(inventoryItemStack))
                ans = true;
            else if (StandardPartition.TryRemoveItem(inventoryItemStack))
                ans = true;
            if (ans)
            {
                RefreshAllEffectivePartitions();
                ItemsRemoved?.Invoke(this, inventoryItemStack);
                inventoryItemStack.Item.ItemCountChangeBehavior(this, inventoryItemStack,false);

                return true;
            }

            return false;

        }

        public Dictionary<Stat,ModifiableStat> RequestAdditionalModStats(Item item,params Stat[] requestedStats)
        {
            return null;
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

        public Inventory PartitionOf;
        public Dictionary<Item, List<InventoryItemStack>> ItemsInPartition;
        public ItemStackFilter AllowedInPartition;
        public float MaxCount;
        public float CurrentCount;
        public bool NeedsConfirmation;

        public InventoryPartition(Inventory inventory, ItemStackFilter allowedInPartition = null, float maxCount = float.MaxValue, bool needsConfirmation = false)
        {
            PartitionOf = inventory;
            ItemsInPartition = new Dictionary<Item, List<InventoryItemStack>>();
            AllowedInPartition = allowedInPartition;
            MaxCount = maxCount;
            CurrentCount = 0;
            NeedsConfirmation = false;
        }

        public Dictionary<Item,ItemEffectModifier> GetEffectiveCount()
        {
            Dictionary<Item, ItemEffectModifier> result=new Dictionary<Item,ItemEffectModifier>();
            foreach (var kvp in ItemsInPartition)
            {
                ItemEffectModifier temp = new();
                result.Add(kvp.Key, kvp.Value.Aggregate(new ItemEffectModifier { }, (total, current) => total + current.GetTotalEffectModifierSum().EquivalentModifier()));
            }
            return result;
        }

        public bool TryAddToPartition(Item item, HashSet<ItemModifier> itemModifiers = null, float count = 1, HashSet<InventoryBehavior> inventoryBehaviors = null) { return TryAddToPartition(new InventoryItemStack(item, itemModifiers, count, inventoryBehaviors)); }

        public bool TryAddToPartition(InventoryItemStack inventoryItemStack)
        {
            if (IsItemAllowed(inventoryItemStack))
            {
                GD.Print("INPART");
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

        public bool TryRemoveItem(Item item, HashSet<ItemModifier> itemModifiers = null, float count = 1, HashSet<InventoryBehavior> inventoryBehaviors = null) { return TryRemoveItem(new InventoryItemStack(item, itemModifiers, count, inventoryBehaviors)); }

        public bool TryRemoveItem(InventoryItemStack inventoryItemStack)
        {
            if (IsItemAllowed(inventoryItemStack))
            {
                GD.Print("INPART");
                if (!ItemsInPartition.ContainsKey(inventoryItemStack.Item))
                {
                    //ItemsInPartition.Add(inventoryItemStack.Item, new List<InventoryItemStack>() { inventoryItemStack });
                    return false;
                }

                foreach (InventoryItemStack iis in ItemsInPartition[inventoryItemStack.Item])
                {
                    float change = iis.Count;
                    if (iis.TryRemoveItem(inventoryItemStack))
                    {
                        change -=iis.Count;
                        if (iis.Count <= 0)
                        {
                            inventoryItemStack.Count = change;
                            ItemsInPartition[inventoryItemStack.Item].Remove(iis);

                        }
                        return true;
                    }
                        
                }

                //If you made it this far, there is no home for this item. So we have to make one for it
                //ItemsInPartition[inventoryItemStack.Item].Add(inventoryItemStack);
                return false;
            }
            return false;
        }

        public bool IsItemAllowed(InventoryItemStack itemStack)
        {
            return !IsFull() && IsItemApplicable(itemStack.Item);
        }

        public bool IsFull() { return (MaxCount != float.MaxValue && CurrentCount >= MaxCount); }
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
        public ItemEffectModifier GetTotalEffectModifierSum() => (ItemModifiers.Aggregate(ItemEffectModifier.StandardEffect, (sum, curr) => sum + curr.itemEffectModifier)+new ItemEffectModifier { Multiplier=Count});
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

        public bool TryRemoveItem(Item otherItem, HashSet<ItemModifier> otherItemModifiers, float otherCount = 1f, HashSet<InventoryBehavior> otherInventoryBehavior = null)
        {
            if (otherInventoryBehavior == null)
                otherInventoryBehavior = new HashSet<InventoryBehavior>() { PermanentItemBehavior.instance };

            if (!CanItemStack(otherItem, otherItemModifiers, otherInventoryBehavior))
                return false;

            Count -= otherCount;
            return true;
        }

        public bool TryRemoveItem(InventoryItemStack otherItemStack) { return TryRemoveItem(otherItemStack.Item, otherItemStack.ItemModifiers, otherItemStack.Count, otherItemStack.InventoryBehaviors); }
        public bool CanItemStack(Item otherItem, HashSet<ItemModifier> otherItemModifiers, HashSet<InventoryBehavior> otherInventoryBehavior) { return otherItem == Item && otherItemModifiers.SetEquals(ItemModifiers) && otherInventoryBehavior.SetEquals(InventoryBehaviors); }

        public bool CanItemStack(InventoryItemStack otherItemStack) { return CanItemStack(otherItemStack.Item, otherItemStack.ItemModifiers, otherItemStack.InventoryBehaviors); }
    }
}
