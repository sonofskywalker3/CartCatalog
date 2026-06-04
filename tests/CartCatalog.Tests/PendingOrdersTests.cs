using System.Collections.Generic;
using System.Linq;
using CartCatalog.Core;
using Xunit;

namespace CartCatalog.Tests;

public class PendingOrdersTests
{
    private static PendingOrder Order(string id, int qty, int day)
        => new PendingOrder { ItemId = id, Quantity = qty, DeliveryDay = day };

    [Fact]
    public void Due_returns_orders_on_or_before_today_and_keeps_the_rest()
    {
        var orders = new List<PendingOrder>
        {
            Order("(O)24", 2, 5),   // due (== today)
            Order("(O)88", 1, 4),   // due (< today)
            Order("(O)90", 3, 6),   // future
        };

        var (due, remaining) = PendingOrders.Due(orders, today: 5);

        Assert.Equal(new[] { "(O)24", "(O)88" }, due.Select(o => o.ItemId).OrderBy(s => s));
        Assert.Equal(new[] { "(O)90" }, remaining.Select(o => o.ItemId));
    }

    [Fact]
    public void Due_with_no_due_orders_returns_empty_due_and_all_remaining()
    {
        var orders = new List<PendingOrder> { Order("(O)24", 1, 10) };

        var (due, remaining) = PendingOrders.Due(orders, today: 3);

        Assert.Empty(due);
        Assert.Single(remaining);
    }

    [Fact]
    public void Due_on_empty_input_returns_two_empty_lists()
    {
        var (due, remaining) = PendingOrders.Due(new List<PendingOrder>(), today: 1);

        Assert.Empty(due);
        Assert.Empty(remaining);
    }
}
