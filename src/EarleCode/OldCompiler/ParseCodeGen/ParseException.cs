﻿// EarleCode
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
using EarleCode.OldCompiler.Lexing;

namespace EarleCode.OldCompiler.ParseCodeGen
{
	public class ParseException : Exception
	{
		public ParseException(Token token, string error)
			: this(token?.File ?? string.Empty, token?.Line ?? -1, token?.Column ?? -1, error)
		{
		}

		public ParseException(string file, int line, int column, string error)
			: this(string.Format("{3}:{0}:{1}: {2}", line, column, error, file))
		{
		}

		public ParseException(string message)
			: base(message)
		{
		}
	}
}