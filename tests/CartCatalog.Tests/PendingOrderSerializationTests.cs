using System.Collections.Generic;
using System.Text.Json;
using CartCatalog.Core;
using Xunit;

namespace CartCatalog.Tests;

public class PendingOrderSerializationTests
{
    [Fact]
    public void List_of_orders_round_trips_without_loss()
    {
        var original = new List<PendingOrder>
        {
            new() { ItemId = "(O)24", Quantity = 2, DeliveryDay = 7 },
            new() { ItemId = "(BC)12", Quantity = 1, DeliveryDay = 9 },
        };

        string json = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<List<PendingOrder>>(json)!;

        Assert.Equal(2, restored.Count);
        Assert.Equal("(O)24", restored[0].ItemId);
        Assert.Equal(2, restored[0].Quantity);
        Assert.Equal(7, restored[0].DeliveryDay);
        Assert.Equal("(BC)12", restored[1].ItemId);
        Assert.Equal(1, restored[1].Quantity);
        Assert.Equal(9, restored[1].DeliveryDay);
    }
}
