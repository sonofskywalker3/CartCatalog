using CartCatalog.Core;
using Xunit;

namespace CartCatalog.Tests
{
    public class PorchSideTests
    {
        [Theory]
        [InlineData("Left", -3)]
        [InlineData("left", -3)]
        [InlineData("Right", 3)]
        [InlineData("nonsense", 3)]
        [InlineData(null, 3)]
        public void First_choice_follows_the_setting(string? side, int firstOffset)
            => Assert.Equal(firstOffset, PorchSide.PreferredOffsetsX(side)[0]);

        [Fact]
        public void Neither_side_uses_the_door_or_the_spouse_spot()
        {
            foreach (string side in PorchSide.All)
                foreach (int dx in PorchSide.PreferredOffsetsX(side))
                {
                    Assert.NotEqual(0, dx);
                    Assert.NotEqual(2, dx);
                }
        }
    }
}
