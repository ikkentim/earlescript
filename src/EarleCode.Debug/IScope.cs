﻿namespace EarleCode.Debug
{
	public interface IScope
	{
		EarleValue this[string name] { get; }

		bool SetIfPresent(string name, EarleValue value);
	}
}