using System;

namespace CartCatalog.Core
{
    /// <summary>Pure pricing math for catalog orders. Shipping markup is applied to the cart's gold
    /// price and rounded up to the next whole gold.</summary>
    public static class Pricing
    {
        /// <summary>The gold cost to order one unit: <c>ceil(cartPrice * markup)</c>.</summary>
        public static int OrderPrice(int cartPrice, double markup)
            => (int)Math.Ceiling(cartPrice * markup);
    }
}
