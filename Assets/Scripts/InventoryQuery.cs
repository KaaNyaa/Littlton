using System;
using System.Collections.Generic;
using System.Linq;

public enum InventorySortOrder
{
    AlphabeticalAscending,
    AlphabeticalDescending
}

public static class InventoryQuery
{
    public static List<InventoryItem> SortAlphabetically(
        IEnumerable<InventoryItem> items,
        InventorySortOrder sortOrder = InventorySortOrder.AlphabeticalAscending)
    {
        if (items == null)
        {
            return new List<InventoryItem>();
        }

        IEnumerable<InventoryItem> validItems = items.Where(i => i != null);

        if (sortOrder == InventorySortOrder.AlphabeticalDescending)
        {
            return validItems
                .OrderByDescending(i => i.ItemName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        return validItems
            .OrderBy(i => i.ItemName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static bool MatchesSearch(InventoryItem item, string searchQuery)
    {
        if (item == null || string.IsNullOrEmpty(item.ItemName))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(searchQuery))
        {
            return false;
        }

        string normalizedQuery = searchQuery.Trim();
        return item.ItemName.IndexOf(normalizedQuery, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static HashSet<int> GetMatchingSlotIndices(
        IList<InventoryItem> orderedItems,
        string searchQuery)
    {
        HashSet<int> matchingIndices = new HashSet<int>();

        if (orderedItems == null || string.IsNullOrWhiteSpace(searchQuery))
        {
            return matchingIndices;
        }

        for (int i = 0; i < orderedItems.Count; i++)
        {
            if (MatchesSearch(orderedItems[i], searchQuery))
            {
                matchingIndices.Add(i);
            }
        }

        return matchingIndices;
    }
}