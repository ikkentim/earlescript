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
using System.Collections.Generic;

namespace EarleCode.Compiler
{
	internal class CompiledBlock
	{
		public CompiledBlock(byte[] pCode, Dictionary<int, int> callLines, string[] referencedFiles, int[] breaks,
			int[] continues, bool requiresScope)
		{
			if (pCode == null) throw new ArgumentNullException(nameof(pCode));
			if (callLines == null) throw new ArgumentNullException(nameof(callLines));
			if (referencedFiles == null) throw new ArgumentNullException(nameof(referencedFiles));
			if (breaks == null) throw new ArgumentNullException(nameof(breaks));
			if (continues == null) throw new ArgumentNullException(nameof(continues));

			PCode = pCode;
			CallLines = callLines;
			Breaks = breaks;
			Continues = continues;
			RequiresScope = requiresScope;
			ReferencedFiles = referencedFiles;
		}

		public byte[] PCode { get; }

		public Dictionary<int, int> CallLines { get; }

		public int[] Breaks { get; }

		public int[] Continues { get; }

		public bool RequiresScope { get; }

		public string[] ReferencedFiles { get; }

		public int Length => PCode.Length;

		public static CompiledBlock Empty { get; } = new CompiledBlock(new byte[0], new Dictionary<int, int>(), new string[0],
			new int[0], new int[0], false);
	}
}