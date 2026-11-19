using System.Collections.Generic;
using NUnit.Framework;

public class InventorySearchAndSortTests
{
    private List<InventoryItem> CreateSampleItems()
    {
        return new List<InventoryItem>
        {
            new InventoryItem { ItemName = "Stone", Quantity = 2, Category = "Resource" },
            new InventoryItem { ItemName = "Wood", Quantity = 4, Category = "Resource" },
            new InventoryItem { ItemName = "Iron Ore", Quantity = 7, Category = "Resource" }
        };
    }

    [Test]
    public void MatchesSearch_UppercaseQuery_MatchesCaseInsensitively()
    {
        InventoryItem wood = new InventoryItem { ItemName = "Wood", Quantity = 4 };

        bool matched = InventoryQuery.MatchesSearch(wood, "WOOD");

        Assert.IsTrue(matched);
    }

    [Test]
    public void MatchesSearch_MixedCasePartialQuery_MatchesCaseInsensitively()
    {
        InventoryItem stone = new InventoryItem { ItemName = "Stone", Quantity = 2 };

        bool matched = InventoryQuery.MatchesSearch(stone, "oNe");

        Assert.IsTrue(matched);
    }

    [Test]
    public void MatchesSearch_EmptyOrWhitespaceInput_ReturnsFalse()
    {
        InventoryItem wood = new InventoryItem { ItemName = "Wood", Quantity = 4 };

        Assert.IsFalse(InventoryQuery.MatchesSearch(wood, ""));
        Assert.IsFalse(InventoryQuery.MatchesSearch(wood, "   "));
    }

    [Test]
    public void SortAlphabetically_Ascending_OrdersAToZCaseInsensitively()
    {
        List<InventoryItem> items = CreateSampleItems();

        List<InventoryItem> sorted = InventoryQuery.SortAlphabetically(
            items,
            InventorySortOrder.AlphabeticalAscending);

        Assert.AreEqual("Iron Ore", sorted[0].ItemName);
        Assert.AreEqual("Stone", sorted[1].ItemName);
        Assert.AreEqual("Wood", sorted[2].ItemName);
    }

    [Test]
    public void SortAlphabetically_Descending_OrdersZToACaseInsensitively()
    {
        List<InventoryItem> items = CreateSampleItems();

        List<InventoryItem> sorted = InventoryQuery.SortAlphabetically(
            items,
            InventorySortOrder.AlphabeticalDescending);

        Assert.AreEqual("Wood", sorted[0].ItemName);
        Assert.AreEqual("Stone", sorted[1].ItemName);
        Assert.AreEqual("Iron Ore", sorted[2].ItemName);
    }

    [Test]
    public void GetMatchingSlotIndices_CaseInsensitiveMatch_ReturnsExpectedIndices()
    {
        List<InventoryItem> sortedAscending = InventoryQuery.SortAlphabetically(
            CreateSampleItems(),
            InventorySortOrder.AlphabeticalAscending);

        HashSet<int> indices = InventoryQuery.GetMatchingSlotIndices(sortedAscending, "sTo");

        Assert.IsTrue(indices.Contains(1));
        Assert.AreEqual(1, indices.Count);
    }
}