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
    ///     Contains the event data for the <see cref="IEarleEventManager.EventFired" /> event.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class EarleEventNotifyEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EarleEventNotifyEventArgs" /> class.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="firer">The firer of the event.</param>
        public EarleEventNotifyEventArgs(string eventName, EarleValue firer)
        {
            EventName = eventName;
            Firer = firer;
        }

        /// <summary>
        ///     Gets the name of the event.
        /// </summary>
        public string EventName { get; }

        /// <summary>
        ///     Gets the firer of the event.
        /// </summary>
        public EarleValue Firer { get; }
    }
}