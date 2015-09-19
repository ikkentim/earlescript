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

using System.IO;
using System.Threading;

namespace EarleCode.Debug
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var context = new SampleContext();
            var runtime = new Runtime();

            runtime.LoadFile(@"\main", File.ReadAllText("earle/main.earle"));
            runtime.LoadFile(@"\ext", File.ReadAllText("earle/ext.earle"));
            
            runtime.Invoke(context, "init", @"\main");

            for (;;)
            {
                if (runtime.WaitingCallsCount > 0)
                {
                    System.Diagnostics.Debug.WriteLine("cont");
                    runtime.Continue();
                }
            }
        }
    }

    internal class SampleContext : IEarleContext
    {
        #region Overrides of Object

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return "Sample Context";
        }

        #endregion
    }
}