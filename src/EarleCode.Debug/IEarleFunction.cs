using System.Collections.Generic;

namespace EarleCode.Debug
{
	public interface IEarleFunction
	{
		int ParameterCount { get; }

		string Name { get; }

		IFrameExecutor GetFrameExecutor(EarleValue target, EarleValue[] args);
	}
}