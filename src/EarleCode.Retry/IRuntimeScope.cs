namespace EarleCode
{
    public interface IRuntimeScope
    {
        EarleValue? GetValue(EarleVariableReference reference);
        bool SetValue(EarleVariableReference reference, EarleValue value);
    }
}