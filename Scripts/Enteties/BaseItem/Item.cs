using Godot;
using System;
using System.Collections.Generic;

namespace BrannPack
{
    public class Item
    {
        private static ItemTier Tier;
        private static List<ItemTierModifier> Modifiers;
        private static string Name;
        private static string CodeName;
        private static string Description;
        private static string AdvancedDescription;

    }

    public class TemporaryItem
    {
        private Item Item;
        private Dictionary<float, float> CountAndDuration;
    }
    public class ItemTier
    {
        private static string Name;
        private static string CodeName;
        private static List<Item> AllItemsInTier;
        private static List<Item> AllUnlockedItems;
    }
    public class ItemTierModifier
    {
        private static string Name;
        private static string CodeName;
        private static List<Item> AllItemsInTier;
        private static List<Item> AllUnlockedItems;
    }
}
