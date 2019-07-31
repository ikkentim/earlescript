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
using EarleCode.Runtime.Values;

namespace EarleCode.Runtime.Events
{
    /// <summary>
    ///     Represents an event manager for <see cref="IEarleObject" /> instances which implement
    ///     <see cref="IEarleEventableObject" />.
    /// </summary>
    /// <seealso cref="EarleCode.Runtime.Events.IEarleEventManager" />
    public class EarleEventManager : IEarleEventManager
    {
        /// <summary>
        ///     Occurs when an event is fired.
        /// </summary>
        public event EventHandler<EarleEventNotifyEventArgs> EventFired;

        /// <summary>
        ///     Notifies the specified event was fired to all of this instances listeners.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="firer">The firer of the event.</param>
        public void Notify(string eventName, EarleValue firer)
        {
            if (eventName == null)
                return;
            OnEventFired(this, new EarleEventNotifyEventArgs(eventName, firer));
        }

        /// <summary>
        ///     Called when an event is fired.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EarleEventNotifyEventArgs" /> instance containing the event data.</param>
        protected virtual void OnEventFired(object sender, EarleEventNotifyEventArgs e)
        {
            EventFired?.Invoke(sender, e);
        }
    }
}