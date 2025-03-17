using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrannPack;
using BrannPack.ItemHandling;
using BrannPack.Tiers;

namespace BrannPack.ItemHandling
{
    public static class ItemCatalog
    {
        // Stores all predefined items (immutable)
        private static Item[] _allStaticItems;

        // Stores dynamically created items
        private static List<Item> _allDynamicItems = new(); 

        public static List<ItemPool> ItemPools;
        public static List<ItemSet> ItemSets;

        public static void Initialize(Item[] staticItems)
        {
            if (_allStaticItems != null) return; // Prevent re-initialization
            _allStaticItems = staticItems;

            // Assign each static item its index in the array
            for (int i = 0; i < _allStaticItems.Length; i++)
            {
                _allStaticItems[i].ItemIndex = i;
            }
        }

        public static int AddDynamicItem(Item newItem)
        {
            int index = _allStaticItems.Length + _allDynamicItems.Count;
            newItem.ItemIndex = index;
            _allDynamicItems.Add(newItem);
            return index;
        }

        public static Item GetItemByIndex(int index)
        {
            if (index < _allStaticItems.Length)
                return _allStaticItems[index]; // Fetch from static items

            int dynamicIndex = index - _allStaticItems.Length;
            if (dynamicIndex < _allDynamicItems.Count)
                return _allDynamicItems[dynamicIndex]; // Fetch from dynamic items

            return null; // Index out of range
        }

        public static IReadOnlyList<Item> GetAllItems()
        {
            return _allStaticItems.Concat(_allDynamicItems).ToList().AsReadOnly();
        }
    }

    public class ItemPool
    {
        public string Name;
        public Item[] Items;
        public float[] ItemWeights;
        public int ItemsInPool;
        public int TotalWeights;
        public int ItemPoolIndex;
    }

    public class ItemSet
    {
        public string Name;
        public string Description;
        public Item[] RequiredItems;
        public Item[] SustitutableItems;
        public int NumberOfSubstitutableItemsToGetReward;
        public Item Reward;
        public int ItemSetIndex;
    }

    public class ItemFilter
    {
        public HashSet<ItemTier> InAnyTiers;
        public HashSet<ItemTier> NotInAnyTiers;

        public HashSet<ItemSubTier> InAnySubTiers;
        public HashSet<ItemSubTier> NotInAnySubTiers;

        public HashSet<ItemModifier> HasAnyDefaultModifiers;
        public HashSet<ItemModifier> HasAllDefaultModifiers;
        public HashSet<ItemModifier> NotHaveAnyDefaultModifiers;
        public HashSet<ItemModifier> NotHaveAllDefaultModifiers;

        public HashSet<ItemModifier> HasAnyPossibleModifiers;
        public HashSet<ItemModifier> HasAllPossibleModifiers;
        public HashSet<ItemModifier> NotHaveAnyPossibleModifiers;
        public HashSet<ItemModifier> NotHaveAllPossibleModifiers;

        public HashSet<Item> Items;
        public HashSet<Item> NotItems;

        public bool? DefaultRequiresConfirmation;
        public bool? IsSharable;

        public (EffectTag tag, ValueCompare compareEnum, int value)[] HasAnyEffectTags;
        public (EffectTag tag, ValueCompare compareEnum, int value)[] HasAllEffectTags;
        public (EffectTag tag, ValueCompare compareEnum, int value)[] NotHaveAnyEffectTags;
        public (EffectTag tag, ValueCompare compareEnum, int value)[] NotHaveAllEffectTags;

        // Enum to decide the priority in case of overlap between whitelist and blacklist
        public enum FilterOverlapPriority
        {
            WhitelistPriority,  // Prioritize the whitelist
            BlacklistPriority   // Prioritize the blacklist
        }

        public enum ValueCompare
        {
            GreaterThan,
            LesserThan,
            EqualTo,
            NotEqualTo,
            AnyValue
        }

        public ItemFilter() { }
        public ItemFilter(HashSet<ItemTier> inAnyTiers, HashSet<ItemTier> notInAnyTiers, HashSet<ItemSubTier> inAnySubTiers, HashSet<ItemSubTier> notInAnySubTiers, HashSet<ItemModifier> hasAnyDefaultModifiers, HashSet<ItemModifier> hasAllDefaultModifiers, HashSet<ItemModifier> notHaveAnyDefaultModifiers, HashSet<ItemModifier> notHaveAllDefaultModifiers, HashSet<ItemModifier> hasAnyPossibleModifiers, HashSet<ItemModifier> hasAllPossibleModifiers, HashSet<ItemModifier> notHaveAnyPossibleModifiers, HashSet<ItemModifier> notHaveAllPossibleModifiers, HashSet<Item> items, HashSet<Item> notItems, bool? requiresConfirmation, bool? isSharable, (EffectTag tag, ValueCompare compareEnum, int value)[] hasAnyEffectTags, (EffectTag tag, ValueCompare compareEnum, int value)[] hasAllEffectTags, (EffectTag tag, ValueCompare compareEnum, int value)[] notHaveAnyEffectTags, (EffectTag tag, ValueCompare compareEnum, int value)[] notHaveAllEffectTags)
        {
            InAnyTiers = inAnyTiers;
            NotInAnyTiers = notInAnyTiers;
            InAnySubTiers = inAnySubTiers;
            NotInAnySubTiers = notInAnySubTiers;
            HasAnyDefaultModifiers = hasAnyDefaultModifiers;
            HasAllDefaultModifiers = hasAllDefaultModifiers;
            NotHaveAnyDefaultModifiers = notHaveAnyDefaultModifiers;
            NotHaveAllDefaultModifiers = notHaveAllDefaultModifiers;
            HasAnyPossibleModifiers = hasAnyPossibleModifiers;
            HasAllPossibleModifiers = hasAllPossibleModifiers;
            NotHaveAnyPossibleModifiers = notHaveAnyPossibleModifiers;
            NotHaveAllPossibleModifiers = notHaveAllPossibleModifiers;
            Items = items;
            NotItems = notItems;
            DefaultRequiresConfirmation = requiresConfirmation;
            IsSharable = isSharable;
            HasAnyEffectTags = hasAnyEffectTags;
            HasAllEffectTags = hasAllEffectTags;
            NotHaveAnyEffectTags = notHaveAnyEffectTags;
            NotHaveAllEffectTags = notHaveAllEffectTags;
        }
        public class ItemFilterBuilder
        {
            private HashSet<ItemTier> inAnyTiers = new();
            private HashSet<ItemTier> notInAnyTiers = new();
            private HashSet<ItemSubTier> inAnySubTiers = new();
            private HashSet<ItemSubTier> notInAnySubTiers = new();
            private HashSet<ItemModifier> hasAnyDefaultModifiers = new();
            private HashSet<ItemModifier> hasAllDefaultModifiers = new();
            private HashSet<ItemModifier> notHaveAnyDefaultModifiers = new();
            private HashSet<ItemModifier> notHaveAllDefaultModifiers = new();
            private HashSet<ItemModifier> hasAnyPossibleModifiers = new();
            private HashSet<ItemModifier> hasAllPossibleModifiers = new();
            private HashSet<ItemModifier> notHaveAnyPossibleModifiers = new();
            private HashSet<ItemModifier> notHaveAllPossibleModifiers = new();
            private HashSet<Item> items = new();
            private HashSet<Item> notItems = new();
            private bool? requiresConfirmation;
            private bool? isSharable;
            private (EffectTag tag, ValueCompare compareEnum, int value)[] hasAnyEffectTags = Array.Empty<(EffectTag, ValueCompare, int)>();
            private (EffectTag tag, ValueCompare compareEnum, int value)[] hasAllEffectTags = Array.Empty<(EffectTag, ValueCompare, int)>();
            private (EffectTag tag, ValueCompare compareEnum, int value)[] notHaveAnyEffectTags = Array.Empty<(EffectTag, ValueCompare, int)>();
            private (EffectTag tag, ValueCompare compareEnum, int value)[] notHaveAllEffectTags = Array.Empty<(EffectTag, ValueCompare, int)>();

            public ItemFilterBuilder WithAnyTiers(params ItemTier[] tiers)
            {
                inAnyTiers.UnionWith(tiers);
                return this;
            }

            public ItemFilterBuilder WithNotAnyTiers(params ItemTier[] tiers)
            {
                notInAnyTiers.UnionWith(tiers);
                return this;
            }

            public ItemFilterBuilder WithItems(params Item[] newItems)
            {
                items.UnionWith(newItems);
                return this;
            }

            public ItemFilterBuilder WithNotItems(params Item[] newItems)
            {
                notItems.UnionWith(newItems);
                return this;
            }

            public ItemFilterBuilder RequiresConfirmation(bool required)
            {
                requiresConfirmation = required;
                return this;
            }

            public ItemFilterBuilder IsSharable(bool sharable)
            {
                isSharable = sharable;
                return this;
            }


            public ItemFilterBuilder HasAnyEffectTags(params (EffectTag tag, ValueCompare compareEnum, int value)[] tags)
            {
                hasAnyEffectTags=tags;
                return this;
            }

            public ItemFilterBuilder HasAllEffectTags(params (EffectTag tag, ValueCompare compareEnum, int value)[] tags)
            {
                hasAllEffectTags = tags;
                return this;
            }

            public ItemFilterBuilder NotHaveAnyEffectTags(params (EffectTag tag, ValueCompare compareEnum, int value)[] tags)
            {
                notHaveAnyEffectTags = tags;
                return this;
            }

            public ItemFilterBuilder NotHaveAllEffectTags(params (EffectTag tag, ValueCompare compareEnum, int value)[] tags)
            {
                notHaveAllEffectTags = tags;
                return this;
            }

            public ItemFilterBuilder HasAnyDefaultModifiers(params ItemModifier[] modifiers)
            {
                hasAnyDefaultModifiers.UnionWith(modifiers);
                return this;
            }

            public ItemFilterBuilder HasAllDefaultModifiers(params ItemModifier[] modifiers)
            {
                hasAllDefaultModifiers.UnionWith(modifiers);
                return this;
            }

            public ItemFilterBuilder NotHaveAnyDefaultModifiers(params ItemModifier[] modifiers)
            {
                notHaveAnyDefaultModifiers.UnionWith(modifiers);
                return this;
            }

            public ItemFilterBuilder NotHaveAllDefaultModifiers(params ItemModifier[] modifiers)
            {
                notHaveAllDefaultModifiers.UnionWith(modifiers);
                return this;
            }

            public ItemFilterBuilder HasAnyPossibleModifiers(params ItemModifier[] modifiers)
            {
                hasAnyPossibleModifiers.UnionWith(modifiers);
                return this;
            }

            public ItemFilterBuilder HasAllPossibleModifiers(params ItemModifier[] modifiers)
            {
                hasAllPossibleModifiers.UnionWith(modifiers);
                return this;
            }

            public ItemFilterBuilder NotHaveAnyPossibleModifiers(params ItemModifier[] modifiers)
            {
                notHaveAnyPossibleModifiers.UnionWith(modifiers);
                return this;
            }

            public ItemFilterBuilder NotHaveAllPossibleModifiers(params ItemModifier[] modifiers)
            {
                notHaveAllPossibleModifiers.UnionWith(modifiers);
                return this;
            }

            public ItemFilter Build()
            {
                return new ItemFilter(inAnyTiers, notInAnyTiers, inAnySubTiers, notInAnySubTiers, hasAnyDefaultModifiers, hasAllDefaultModifiers, notHaveAnyDefaultModifiers, notHaveAllDefaultModifiers, hasAnyPossibleModifiers, hasAllPossibleModifiers, notHaveAnyPossibleModifiers, notHaveAllPossibleModifiers, items, notItems, requiresConfirmation, isSharable, hasAnyEffectTags, hasAllEffectTags, notHaveAnyEffectTags, notHaveAllEffectTags);
            }
        }

        public static Item[] GetAllApplicableStaticItems(ItemFilter filter, Item[] items)//Add some sort of variable to denote what to do if there is overlap. (Prioritize blacklist, prioritize whitelist)
        {
            List<Item> applicableItems = new List<Item>(items);
            applicableItems.RemoveAll(item => !filter.IsItemApplicable(item)); // Remove items that are not applicable
            return applicableItems.ToArray();
        }

        public bool IsItemApplicable(Item item, FilterOverlapPriority overlapPriority = FilterOverlapPriority.WhitelistPriority) { return IsItemApplicable(item, this, overlapPriority); }

        // Helper method to check if an item passes the whitelist conditions
        public static bool IsItemApplicable(Item item, ItemFilter filter, FilterOverlapPriority overlapPriority = FilterOverlapPriority.WhitelistPriority)
        {
            if (filter.DefaultRequiresConfirmation != null && filter.DefaultRequiresConfirmation != item.RequiresConfirmation)
                return false;

            if (filter.IsSharable != null && filter.IsSharable != item.IsSharable)
                return false;


            // Handle Items
            bool InWLItem = (filter.Items == null || filter.Items.Contains(item));
            bool InBLItem = (filter.NotItems != null && filter.NotItems.Contains(item));
            if (InWLItem && InBLItem)
            {
                if (overlapPriority == FilterOverlapPriority.WhitelistPriority)
                {
                    InBLItem = false; // If whitelist takes priority, blacklist is ignored
                }
                else
                {
                    InWLItem = false; // If blacklist takes priority, whitelist is ignored
                }
            }
            if (!InWLItem || InBLItem)
                return false;

            // Handle Tiers
            bool InWLTier = (filter.InAnyTiers == null || filter.InAnyTiers.Contains(item.Tier));
            bool InBLTier = (filter.NotInAnyTiers != null && filter.NotInAnyTiers.Contains(item.Tier));
            if (InWLTier && InBLTier)
            {
                if (overlapPriority == FilterOverlapPriority.WhitelistPriority)
                {
                    InBLTier = false; // If whitelist takes priority, blacklist is ignored
                }
                else
                {
                    InWLTier = false; // If blacklist takes priority, whitelist is ignored
                }
            }
            if (!InWLTier || InBLTier)
                return false;

            // Handle SubTiers
            bool InWLSubTier = (filter.InAnySubTiers == null || filter.InAnySubTiers.Contains(item.SubTier));
            bool InBLSubTier = (filter.NotInAnySubTiers != null && filter.NotInAnySubTiers.Contains(item.SubTier));
            if (InWLSubTier && InBLSubTier)
            {
                if (overlapPriority == FilterOverlapPriority.WhitelistPriority)
                {
                    InBLSubTier = false; // If whitelist takes priority, blacklist is ignored
                }
                else
                {
                    InWLSubTier = false; // If blacklist takes priority, whitelist is ignored
                }
            }
            if (!InWLSubTier || InBLSubTier)
                return false;

            // Handle AnyModifiers
            HashSet<ItemModifier> tempHasAnyDMod = filter.HasAnyDefaultModifiers;
            HashSet<ItemModifier> tempNotHaveAnyDMod = filter.NotHaveAnyDefaultModifiers;

            if (tempHasAnyDMod.Overlaps(tempNotHaveAnyDMod))
            {
                if (overlapPriority == FilterOverlapPriority.WhitelistPriority)
                {
                    tempNotHaveAnyDMod.ExceptWith(tempHasAnyDMod);// If whitelist takes priority, blacklist is ignored
                }
                else
                {
                    tempHasAnyDMod.ExceptWith(tempNotHaveAnyDMod); // If blacklist takes priority, whitelist is ignored
                }
            }



            bool HasAnyDefaultModifiers = (tempHasAnyDMod == null || tempHasAnyDMod.Intersect(item.DefaultModifiers).Any());
            bool NotHaveAnyDefaultModifiers = (tempNotHaveAnyDMod != null && tempNotHaveAnyDMod.Intersect(item.DefaultModifiers).Any());

            if (!HasAnyDefaultModifiers || NotHaveAnyDefaultModifiers)
                return false;

            // Handle AllModifiers
            bool HasAllDefaultModifiers = (filter.HasAllDefaultModifiers != null && filter.HasAllDefaultModifiers.All(mod => item.DefaultModifiers.Contains(mod)));
            bool NotHaveAllDefaultModifiers = (filter.NotHaveAllDefaultModifiers != null && filter.NotHaveAllDefaultModifiers.All(mod => item.DefaultModifiers.Contains(mod)));

            if (!HasAllDefaultModifiers || NotHaveAllDefaultModifiers)
                return false;

            // Handle Any Possible Modifiers (Not yet fixed up)

            HashSet<ItemModifier> tempHasAnyPMod = filter.HasAnyPossibleModifiers;
            HashSet<ItemModifier> tempNotHaveAnyPMod = filter.NotHaveAnyPossibleModifiers;

            if (tempHasAnyPMod.Overlaps(tempNotHaveAnyPMod))
            {
                if (overlapPriority == FilterOverlapPriority.WhitelistPriority)
                {
                    tempNotHaveAnyPMod.ExceptWith(tempHasAnyPMod);// If whitelist takes priority, blacklist is ignored
                }
                else
                {
                    tempHasAnyPMod.ExceptWith(tempNotHaveAnyPMod); // If blacklist takes priority, whitelist is ignored
                }
            }


            bool HasAnyPossibleModifiers = (tempHasAnyPMod == null || tempHasAnyPMod.Intersect(item.PossibleModifiers).Any());
            bool NotHaveAnyPossibleModifiers = (tempNotHaveAnyPMod != null && tempNotHaveAnyPMod.Intersect(item.PossibleModifiers).Any());

            if (!HasAnyPossibleModifiers || NotHaveAnyPossibleModifiers)
                return false;

            // Handle All Possible Modifiers

            bool HasAllPossibleModifiers = (filter.HasAllPossibleModifiers != null && filter.HasAllPossibleModifiers.All(mod => item.PossibleModifiers.Contains(mod)));
            bool NotHaveAllPossibleModifiers = (filter.NotHaveAllPossibleModifiers != null && filter.NotHaveAllPossibleModifiers.All(mod => item.PossibleModifiers.Contains(mod)));

            if (!HasAllPossibleModifiers || NotHaveAllPossibleModifiers)
                return false;

            // Handle Effect Tags (Whitelisted and Blacklisted tags)
            bool HasAnyEffectTags = (filter.HasAnyEffectTags == null || filter.HasAnyEffectTags.Any(tag => item.EffectWeight.ContainsKey(tag.tag) && CompareEffectWeight(tag.tag, item.EffectWeight[tag.tag], tag.compareEnum, tag.value)));
            bool NotHaveAnyEffectTags = (filter.NotHaveAnyEffectTags != null && filter.NotHaveAnyEffectTags.Any(tag => item.EffectWeight.ContainsKey(tag.tag) && CompareEffectWeight(tag.tag, item.EffectWeight[tag.tag], tag.compareEnum, tag.value)));

            if (!HasAnyEffectTags || NotHaveAnyEffectTags)
                return false;

            bool HasAllEffectTags = (filter.HasAllEffectTags != null && filter.HasAllEffectTags.All(tag => item.EffectWeight.ContainsKey(tag.tag) && CompareEffectWeight(tag.tag, item.EffectWeight[tag.tag], tag.compareEnum, tag.value)));
            bool NotHaveAllEffectTags = (filter.NotHaveAllEffectTags != null && filter.NotHaveAllEffectTags.All(tag => item.EffectWeight.ContainsKey(tag.tag) && CompareEffectWeight(tag.tag, item.EffectWeight[tag.tag], tag.compareEnum, tag.value)));

            if (!HasAllEffectTags || NotHaveAllEffectTags)
                return false;

            // Evaluate whether the item is applicable based on all the filter criteria
            return true;
        }

        // Method to compare effect tag weights based on the specified comparison type
        private static bool CompareEffectWeight(EffectTag tag, float effectWeight, ValueCompare comparison, int compareValue)
        {
            switch (comparison)
            {
                case ValueCompare.GreaterThan:
                    return effectWeight > compareValue;
                case ValueCompare.LesserThan:
                    return effectWeight < compareValue;
                case ValueCompare.EqualTo:
                    return effectWeight == compareValue;
                case ValueCompare.AnyValue:
                    return true;
                case ValueCompare.NotEqualTo:
                    return effectWeight != compareValue;
                default:
                    return false;
            }
        }

    }

    public class ItemStackFilter:ItemFilter
    {
        public HashSet<ItemModifier> IsAnyModifiers;
        public HashSet<ItemModifier> IsAllModifiers;
        public HashSet<ItemModifier> IsNotAnyModifiers;
        public HashSet<ItemModifier> IsNotAllModifiers;

        public HashSet<InventoryBehavior> IsAnyBehaviors;
        public HashSet<InventoryBehavior> IsAllBehaviors;
        public HashSet<InventoryBehavior> IsNotAnyBehaviors;
        public HashSet<InventoryBehavior> IsNotAllBehaviors;

        public bool? RequiresConfirmation;

        public (ValueCompare,float)[] countCompare;

        public ItemStackFilter(HashSet<ItemModifier> isAnyModifiers, HashSet<ItemModifier> isAllModifiers, HashSet<ItemModifier> isNotAnyModifiers, HashSet<ItemModifier> isNotAllModifiers, HashSet<InventoryBehavior> isAnyBehaviors, HashSet<InventoryBehavior> isAllBehaviors, HashSet<InventoryBehavior> isNotAnyBehaviors, HashSet<InventoryBehavior> isNotAllBehaviors, bool? requiresConfirmation, (ValueCompare, float)[] count, HashSet<ItemTier> inAnyTiers, HashSet<ItemTier> notInAnyTiers, HashSet<ItemSubTier> inAnySubTiers, HashSet<ItemSubTier> notInAnySubTiers, HashSet<ItemModifier> hasAnyDefaultModifiers, HashSet<ItemModifier> hasAllDefaultModifiers, HashSet<ItemModifier> notHaveAnyDefaultModifiers, HashSet<ItemModifier> notHaveAllDefaultModifiers, HashSet<ItemModifier> hasAnyPossibleModifiers, HashSet<ItemModifier> hasAllPossibleModifiers, HashSet<ItemModifier> notHaveAnyPossibleModifiers, HashSet<ItemModifier> notHaveAllPossibleModifiers, HashSet<Item> items, HashSet<Item> notItems, bool? requiresDefaultConfirmation, bool? isSharable, (EffectTag tag, ValueCompare compareEnum, int value)[] hasAnyEffectTags, (EffectTag tag, ValueCompare compareEnum, int value)[] hasAllEffectTags, (EffectTag tag, ValueCompare compareEnum, int value)[] notHaveAnyEffectTags, (EffectTag tag, ValueCompare compareEnum, int value)[] notHaveAllEffectTags):base(inAnyTiers,  notInAnyTiers,  inAnySubTiers, notInAnySubTiers, hasAnyDefaultModifiers,hasAllDefaultModifiers,  notHaveAnyDefaultModifiers, notHaveAllDefaultModifiers, hasAnyPossibleModifiers, hasAllPossibleModifiers, notHaveAnyPossibleModifiers, notHaveAllPossibleModifiers, items, notItems, requiresDefaultConfirmation, isSharable, hasAnyEffectTags, hasAllEffectTags, notHaveAnyEffectTags, notHaveAllEffectTags)
        {
            IsAnyModifiers = isAnyModifiers;
            IsAllModifiers = isAllModifiers;
            IsNotAnyModifiers = isNotAnyModifiers;
            IsNotAllModifiers = isNotAllModifiers;
            IsAnyBehaviors = isAnyBehaviors;
            IsAllBehaviors = isAllBehaviors;
            IsNotAnyBehaviors = isNotAnyBehaviors;
            IsNotAllBehaviors = isNotAllBehaviors;
            RequiresConfirmation = requiresConfirmation;
            this.count = count;
        }

        public bool IsItemStackApplicable(InventoryItemStack itemStack, FilterOverlapPriority overlapPriority = FilterOverlapPriority.WhitelistPriority) { return IsItemStackApplicable(itemStack, this, overlapPriority); }

        public static bool IsItemStackApplicable(InventoryItemStack itemStack, ItemStackFilter filter, FilterOverlapPriority overlapPriority = FilterOverlapPriority.WhitelistPriority)
        {
            if (filter.RequiresConfirmation != null && filter.RequiresConfirmation != itemStack.NeedsConfirmation)
                return false;

            // Handle Modifiers (Not yet fixed up)

            HashSet<ItemModifier> tempIsAnyMod = filter.IsAnyModifiers;
            HashSet<ItemModifier> tempIsNotAnyMod = filter.IsNotAnyModifiers;

            if (tempIsAnyMod.Overlaps(tempIsNotAnyMod))
            {
                if (overlapPriority == FilterOverlapPriority.WhitelistPriority)
                {
                    tempIsNotAnyMod.ExceptWith(tempIsAnyMod);// If whitelist takes priority, blacklist is ignored
                }
                else
                {
                    tempIsAnyMod.ExceptWith(tempIsNotAnyMod); // If blacklist takes priority, whitelist is ignored
                }
            }


            bool IsAnyModifiers = (tempIsAnyMod == null || tempIsAnyMod.Intersect(itemStack.ItemModifiers).Any());
            bool IsNotAnyModifiers = (tempIsNotAnyMod != null && tempIsAnyMod.Intersect(itemStack.ItemModifiers).Any());

            if (!IsAnyModifiers || IsNotAnyModifiers)
                return false;

            // Handle All Modifiers

            bool IsAllModifiers = (filter.IsAllModifiers != null && filter.IsAllModifiers.All(mod => itemStack.ItemModifiers.Contains(mod)));
            bool IsNotAllModifiers = (filter.IsNotAllModifiers != null && filter.IsNotAllModifiers.All(mod => itemStack.ItemModifiers.Contains(mod)));

            if (!IsAllModifiers || IsNotAllModifiers)
                return false;

            // Handle Behaviors (Not yet fixed up)

            HashSet<InventoryBehavior> tempIsAnyBeh = filter.IsAnyBehaviors;
            HashSet<InventoryBehavior> tempIsNotAnyBeh = filter.IsNotAnyBehaviors;

            if (tempIsAnyBeh.Overlaps(tempIsNotAnyBeh))
            {
                if (overlapPriority == FilterOverlapPriority.WhitelistPriority)
                {
                    tempIsNotAnyBeh.ExceptWith(tempIsAnyBeh);// If whitelist takes priority, blacklist is ignored
                }
                else
                {
                    tempIsAnyBeh.ExceptWith(tempIsNotAnyBeh); // If blacklist takes priority, whitelist is ignored
                }
            }


            bool IsAnyBehaviors = (tempIsAnyBeh == null || tempIsAnyBeh.Intersect(itemStack.InventoryBehaviors).Any());
            bool IsNotAnyBehaviors = (tempIsNotAnyBeh != null && tempIsAnyBeh.Intersect(itemStack.InventoryBehaviors).Any());

            if (!IsAnyBehaviors || IsNotAnyBehaviors)
                return false;

            // Handle All Behaviors

            bool IsAllBehaviors = (filter.IsAllBehaviors != null && filter.IsAllBehaviors.All(Beh => itemStack.InventoryBehaviors.Contains(Beh)));
            bool IsNotAllBehaviors = (filter.IsNotAllBehaviors != null && filter.IsNotAllBehaviors.All(Beh => itemStack.InventoryBehaviors.Contains(Beh)));

            if (!IsAllBehaviors || IsNotAllBehaviors)
                return false;

            // Handle Count

            bool AllCountCompare = (filter.countCompare != null && filter.countCompare.All(tag => CompareCount(itemStack.Count, tag.Item1, tag.Item2)));
            if (!AllCountCompare)
                return false;


            return ItemFilter.IsItemApplicable(itemStack.Item, filter as ItemFilter, overlapPriority);
        }

        private static bool CompareCount(float count, ValueCompare comparison, float compareCount)
        {
            switch (comparison)
            {
                case ValueCompare.GreaterThan:
                    return count > compareCount;
                case ValueCompare.LesserThan:
                    return count < compareCount;
                case ValueCompare.EqualTo:
                    return count == compareCount;
                case ValueCompare.AnyValue:
                    return true;
                case ValueCompare.NotEqualTo:
                    return count != compareCount;
                default:
                    return false;
            }
        }

        public class ItemStackFilterBuilder
        {
            private HashSet<ItemTier> inAnyTiers = new();
            private HashSet<ItemTier> notInAnyTiers = new();
            private HashSet<ItemSubTier> inAnySubTiers = new();
            private HashSet<ItemSubTier> notInAnySubTiers = new();
            private HashSet<ItemModifier> hasAnyDefaultModifiers = new();
            private HashSet<ItemModifier> hasAllDefaultModifiers = new();
            private HashSet<ItemModifier> notHaveAnyDefaultModifiers = new();
            private HashSet<ItemModifier> notHaveAllDefaultModifiers = new();
            private HashSet<ItemModifier> hasAnyPossibleModifiers = new();
            private HashSet<ItemModifier> hasAllPossibleModifiers = new();
            private HashSet<ItemModifier> notHaveAnyPossibleModifiers = new();
            private HashSet<ItemModifier> notHaveAllPossibleModifiers = new();
            private HashSet<Item> items = new();
            private HashSet<Item> notItems = new();
            private bool? requiresDefaultConfirmation;
            private bool? isSharable;
            private (EffectTag tag, ValueCompare compareEnum, int value)[] hasAnyEffectTags = Array.Empty<(EffectTag, ValueCompare, int)>();
            private (EffectTag tag, ValueCompare compareEnum, int value)[] hasAllEffectTags = Array.Empty<(EffectTag, ValueCompare, int)>();
            private (EffectTag tag, ValueCompare compareEnum, int value)[] notHaveAnyEffectTags = Array.Empty<(EffectTag, ValueCompare, int)>();
            private (EffectTag tag, ValueCompare compareEnum, int value)[] notHaveAllEffectTags = Array.Empty<(EffectTag, ValueCompare, int)>();
            private HashSet<ItemModifier> isAnyModifiers = new();
            private HashSet<ItemModifier> isAllModifiers = new();
            private HashSet<ItemModifier> isNotAnyModifiers = new();
            private HashSet<ItemModifier> isNotAllModifiers = new();
            private HashSet<InventoryBehavior> isAnyBehaviors = new();
            private HashSet<InventoryBehavior> isAllBehaviors = new();
            private HashSet<InventoryBehavior> isNotAnyBehaviors = new();
            private HashSet<InventoryBehavior> isNotAllBehaviors = new();
            private bool? requiresConfirmation;
            private (ValueCompare, float)[] countCompare = Array.Empty<(ValueCompare, float)>();

            public ItemStackFilterBuilder WithAnyTiers(params ItemTier[] tiers)
            {
                inAnyTiers.UnionWith(tiers);
                return this;
            }

            public ItemStackFilterBuilder WithNotAnyTiers(params ItemTier[] tiers)
            {
                notInAnyTiers.UnionWith(tiers);
                return this;
            }

            public ItemStackFilterBuilder WithItems(params Item[] newItems)
            {
                items.UnionWith(newItems);
                return this;
            }

            public ItemStackFilterBuilder WithNotItems(params Item[] newItems)
            {
                notItems.UnionWith(newItems);
                return this;
            }

            public ItemStackFilterBuilder RequiresDefaultConfirmation(bool required)
            {
                requiresDefaultConfirmation = required;
                return this;
            }

            public ItemStackFilterBuilder IsSharable(bool sharable)
            {
                isSharable = sharable;
                return this;
            }


            public ItemStackFilterBuilder HasAnyEffectTags(params (EffectTag tag, ValueCompare compareEnum, int value)[] tags)
            {
                hasAnyEffectTags = tags;
                return this;
            }

            public ItemStackFilterBuilder HasAllEffectTags(params (EffectTag tag, ValueCompare compareEnum, int value)[] tags)
            {
                hasAllEffectTags = tags;
                return this;
            }

            public ItemStackFilterBuilder NotHaveAnyEffectTags(params (EffectTag tag, ValueCompare compareEnum, int value)[] tags)
            {
                notHaveAnyEffectTags = tags;
                return this;
            }

            public ItemStackFilterBuilder NotHaveAllEffectTags(params (EffectTag tag, ValueCompare compareEnum, int value)[] tags)
            {
                notHaveAllEffectTags = tags;
                return this;
            }

            public ItemStackFilterBuilder HasAnyDefaultModifiers(params ItemModifier[] modifiers)
            {
                hasAnyDefaultModifiers.UnionWith(modifiers);
                return this;
            }

            public ItemStackFilterBuilder HasAllDefaultModifiers(params ItemModifier[] modifiers)
            {
                hasAllDefaultModifiers.UnionWith(modifiers);
                return this;
            }

            public ItemStackFilterBuilder NotHaveAnyDefaultModifiers(params ItemModifier[] modifiers)
            {
                notHaveAnyDefaultModifiers.UnionWith(modifiers);
                return this;
            }

            public ItemStackFilterBuilder NotHaveAllDefaultModifiers(params ItemModifier[] modifiers)
            {
                notHaveAllDefaultModifiers.UnionWith(modifiers);
                return this;
            }

            public ItemStackFilterBuilder HasAnyPossibleModifiers(params ItemModifier[] modifiers)
            {
                hasAnyPossibleModifiers.UnionWith(modifiers);
                return this;
            }

            public ItemStackFilterBuilder HasAllPossibleModifiers(params ItemModifier[] modifiers)
            {
                hasAllPossibleModifiers.UnionWith(modifiers);
                return this;
            }

            public ItemStackFilterBuilder NotHaveAnyPossibleModifiers(params ItemModifier[] modifiers)
            {
                notHaveAnyPossibleModifiers.UnionWith(modifiers);
                return this;
            }

            public ItemStackFilterBuilder NotHaveAllPossibleModifiers(params ItemModifier[] modifiers)
            {
                notHaveAllPossibleModifiers.UnionWith(modifiers);
                return this;
            }

            public ItemStackFilterBuilder IsAnyModifiers(params ItemModifier[] modifiers)
            {
                isAnyModifiers.UnionWith(modifiers);
                return this;
            }

            public ItemStackFilterBuilder IsAllModifiers(params ItemModifier[] modifiers)
            {
                isAllModifiers.UnionWith(modifiers);
                return this;
            }

            public ItemStackFilterBuilder IsNotAnyModifiers(params ItemModifier[] modifiers)
            {
                isNotAnyModifiers.UnionWith(modifiers);
                return this;
            }

            public ItemStackFilterBuilder IsNotAllModifiers(params ItemModifier[] modifiers)
            {
                isNotAllModifiers.UnionWith(modifiers);
                return this;
            }

            public ItemStackFilterBuilder WithAnyBehaviors(params InventoryBehavior[] behaviors)
            {
                isAnyBehaviors.UnionWith(behaviors);
                return this;
            }

            public ItemStackFilterBuilder WithAllBehaviors(params InventoryBehavior[] behaviors)
            {
                isAllBehaviors.UnionWith(behaviors);
                return this;
            }

            public ItemStackFilterBuilder WithoutAnyBehaviors(params InventoryBehavior[] behaviors)
            {
                isNotAnyBehaviors.UnionWith(behaviors);
                return this;
            }

            public ItemStackFilterBuilder WithoutAllBehaviors(params InventoryBehavior[] behaviors)
            {
                isNotAllBehaviors.UnionWith(behaviors);
                return this;
            }

            public ItemStackFilterBuilder WithCountCompare(params (ValueCompare, float)[] compares)
            {
                countCompare = compares;
                return this;
            }

            public ItemStackFilter Build()
            {
                return new ItemStackFilter(isAnyModifiers,isAllModifiers,isNotAnyModifiers,isNotAllModifiers,isAnyBehaviors,isAllBehaviors,isNotAnyBehaviors,isNotAllBehaviors,requiresConfirmation,countCompare,inAnyTiers, notInAnyTiers, inAnySubTiers, notInAnySubTiers, hasAnyDefaultModifiers, hasAllDefaultModifiers, notHaveAnyDefaultModifiers, notHaveAllDefaultModifiers, hasAnyPossibleModifiers, hasAllPossibleModifiers, notHaveAnyPossibleModifiers, notHaveAllPossibleModifiers, items, notItems, requiresConfirmation, isSharable, hasAnyEffectTags, hasAllEffectTags, notHaveAnyEffectTags, notHaveAllEffectTags);
            }
        }
    }
}
