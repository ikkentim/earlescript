// EarleCode
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
using EarleCode.Blocks;
using EarleCode.Tokens;

namespace EarleCode.Parsers
{
    public static class ParserUtilities
    {
        public static IBlock Delegate<T>(ICompiler compiler, IScriptScope scriptScope, ITokenizer tokenizer)
            where T : IParser
        {
            return Delegate<T, IBlock>(compiler, scriptScope, tokenizer);
        }

        public static T2 Delegate<T, T2>(ICompiler compiler, IScriptScope scriptScope, ITokenizer tokenizer)
            where T : IParser where T2 : IBlock
        {
            var parser = (IParser) Activator.CreateInstance<T>();
            return (T2) parser.Parse(compiler, scriptScope, tokenizer);
        }
    }
}