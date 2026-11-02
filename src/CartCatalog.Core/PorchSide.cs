namespace CartCatalog.Core
{
    /// <summary>Which side of the farmhouse door the delivery crate prefers. A farmhouse retexture can
    /// hide the crate behind its art on one side, so the player can pick the other.</summary>
    public static class PorchSide
    {
        public const string Right = "Right";
        public const string Left = "Left";

        /// <summary>The choices the settings menu offers, default first.</summary>
        public static readonly string[] All = { Right, Left };

        // +3 is the usual spot and +1 its fallback; both clear the spouse's standing spot at +2.
        // The left side has no spouse spot, and -1 flanks the door, so it tries -3 then -2.
        private static readonly int[] RightOffsetsX = { 3, 1 };
        private static readonly int[] LeftOffsetsX = { -3, -2 };

        /// <summary>Preferred X-offsets from the door entry, best first. Anything but "Left" reads as
        /// the default right side, so a hand-edited config can never lose the crate.</summary>
        public static int[] PreferredOffsetsX(string? side)
            => string.Equals(side, Left, System.StringComparison.OrdinalIgnoreCase) ? LeftOffsetsX : RightOffsetsX;
    }
}
