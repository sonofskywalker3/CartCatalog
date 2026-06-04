using System.Text.RegularExpressions;
using CartCatalog.Core;
using Xunit;

namespace CartCatalog.Tests;

public class MailLetterTests
{
    [Fact]
    public void Build_emits_the_universal_item_id_token_with_qualified_id_and_count()
    {
        string body = MailLetter.Build("(O)128", 3);

        Assert.Equal(
            "Your Cart Catalog order has arrived.^^%item id (O)128 3 %%[#]Cart Catalog Delivery",
            body);
    }

    [Theory]
    [InlineData("(O)128", 1)]   // object
    [InlineData("(BC)12", 5)]   // big craftable — old token ignored this count
    [InlineData("(F)1466", 2)]  // furniture — old token ignored this count
    [InlineData("(H)1", 4)]     // hat — was silently dropped by the old (O)/(BC)/(F) gate
    [InlineData("(W)4", 1)]     // weapon
    [InlineData("(B)504", 1)]   // boots
    [InlineData("(R)529", 1)]   // ring
    public void Build_always_carries_the_qualified_id_and_quantity_verbatim(string id, int qty)
    {
        string body = MailLetter.Build(id, qty);

        // The count must appear right after the id so ItemRegistry.Create(id, count) honors it for
        // every type — the whole point of switching off the per-type bigobject/furniture tokens.
        Assert.Contains($"%item id {id} {qty} ", body);
    }

    [Fact]
    public void Build_batch_with_one_item_matches_the_single_item_overload()
    {
        Assert.Equal(MailLetter.Build("(O)128", 3),
            MailLetter.Build(new[] { ("(O)128", 3) }));
    }

    [Fact]
    public void Build_batch_emits_one_item_block_per_distinct_item_with_a_single_intro_and_title()
    {
        string body = MailLetter.Build(new[] { ("(O)128", 1), ("(F)1466", 2) });

        Assert.Equal(
            "Your Cart Catalog order has arrived.^^%item id (O)128 1 %%%item id (F)1466 2 %%[#]Cart Catalog Delivery",
            body);
        // exactly one intro line and one title marker, regardless of item count
        Assert.Single(Regex.Matches(body, @"Your Cart Catalog order has arrived\."));
        Assert.Single(Regex.Matches(body, @"\[#\]Cart Catalog Delivery"));
    }

    [Fact]
    public void Build_batch_merges_same_item_orders_into_one_summed_stack()
    {
        // The reported case: buy 1, then buy 4 more of the same item -> one stack of 5, one block.
        string body = MailLetter.Build(new[] { ("(O)739", 1), ("(O)739", 4) });

        Assert.Equal(
            "Your Cart Catalog order has arrived.^^%item id (O)739 5 %%[#]Cart Catalog Delivery",
            body);
    }

    [Fact]
    public void Build_batch_preserves_first_seen_order_when_merging()
    {
        string body = MailLetter.Build(new[] { ("(O)739", 1), ("(O)128", 2), ("(O)739", 3) });

        // (O)739 stays first (first seen) and sums to 4; (O)128 follows.
        Assert.Equal(
            "Your Cart Catalog order has arrived.^^%item id (O)739 4 %%%item id (O)128 2 %%[#]Cart Catalog Delivery",
            body);
    }
}
