using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EarleCode.Compiling.Lexing;
using EarleCode.Compiling.Parsing.Grammars;

namespace EarleCode.Compiling.Parsing
{
    /// <summary>
    /// Represents a rule to terminals collection dictionary.
    /// </summary>
    public abstract class RuleTerminalSet : IRuleTerminalSet
    {
        private readonly Dictionary<string, List<Terminal>> _set =
            new Dictionary<string, List<Terminal>>();

        /// <summary>
        /// Adds the specified terminal to the collection of the rule with the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name of the rule to add the terminal to.</param>
        /// <param name="value">The terminal to add to the collection of the specified rule.</param>
        /// <returns><c>true</c> if the terminal was not yet present in the collection; false otherwise.</returns>
        protected bool Add(string name, Terminal value)
        {
            if (!_set.TryGetValue(name, out var terminals))
            {
                _set[name] = terminals = new List<Terminal>();
            }

            if (terminals.Contains(value)) return false;
            
            terminals.Add(value);
            return true;
        }
	    
        /// <summary>
        /// Adds the specified terminals to the collection of the rule with the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Then name of the rule to add the terminals to.</param>
        /// <param name="values">The terminals to add to the collection of the specified rule.</param>
        /// <returns><c>true</c> if any terminal was not yet present in the collection; false otherwise.</returns>
        protected bool Add(string name, IEnumerable<Terminal> values)
        {
            if (values == null) return false;

            if (!_set.TryGetValue(name, out var terminals))
            {
                _set[name] = terminals = new List<Terminal>();
            }

            var result = false;
            foreach (var value in values)
            {
                if (terminals.Contains(value)) continue;

                terminals.Add(value);
                result = true;
            }

            return result;
        }

        #region Implementation of IRuleTerminalSet

        /// <summary>
        /// Gets the terminals of the specified <paramref name="rule"/>.
        /// </summary>
        /// <param name="rule">The rule to get the terminals of.</param>
        public IEnumerable<Terminal> this[string rule] => Get(rule);

        /// <summary>
        /// Gets the terminals of the specified <paramref name="rule"/>.
        /// </summary>
        /// <param name="rule">The rule to get the terminals of.</param>
        /// <returns>The terminals of the rule.</returns>
        public IEnumerable<Terminal> Get(string rule)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            
            _set.TryGetValue(rule, out var result);
            return result;
        }


        #endregion

        #region Implementation of IEnumerable

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<string, IEnumerable<Terminal>>> GetEnumerator()
        {
            return _set
                .Select(kv => new KeyValuePair<string, IEnumerable<Terminal>>(kv.Key, kv.Value))
                .GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}