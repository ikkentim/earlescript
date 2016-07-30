using System;
using System.Linq;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime.Events
{
    internal class EarleEventManagerNatives
    {
        private static IEarleEventManager GetManager(EarleStackFrame frame)
        {
            var manager = frame.Target.As<IEarleEventableObject>()?.EventManager;

            if(manager == null)
                frame.Runtime.HandleWarning("A valid target must be specified when calling waittill");
            
            return manager;
        }

        [EarleNativeFunction]
        private static void WaitTill(EarleStackFrame frame, string eventName)
        {
            var manager = GetManager(frame);

            if(manager != null)
                frame.SubFrame = new WaitTillStackFrameExecutor(frame.SpawnSubFrame(frame.Target), manager, eventName);
        }

        [EarleNativeFunction]
        private static void EndOn(EarleStackFrame frame, string eventName)
        {
            var manager = GetManager(frame);

            var thread = frame.Thread;
            manager.EventFired += (sender, e) => {
                if(e.EventName == eventName)
                {
                    thread.Kill();
                }
            };
        }

        [EarleNativeFunction]
        private static void Notify(EarleStackFrame frame, string eventName, EarleValue[] optionals)
        {
            var manager = frame.Target.As<IEarleEventableObject>()?.EventManager;

            if(manager == null)
            {
                frame.Runtime.HandleWarning("A valid target must be specified when calling notify");
                return;
            }

            manager.Notify(eventName, optionals?.FirstOrDefault() ?? EarleValue.Undefined);
        }

        private class WaitTillStackFrameExecutor : EarleStackFrameExecutor
        {
            private bool _hasFired;
            private EarleValue _firer;
            private string _eventName;

            public WaitTillStackFrameExecutor(EarleStackFrame frame, IEarleEventManager eventManager, string eventName) : base(frame, null, null)
            {
                if(eventManager == null)
                    throw new ArgumentNullException(nameof(eventManager));
                if(eventName == null)
                    throw new ArgumentNullException(nameof(eventName));

                _eventName = eventName;
                eventManager.EventFired += OnEventFired;
            }

            private void OnEventFired(object sender, EarleEventNotifyEventArgs e)
            {
                if(e.EventName == _eventName)
                {
                    _hasFired = true;
                    _firer = e.Firer;
                }
            }

            public override EarleValue? Run()
            {
                return _hasFired ? (EarleValue?)_firer : null;
            }
        }
    }
}
