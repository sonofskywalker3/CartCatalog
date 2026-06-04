namespace CartCatalog.Core
{
    /// <summary>Builds the Data/mail letter body for a delivered order. Pure string formatting,
    /// kept in Core so it can be unit-tested without the game/SMAPI assemblies.</summary>
    public static class MailLetter
    {
        /// <summary>Letter body using LetterViewerMenu's universal "%item id &lt;qualifiedId&gt;
        /// &lt;count&gt;" token. That token calls <c>ItemRegistry.Create(id, count)</c> verbatim, so it
        /// honors quantity for EVERY mailable item type (object, big craftable, furniture, hat,
        /// weapon, boots, ring, clothing). The older "%item bigobject"/"%item furniture" tokens
        /// prepend the prefix and ignore count, which charged-for-N-but-delivered-1.</summary>
        public static string Build(string qualifiedItemId, int quantity)
            => $"Your Cart Catalog order has arrived.^^%item id {qualifiedItemId} {quantity} %%[#]Cart Catalog Delivery";
    }
}
