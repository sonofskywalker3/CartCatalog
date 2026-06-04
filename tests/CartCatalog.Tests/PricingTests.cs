using CartCatalog.Core;
using Xunit;

namespace CartCatalog.Tests;

public class PricingTests
{
    [Theory]
    [InlineData(100, 1.05, 105)]   // exact
    [InlineData(101, 1.05, 107)]   // 106.05 -> ceil 107
    [InlineData(1000, 1.0, 1000)]  // no markup
    [InlineData(33, 1.5, 50)]      // 49.5 -> ceil 50
    [InlineData(1, 1.05, 2)]       // 1.05 -> ceil 2
    public void OrderPrice_rounds_up(int cartPrice, double markup, int expected)
    {
        Assert.Equal(expected, Pricing.OrderPrice(cartPrice, markup));
    }
}
