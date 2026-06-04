using System.Collections.Generic;
using System.Linq;
using CartCatalog.Core;
using Xunit;

namespace CartCatalog.Tests;

public class OrderMergeTests
{
    private static PendingOrder Order(string id, int qty, int day = 0)
        => new PendingOrder { ItemId = id, Quantity = qty, DeliveryDay = day };

    [Fact]
    public void Merge_sums_same_item_orders_into_one_stack()
    {
        var merged = OrderMerge.Merge(new[] { Order("(O)739", 1), Order("(O)739", 4) });

        Assert.Single(merged);
        Assert.Equal("(O)739", merged[0].ItemId);
        Assert.Equal(5, merged[0].Quantity);
    }

    [Fact]
    public void Merge_preserves_first_seen_order_when_merging()
    {
        var merged = OrderMerge.Merge(new[] { Order("(O)739", 1), Order("(O)128", 2), Order("(O)739", 3) });

        Assert.Equal(new[] { "(O)739", "(O)128" }, merged.Select(o => o.ItemId));
        Assert.Equal(4, merged[0].Quantity);   // (O)739 summed
        Assert.Equal(2, merged[1].Quantity);
    }

    [Fact]
    public void Merge_keeps_distinct_items_separate()
    {
        var merged = OrderMerge.Merge(new[] { Order("(O)128", 1), Order("(F)1466", 2) });

        Assert.Equal(2, merged.Count);
        Assert.Equal(new[] { "(O)128", "(F)1466" }, merged.Select(o => o.ItemId));
    }

    [Fact]
    public void Merge_on_empty_input_returns_empty()
    {
        Assert.Empty(OrderMerge.Merge(new List<PendingOrder>()));
    }
}
