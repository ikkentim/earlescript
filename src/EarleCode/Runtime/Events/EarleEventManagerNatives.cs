using System;
using System.Linq;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime.Events
{
    internal class EarleEventManagerNatives
    {
        private static IEarleEventManager GetManager(EarleStackFrame frame, string funcName)
        {
            var manager = frame.Executor.Target.As<IEarleEventableObject>()?.EventManager;

            if(manager == null)
                frame.Runtime.HandleWarning($"A valid target must be specified when calling `{funcName}`");
            
            return manager;
        }

        [EarleNativeFunction]
        private static void WaitTill(EarleStackFrame frame, string eventName)
        {
            var manager = GetManager(frame, "waittill");

            if(manager != null)
                frame.ChildFrame = new WaitTillStackFrameExecutor(frame, frame.Executor.Target, manager, eventName).Frame;
        }

        [EarleNativeFunction]
        private static void EndOn(EarleStackFrame frame, string eventName)
        {
            var manager = GetManager(frame, "endon");

            if(manager != null)
            {
                var thread = frame.Thread;
                manager.EventFired += (sender, e) => {
                    if(e.EventName == eventName)
                    {
                        thread.Kill();
                    }
                };
            }
        }

        [EarleNativeFunction]
        private static void Notify(EarleStackFrame frame, string eventName, EarleValue[] optionals)
        {
            GetManager(frame, "notify")
                ?.Notify(eventName, optionals?.FirstOrDefault() ?? EarleValue.Undefined);
        }

        private class WaitTillStackFrameExecutor : EarleBaseStackFrameExecutor
        {
            private bool _hasFired;
            private EarleValue _firer;
            private string _eventName;

            public WaitTillStackFrameExecutor(EarleStackFrame parentFrame, EarleValue target, IEarleEventManager eventManager, string eventName) : base(target)
            {
                if(eventManager == null)
                    throw new ArgumentNullException(nameof(eventManager));
                if(eventName == null)
                    throw new ArgumentNullException(nameof(eventName));

                Frame = parentFrame.SpawnChild(null, this, -2);//TODO? -2 ? why?

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
