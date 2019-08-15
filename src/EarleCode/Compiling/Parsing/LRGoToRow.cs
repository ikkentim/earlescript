using System.Collections;
using System.Collections.Generic;
using EarleCode.Compiling.Parsing.Grammars.Productions;

namespace EarleCode.Compiling.Parsing
{
    /// <summary>
    /// Represents a row in an <see cref="SLRCanonicalCollection"/>.
    /// </summary>
    public class LRGoToRow<T>: IEnumerable<KeyValuePair<ProductionRuleElement, Closure<T>>>
    {
	    private readonly Dictionary<ProductionRuleElement, Closure<T>> _items =
		    new Dictionary<ProductionRuleElement, Closure<T>>(new EqualityComparer());

        /// <summary>
        /// Gets the closure of the specified <paramref name="element"/>
        /// </summary>
        /// <param name="element">The element to get the closure of.</param>
        public Closure<T> this[ProductionRuleElement element] => Get(element);

        /// <summary>
        /// Gets all production rule elements.
        /// </summary>
        public IEnumerable<ProductionRuleElement> Keys => _items.Keys;

        /// <summary>
        /// Gets all closures.
        /// </summary>
        public IEnumerable<Closure<T>> Values => _items.Values;

        /// <summary>
        /// Gets the closure of the specified <paramref name="element"/>.
        /// </summary>
        /// <param name="element">The element to get the closure of.</param>
        /// <returns>The closure of the specified element.</returns>
        private Closure<T> Get(ProductionRuleElement element)
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
        public bool Add(ProductionRuleElement element, T item)
        {
            if (!_items.TryGetValue(element, out var value))
                _items[element] = value = new Closure<T>();
            return value.Add(item);
        }

        /// <summary>
        /// Adds the items from the specified <paramref name="closure"/> to the closure of the specified <paramref name="element"/>
        /// </summary>
        /// <param name="element">The closure.</param>
        /// <param name="closure">The closure to add to the closure.</param>
        /// <returns><c>true</c> if a value has been added; <c>false</c> otherwise.</returns>
        public bool Add(ProductionRuleElement element, Closure<T> closure)
        {
            if (!_items.TryGetValue(element, out var value))
                _items[element] = value = new Closure<T>();
            return value.Add(closure);
        }

        public IEnumerator<KeyValuePair<ProductionRuleElement, Closure<T>>> GetEnumerator()
        {
	        return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
	        return GetEnumerator();
        }

        private class EqualityComparer : IEqualityComparer<ProductionRuleElement>
        {
	        public bool Equals(ProductionRuleElement x, ProductionRuleElement y)
	        {
		        return ReferenceEquals(x, y);
	        }

	        public int GetHashCode(ProductionRuleElement obj)
	        {
		        return obj.GetHashCode();
	        }
        }
	}
}