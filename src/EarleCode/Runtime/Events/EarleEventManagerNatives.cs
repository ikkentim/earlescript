// EarleCode
// Copyright 2016 Tim Potze
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Linq;
using EarleCode.Runtime.Attributes;
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime.Events
{
    /// <summary>
    ///     Contains the Earle native functions provided by the event system.
    /// </summary>
    internal class EarleEventManagerNatives
    {
        /// <summary>
        ///     Gets the event manager from the specified stack frame. If no manager could be found, a warning is sent to the
        ///     runtime and null is returned.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="funcName">Name of the function.</param>
        /// <returns></returns>
        private static IEarleEventManager GetManager(EarleStackFrame frame, string funcName)
        {
            var manager = frame.Executor.Target.As<IEarleEventableObject>()?.EventManager;

            if (manager == null)
                frame.Runtime.HandleWarning($"A valid target must be specified when calling `{funcName}`");

            return manager;
        }

        /// <summary>
        ///     A native which will wait for the specified event name to be fired on the current target.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="eventName">Name of the event.</param>
        [EarleNativeFunction]
        private static void WaitTill(EarleStackFrame frame, string eventName)
        {
            var manager = GetManager(frame, "waittill");

            if (manager != null)
                frame.ChildFrame =
                    new WaitTillStackFrameExecutor(frame, frame.Executor.Target, manager, eventName).Frame;
        }

        /// <summary>
        ///     A native which will end the current thread when the specified event is  fired on the current target.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="eventName">Name of the event.</param>
        [EarleNativeFunction]
        private static void EndOn(EarleStackFrame frame, string eventName)
        {
            var manager = GetManager(frame, "endon");

            if (manager != null)
            {
                var thread = frame.Thread;
                manager.EventFired += (sender, e) =>
                {
                    if (e.EventName == eventName)
                    {
                        thread.Kill();
                    }
                };
            }
        }

        /// <summary>
        ///     A native which will notify the event manager of the current target of the specified event name.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="optionals">The optionals.</param>
        [EarleNativeFunction]
        private static void Notify(EarleStackFrame frame, string eventName, EarleValue[] optionals)
        {
            GetManager(frame, "notify")
                ?.Notify(eventName, optionals?.FirstOrDefault() ?? EarleValue.Undefined);
        }

        /// <summary>
        ///     Represents the stack frame executor for the waittill native function.
        /// </summary>
        /// <seealso cref="EarleCode.Runtime.EarleBaseStackFrameExecutor" />
        private class WaitTillStackFrameExecutor : EarleBaseStackFrameExecutor
        {
            private readonly string _eventName;
            private EarleValue _firer;
            private bool _hasFired;

            /// <summary>
            ///     Initializes a new instance of the <see cref="WaitTillStackFrameExecutor" /> class.
            /// </summary>
            /// <param name="parentFrame">The parent frame.</param>
            /// <param name="target">The target.</param>
            /// <param name="eventManager">The event manager.</param>
            /// <param name="eventName">Name of the event.</param>
            /// <exception cref="System.ArgumentNullException">
            ///     Thrown if <see cref="eventManager" /> or <see cref="eventName" /> is
            ///     null.
            /// </exception>
            public WaitTillStackFrameExecutor(EarleStackFrame parentFrame, EarleValue target,
                IEarleEventManager eventManager,
                string eventName) : base(target)
            {
                if (eventManager == null)
                    throw new ArgumentNullException(nameof(eventManager));
                if (eventName == null)
                    throw new ArgumentNullException(nameof(eventName));

                Frame = parentFrame.SpawnChild(null, this, EarleStackFrame.SleepCallIP);

                _eventName = eventName;
                eventManager.EventFired += OnEventFired;
            }

            /// <summary>
            ///     Called when an event is fired.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The <see cref="EarleEventNotifyEventArgs" /> instance containing the event data.</param>
            private void OnEventFired(object sender, EarleEventNotifyEventArgs e)
            {
                if (e.EventName == _eventName)
                {
                    _hasFired = true;
                    _firer = e.Firer;
                }
            }

            /// <summary>
            ///     Runs this frame.
            /// </summary>
            /// <returns>
            ///     null if the execution did not complete or a value if it did.
            /// </returns>
            public override EarleValue? Run()
            {
                return _hasFired ? (EarleValue?) _firer : null;
            }
        }
    }
}