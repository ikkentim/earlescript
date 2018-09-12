using System;
using System.Collections.Generic;

namespace EarleCode.Debug
{
	public class Scope : IScope
	{
		private readonly Dictionary<string, EarleValue> _variables = new Dictionary<string, EarleValue>();

		public Scope(IScope parent)
		{
			Parent = parent;
		}

		public EarleValue this[string name]
		{
			get
			{
				if (Parent == this)
					throw new RuntimeException("Circular scoping");

				if (Parent == null)
					throw new RuntimeException("Orphan scope");

				if (name == null)
					throw new ArgumentNullException(nameof(name));

				return _variables.TryGetValue(name, out var value)
					? value
					: Parent[name];
			}
			set
			{
				if (Parent == this)
					throw new RuntimeException("Circular scoping");

				if (Parent == null)
					throw new RuntimeException("Orphan scope");

				if (name == null) throw new ArgumentNullException(nameof(name));
				if (!Parent.SetIfPresent(name, value))
					_variables[name] = value;
			}
		}

		public IScope Parent { get; }

		public void SetInCurrent(string name, EarleValue value)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			_variables[name] = value;
		}

		public bool SetIfPresent(string name, EarleValue value)
		{
			if (Parent == this)
				throw new RuntimeException("Circular scoping");

			if (Parent == null)
				throw new RuntimeException("Orphan scope");

			if (name == null)
				throw new ArgumentNullException(nameof(name));

			if (Parent.SetIfPresent(name, value))
				return true;

			if (_variables.ContainsKey(name))
			{
				_variables[name] = value;
				return true;
			}

			return false;
		}
	}
}