using System;
using System.Linq;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime.Events
{
    public interface IEarleEventManager : IEarleObject
    {
        event EventHandler<EarleEventNotifyEventArgs> EventFired;

        void Notify(string eventName, EarleValue firer);
    }
    
}
