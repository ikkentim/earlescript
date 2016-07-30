using System;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime.Events
{
    public class EarleEventManager : IEarleEventManager
    {
        public event EventHandler<EarleEventNotifyEventArgs> EventFired;

        public void Notify(string eventName, EarleValue firer)
        {
            if(eventName == null)
                return;
            OnEventFired(this, new EarleEventNotifyEventArgs(eventName, firer));
        }

        protected virtual void OnEventFired(object sender, EarleEventNotifyEventArgs e)
        {
            EventFired?.Invoke(sender, e);
        }
    } 
}