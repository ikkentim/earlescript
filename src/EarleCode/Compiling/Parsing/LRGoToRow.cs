using System.Collections.Generic;
using EarleCode.Compiling.Parsing.Grammars.Productions;

namespace EarleCode.Compiling.Parsing
{
    /// <summary>
    /// Represents a row in an <see cref="LRCanonicalCollection"/>.
    /// </summary>
    public class LRGoToRow
    {
        private readonly Dictionary<ProductionRuleElement, LRClosure> _items =
            new Dictionary<ProductionRuleElement, LRClosure>();

        /// <summary>
        /// Gets the closure of the specified <paramref name="element"/>
        /// </summary>
        /// <param name="element">The elemenent to get the closure of.</param>
        public LRClosure this[ProductionRuleElement element] => Get(element);

        /// <summary>
        /// Gets all production rule elements.
        /// </summary>
        public IEnumerable<ProductionRuleElement> Keys => _items.Keys;

        /// <summary>
        /// Gets all closures.
        /// </summary>
        public IEnumerable<LRClosure> Values => _items.Values;

        /// <summary>
        /// Gets the closure of the specified <paramref name="element"/>.
        /// </summary>
        /// <param name="element">The elemenent to get the closure of.</param>
        /// <returns>The closure of the specified element.</returns>
        public LRClosure Get(ProductionRuleElement element)
        {
            _items.TryGetValue(element, out var value);
            return value;
        }

        /// <summary>
        /// Adds the specified <paramref name="item"/> to the closure of the specified <paramref name="element"/>.
        /// </summary>
        /// <param name="element">The closure.</param>
        /// <param name="item">The item to add to the closure.</param>
        /// <returns><c>true</c> if the item has been added; <c>false</c> otherwise.</returns>
        public bool Add(ProductionRuleElement element, LRItem item)
        {
            if (!_items.TryGetValue(element, out var value))
                _items[element] = value = new LRClosure();
            return value.Add(item);
        }

        /// <summary>
        /// Adds the items from the specified <paramref name="closure"/> to the closure of the specified <paramref name="element"/>
        /// </summary>
        /// <param name="element">The closure.</param>
        /// <param name="closure">The closure to add to the closure.</param>
        /// <returns><c>true</c> if a value has been added; <c>false</c> otherwise.</returns>
        public bool Add(ProductionRuleElement element, LRClosure closure)
        {
            if (!_items.TryGetValue(element, out var value))
                _items[element] = value = new LRClosure();
            return value.Add(closure);
        }
    }
}