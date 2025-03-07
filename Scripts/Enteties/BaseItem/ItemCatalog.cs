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

        public class ItemFilter
        {
            public HashSet<ItemTier> InAnyTiers;
            public HashSet<ItemTier> NotInAnyTiers;

            public HashSet<ItemSubTier> InAnySubTiers;
            public HashSet<ItemSubTier> NotInAnySubTiers;

            public HashSet<ItemTierModifier> HasAnyDefaultModifiers;
            public HashSet<ItemTierModifier> HasAllDefaultModifiers;
            public HashSet<ItemTierModifier> NotHaveAnyDefaultModifiers;
            public HashSet<ItemTierModifier> NotHaveAllDefaultModifiers;

            public HashSet<ItemTierModifier> HasAnyPossibleModifiers;
            public HashSet<ItemTierModifier> HasAllPossibleModifiers;
            public HashSet<ItemTierModifier> NotHaveAnyPossibleModifiers;
            public HashSet<ItemTierModifier> NotHaveAllPossibleModifiers;

            public HashSet<Item> Items;
            public HashSet<Item> NotItems;

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



            public static Item[] GetApplicableItems(ItemFilter filter)//Add some sort of variable to denote what to do if there is overlap. (Prioritize blacklist, prioritize whitelist)
            {
                //Handle overlap in white and blacklists
                // use filter.items as the pool of items to filter
                return null;
            }

            // Helper method to check if an item passes the whitelist conditions
            private static bool IsItemApplicable(Item item, ItemFilter filter, FilterOverlapPriority overlapPriority = FilterOverlapPriority.WhitelistPriority)
            {
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
                HashSet<ItemTierModifier> tempHasAnyDMod = filter.HasAnyDefaultModifiers;
                HashSet<ItemTierModifier> tempNotHaveAnyDMod = filter.NotHaveAnyDefaultModifiers;

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

                HashSet<ItemTierModifier> tempHasAnyDMod = filter.HasAnyDefaultModifiers;
                HashSet<ItemTierModifier> tempNotHaveAnyDMod = filter.NotHaveAnyDefaultModifiers;

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


                bool HasAnyPossibleModifiers = (filter.HasAnyPossibleModifiers == null || filter.HasAnyPossibleModifiers.Intersect(item.PossibleModifiers).Any());
                bool NotHaveAnyPossibleModifiers = (filter.NotHaveAnyPossibleModifiers != null && filter.NotHaveAnyPossibleModifiers.Intersect(item.PossibleModifiers).Any());

                // Handle All Possible Modifiers

                bool HasAllPossibleModifiers = (filter.HasAllPossibleModifiers != null && filter.HasAllPossibleModifiers.All(mod => item.PossibleModifiers.Contains(mod)));
                bool NotHaveAllPossibleModifiers = (filter.NotHaveAllPossibleModifiers != null && filter.NotHaveAllPossibleModifiers.All(mod => item.PossibleModifiers.Contains(mod)));

                

                // Handle Effect Tags (Whitelisted and Blacklisted tags)
                bool HasAnyEffectTags = (filter.HasAnyEffectTags == null || filter.HasAnyEffectTags.Any(tag => item.EffectWeights.ContainsKey(tag.tag) && CompareEffectWeight(tag.tag, item.EffectWeights[tag.tag], tag.compareEnum, tag.value)));
                bool HasAllEffectTags = (filter.HasAllEffectTags != null && filter.HasAllEffectTags.All(tag => item.EffectWeights.ContainsKey(tag.tag) && CompareEffectWeight(tag.tag, item.EffectWeights[tag.tag], tag.compareEnum, tag.value)));
                bool NotHaveAnyEffectTags = (filter.NotHaveAnyEffectTags != null && filter.NotHaveAnyEffectTags.Any(tag => item.EffectWeights.ContainsKey(tag.tag) && CompareEffectWeight(tag.tag, item.EffectWeights[tag.tag], tag.compareEnum, tag.value)));
                bool NotHaveAllEffectTags = (filter.NotHaveAllEffectTags != null && filter.NotHaveAllEffectTags.All(tag => item.EffectWeights.ContainsKey(tag.tag) && CompareEffectWeight(tag.tag, item.EffectWeights[tag.tag], tag.compareEnum, tag.value)));

                // Evaluate whether the item is applicable based on all the filter criteria
                return (InWLTier && !InBLTier && InWLSubTier && !InBLSubTier &&
                        HasAnyDefaultModifiers && HasAllDefaultModifiers && NotHaveAnyDefaultModifiers && NotHaveAllDefaultModifiers &&
                        HasAnyPossibleModifiers && HasAllPossibleModifiers && NotHaveAnyPossibleModifiers && NotHaveAllPossibleModifiers &&
                        InWLItem && !InBLItem && HasAnyEffectTags && HasAllEffectTags && NotHaveAnyEffectTags && NotHaveAllEffectTags);
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

        public class ItemPool
        {
            public string Name;
            public Item[] Items;
            public float[] ItemWeights;
            public int ItemsInPool;
            public int TotalWeights;
            public int ItemPoolIndex;
        }
    }
}
