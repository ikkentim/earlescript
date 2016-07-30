using System;
using System.Linq;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime.Events
{

    public class EarleEventNotifyEventArgs : EventArgs
    {
        public EarleEventNotifyEventArgs(string eventName, EarleValue firer)
        {
            EventName = eventName;
            Firer = firer;
        }

        public string EventName { get; }
        public EarleValue Firer { get; }
    }
    
}
