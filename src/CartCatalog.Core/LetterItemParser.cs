using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CartCatalog.Core
{
    /// <summary>Parses a legacy Cart Catalog mail-letter body (pre-porch versions) back into its items
    /// so a one-time migration can re-deliver letters that were still unread when the player updated.
    /// The old body embedded each item as a "%item id &lt;qualifiedId&gt; &lt;count&gt; %%" token (see the
    /// removed MailLetter.Build). Pure, kept in Core so it's unit-testable.</summary>
    public static class LetterItemParser
    {
        private static readonly Regex ItemToken = new Regex(@"%item id (\S+) (\d+) %%", RegexOptions.Compiled);

        /// <summary>Extract the (itemId, quantity) tokens from a letter body as immediately-due orders
        /// (DeliveryDay 0). Returns an empty list for null/empty/token-less bodies.</summary>
        public static List<PendingOrder> Parse(string body)
        {
            var orders = new List<PendingOrder>();
            if (string.IsNullOrEmpty(body)) return orders;
            foreach (Match m in ItemToken.Matches(body))
            {
                if (int.TryParse(m.Groups[2].Value, out int qty) && qty > 0)
                    orders.Add(new PendingOrder { ItemId = m.Groups[1].Value, Quantity = qty, DeliveryDay = 0 });
            }
            return orders;
        }
    }
}
