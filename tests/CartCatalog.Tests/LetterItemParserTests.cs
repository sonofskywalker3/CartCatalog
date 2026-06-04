using System.Linq;
using CartCatalog.Core;
using Xunit;

namespace CartCatalog.Tests;

public class LetterItemParserTests
{
    [Fact]
    public void Parse_extracts_a_single_item_token()
    {
        var orders = LetterItemParser.Parse("Your Cart Catalog order has arrived.^^%item id (O)128 3 %%[#]Cart Catalog Delivery");

        Assert.Single(orders);
        Assert.Equal("(O)128", orders[0].ItemId);
        Assert.Equal(3, orders[0].Quantity);
        Assert.Equal(0, orders[0].DeliveryDay);
    }

    [Fact]
    public void Parse_extracts_multiple_concatenated_tokens_in_order()
    {
        var orders = LetterItemParser.Parse(
            "Your Cart Catalog order has arrived.^^%item id (O)128 1 %%%item id (F)1466 2 %%[#]Cart Catalog Delivery");

        Assert.Equal(new[] { "(O)128", "(F)1466" }, orders.Select(o => o.ItemId));
        Assert.Equal(new[] { 1, 2 }, orders.Select(o => o.Quantity));
    }

    [Fact]
    public void Parse_returns_empty_for_null_or_empty()
    {
        Assert.Empty(LetterItemParser.Parse(null));
        Assert.Empty(LetterItemParser.Parse(""));
    }

    [Fact]
    public void Parse_returns_empty_when_no_tokens_present()
    {
        Assert.Empty(LetterItemParser.Parse("Just a plain letter with no attachments.[#]Title"));
    }
}
