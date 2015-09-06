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

using System.Linq;
using EarleCode.Parsers;

namespace EarleCode.Tokens
{
    public static class TokenizerUtils
    {
        public static void AssertMoveNext(this ITokenizer tokenizer)
        {
            if (!tokenizer.MoveNext())
                throw new ParseException("Unexpected end of file");
        }

        public static void AssertToken(this ITokenizer tokenizer, params TokenType[] types)
        {
            if (!types.Contains(tokenizer.Current.Type))
                throw new ParseException(tokenizer.Current, "Unexpected token");
        }

        public static void AssertToken(this ITokenizer tokenizer, string value, params TokenType[] types)
        {
            if (!types.Contains(tokenizer.Current.Type) || tokenizer.Current.Value != value)
                throw new ParseException(tokenizer.Current, "Unexpected token");
        }

        public static void SkipToken(this ITokenizer tokenizer, string value, params TokenType[] types)
        {
            AssertToken(tokenizer, value, types);
            AssertMoveNext(tokenizer);
        }

        public static void SkipToken(this ITokenizer tokenizer, params TokenType[] types)
        {
            AssertToken(tokenizer, types);
            AssertMoveNext(tokenizer);
        }
    }
}