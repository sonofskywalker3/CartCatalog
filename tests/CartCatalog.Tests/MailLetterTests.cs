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
}
