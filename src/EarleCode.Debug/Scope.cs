using System;
using System.Collections.Generic;

namespace EarleCode.Debug
{
	public class Scope : IScope
	{
		public IScope Parent { get; set; }
		public Dictionary<string, EarleValue> Variables = new Dictionary<string, EarleValue>();

		public EarleValue this[string name]
		{
			get
			{
				if (Parent == this)
				{
					throw new RuntimeException("Scope broke");
				}

				if (name == null) throw new ArgumentNullException(nameof(name));
				return Variables.TryGetValue(name, out var value)
					? value
					: Parent?[name] ?? EarleValue.Null;
			}
			set
			{
				if (name == null) throw new ArgumentNullException(nameof(name));
				if (!(Parent?.SetIfPresent(name, value) ?? false))
					Variables[name] = value;
			}
		}

		public bool SetIfPresent(string name, EarleValue value)
		{
			if (Parent != null && Parent.SetIfPresent(name, value))
				return true;

			if (Variables.ContainsKey(name))
			{
				Variables[name] = value;
				return true;
			}

			return false;
		}
	}
}