namespace EarleCode.Debug
{
	public interface IFrameExecutor
	{
		string Name { get; }

		EarleValue? Run(IFrameExecutor parent);
	}
}