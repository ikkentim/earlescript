namespace EarleCode.Interpreter
{
	public interface IEarleFunction
	{
		int ParameterCount { get; }

		string Name { get; }

		IFrameExecutor GetFrameExecutor(EarleValue target, EarleValue[] args);
	}
}