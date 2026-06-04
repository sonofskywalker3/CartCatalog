namespace CartCatalog.Core
{
    /// <summary>One queued mail order: a qualified item id, a quantity, and the absolute day
    /// (<c>Game1.Date.TotalDays</c>) on which it should be delivered. Public get/set + a
    /// parameterless ctor so it round-trips through JSON (SMAPI save data / System.Text.Json).</summary>
    public sealed class PendingOrder
    {
        public string ItemId { get; set; } = "";
        public int Quantity { get; set; }
        public int DeliveryDay { get; set; }
    }
}
