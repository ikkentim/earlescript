namespace EarleCode.Interpreter
{
	public interface IFrameExecutor
	{
		string Name { get; }

		EarleValue? Run(IFrameExecutor parent);
	}
}