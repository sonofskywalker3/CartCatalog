namespace CartCatalog
{
    /// <summary>Player-tunable settings, read from config.json and (optionally) edited via GMCM.</summary>
    public sealed class ModConfig
    {
        /// <summary>Shipping multiplier applied to each item's cart price (1.05 = +5%).</summary>
        public double Markup { get; set; } = 1.05;

        /// <summary>Whole days between ordering and mailbox delivery (1 = next morning).</summary>
        public int DeliveryDelayDays { get; set; } = 1;

        /// <summary>Gold price of the Cart Catalog book in the Traveling Cart.</summary>
        public int BookPrice { get; set; } = 5000;

        /// <summary>Chance (0..1) the book appears in the cart each visit, while not already owned.</summary>
        public double AppearanceChance { get; set; } = 0.5;
    }
}
