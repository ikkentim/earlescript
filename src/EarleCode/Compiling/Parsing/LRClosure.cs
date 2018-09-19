using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EarleCode.Compiling.Parsing
{
	/// <summary>
	/// Represents a closure.
	/// </summary>
	public class LRClosure : IEnumerable<LRItem>
	{
		/// <summary>
		/// Gets the items of this closure.
		/// </summary>
		public HashSet<LRItem> Items { get; } = new HashSet<LRItem>();

		/// <summary>
		/// Adds the specified <paramref name="item"/> to this closure.
		/// </summary>
		/// <param name="item">The item to add.</param>
		/// <returns><c>true</c> if the item was added; <c>false</c> otherwise.</returns>
		public bool Add(LRItem item)
		{
			return Items.Add(item);
		}

		/// <summary>
		/// Adds the items of the specified <paramref name="closure"/> to this closure.
		/// </summary>
		/// <param name="closure">The closure to add.</param>
		/// <returns><c>true</c> if an item was added; <c>false</c> otherwise.</returns>
		public bool Add(LRClosure closure)
		{
			var response = false;
			foreach (var item in closure.Items)
				response |= Add(item);

			return response;
		}

		#region Implementation of IEnumerable

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		public IEnumerator<LRItem> GetEnumerator()
		{
			return Items.GetEnumerator();
		}

		/// <summary>Returns an enumerator that iterates through a collection.</summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Overrides of Object

		/// <summary>Returns a string that represents the current object.</summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString()
		{
			return string.Join("; ", Items);
		}

		#endregion

		#region Equality members

		/// <summary>
		/// Determines whether the specified closure is equal to the current closure.
		/// </summary>
		/// <param name="other">The closure to compare with the current closure.</param>
		/// <returns>true if the specified closure is equal to the current closure; otherwise, false.</returns>
		public bool Equals(LRClosure other)
		{
			return Items.SetEquals(other.Items);
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((LRClosure) obj);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return (Items != null ? Items.GetHashCode() : 0);
		}

		/// <summary>Returns a value that indicates whether the values of two <see cref="T:ConsoleApp1.Compiling.Parsing.Closure" /> objects are equal.</summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
		public static bool operator ==(LRClosure left, LRClosure right)
		{
			return Equals(left, right);
		}

		/// <summary>Returns a value that indicates whether two <see cref="T:ConsoleApp1.Compiling.Parsing.Closure" /> objects have different values.</summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
		public static bool operator !=(LRClosure left, LRClosure right)
		{
			return !Equals(left, right);
		}

		#endregion
	}
}