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

namespace EarleCode.Compiler
{
    public class CompiledBlock
    {
        public CompiledBlock(byte[] pCode, int[] breaks, int[] continues)
        {
            if(pCode == null) throw new ArgumentNullException(nameof(pCode));
            if(breaks == null) throw new ArgumentNullException(nameof(breaks));
            if(continues == null) throw new ArgumentNullException(nameof(continues));
            
            PCode = pCode;
            Breaks = breaks;
            Continues = continues;
        }

        public byte[] PCode { get; }
        public int[] Breaks { get; }
        public int[] Continues { get; }
        public int Length => PCode.Length;

        public static CompiledBlock Empty { get; } = new CompiledBlock(new byte[0], new int[0], new int[0]);
    }
    
}