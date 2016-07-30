using EarleCode.Runtime.Values;

namespace EarleCode.Runtime.Events
{
    public class EarleSimpleEventableStructure : EarleStructure, IEarleEventableObject
    {
        public IEarleEventManager EventManager { get; } = new EarleEventManager();
    }
}