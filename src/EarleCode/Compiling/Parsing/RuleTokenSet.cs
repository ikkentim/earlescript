using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Parsing
{
	/// <summary>
	/// Represents a rule to token collection dictionary.
	/// </summary>
    public abstract class RuleTokenSet : IRuleTokenSet
    {
        private readonly Dictionary<string, List<Token>> _set =
            new Dictionary<string, List<Token>>();

	    /// <summary>
	    /// Adds the specified token to the collection of the rule with the specified <paramref name="name"/>.
	    /// </summary>
	    /// <param name="name">The name of the rule to add the token to.</param>
	    /// <param name="value">The token to add to the collection of the specified rule.</param>
	    /// <returns><c>true</c> if the token was not yet present in the collection; false otherwise.</returns>
        protected bool Add(string name, Token value)
        {
            if (!_set.TryGetValue(name, out var tokens))
            {
                _set[name] = tokens = new List<Token>();
            }

            if (tokens.Contains(value)) return false;
            
            tokens.Add(value);
            return true;
        }
	    
	    /// <summary>
	    /// Adds the specified tokens to the collection of the rule with the specified <paramref name="name"/>.
	    /// </summary>
	    /// <param name="name">Then name of the rule to add the tokens to.</param>
	    /// <param name="values">The tokens to add to the collection of the specified rule.</param>
	    /// <returns><c>true</c> if any token was not yet present in the collection; false otherwise.</returns>
        protected bool Add(string name, IEnumerable<Token> values)
        {
	        if (values == null) return false;

            if (!_set.TryGetValue(name, out var tokens))
            {
                _set[name] = tokens = new List<Token>();
            }

            var result = false;
            foreach (var value in values)
            {
                if (tokens.Contains(value)) continue;

                tokens.Add(value);
                result = true;
            }

            return result;
        }

		#region Implementation of IRuleTokenSet

	    /// <summary>
	    /// Gets the tokens of the specified <paramref name="rule"/>.
	    /// </summary>
	    /// <param name="rule">The rule to get the tokens of.</param>
	    public IEnumerable<Token> this[string rule] => Get(rule);

	    /// <summary>
	    /// Gets the tokens of the specified <paramref name="rule"/>.
	    /// </summary>
	    /// <param name="rule">The rule to get the tokens of.</param>
	    /// <returns>The tokens of the rule.</returns>
	    public IEnumerable<Token> Get(string rule)
	    {
		    if (rule == null) throw new ArgumentNullException(nameof(rule));
            
		    _set.TryGetValue(rule, out var result);
		    return result;
	    }


		#endregion

	    #region Implementation of IEnumerable

	    /// <summary>Returns an enumerator that iterates through the collection.</summary>
	    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
	    public IEnumerator<KeyValuePair<string, IEnumerable<Token>>> GetEnumerator()
	    {
		    return _set
			    .Select(kv => new KeyValuePair<string, IEnumerable<Token>>(kv.Key, kv.Value))
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