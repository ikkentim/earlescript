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
    public interface IEarleEventManager : IEarleObject
    {
        /// <summary>
        ///     Occurs when an event is fired.
        /// </summary>
        event EventHandler<EarleEventNotifyEventArgs> EventFired;

        /// <summary>
        ///     Notifies the specified event was fired to all of this instances listeners.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="firer">The firer of the event.</param>
        void Notify(string eventName, EarleValue firer);
    }
}