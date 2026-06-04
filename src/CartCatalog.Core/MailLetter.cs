using System.Collections.Generic;
using System.Linq;

namespace CartCatalog.Core
{
    /// <summary>Builds the Data/mail letter body for delivered orders. Pure string formatting,
    /// kept in Core so it can be unit-tested without the game/SMAPI assemblies.</summary>
    public static class MailLetter
    {
        /// <summary>Single-item convenience overload. Equivalent to the batch builder with one order.</summary>
        public static string Build(string qualifiedItemId, int quantity)
            => Build(new[] { (qualifiedItemId, quantity) });

        /// <summary>Build ONE delivery letter carrying every ordered item. Orders sharing a qualified
        /// id are merged (quantities summed), preserving first-seen order, so repeated buys of the same
        /// item arrive as a single stack. Each distinct item is its OWN "%item id &lt;id&gt; &lt;count&gt; %%"
        /// block: LetterViewerMenu only honors a single id/count pair per block (multiple pairs in one
        /// block are chosen at random), while its parser loops over every block and its grab handler
        /// hands out one stack per tap. The "%item id" token calls <c>ItemRegistry.Create(id, count)</c>
        /// verbatim, so it works for every mailable type — (O)(BC)(F)(H)(W)(B)(S/P)(R).</summary>
        public static string Build(IEnumerable<(string itemId, int quantity)> items)
        {
            var order = new List<string>();
            var totals = new Dictionary<string, int>();
            foreach ((string id, int qty) in items)
            {
                if (!totals.ContainsKey(id)) order.Add(id);
                totals[id] = totals.TryGetValue(id, out int cur) ? cur + qty : qty;
            }

            string tokens = string.Concat(order.Select(id => $"%item id {id} {totals[id]} %%"));
            return $"Your Cart Catalog order has arrived.^^{tokens}[#]Cart Catalog Delivery";
        }
    }
}
