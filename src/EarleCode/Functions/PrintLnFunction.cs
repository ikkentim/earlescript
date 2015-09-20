﻿// EarleCode
// Copyright 2015 Tim Potze
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
using EarleCode.Values;

namespace EarleCode.Functions
{
    internal class PrintLnFunction : IEarleFunction
    {
        #region Implementation of IEarleFunction

        public InvocationResult Invoke(Runtime runtime, IEarleContext context, params EarleValue[] args)
        {
            foreach (var arg in args)
            {
                Console.WriteLine(arg.Value);
            }
            return InvocationResult.Empty;
        }

        public InvocationResult Continue(Runtime runtime, IncompleteInvocationResult incompleteInvocationResult)
        {
            throw new NotImplementedException("This function call does not await events");
        }

        #endregion
    }
}