using System;
namespace EarleCode.Runtime.Events
{
    public interface IEarleEventableObject
    {
        IEarleEventManager EventManager { get; }
    }
}

