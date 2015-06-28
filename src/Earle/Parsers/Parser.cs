// Earle
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
using Earle.Blocks;
using Earle.Tokens;

namespace Earle.Parsers
{
    public abstract class Parser<T> : IParser where T : Block
    {
        protected Parser(bool requiresBlock, string parserRule)
        {
            RequiresBlock = requiresBlock;
            ParserRule = parserRule;
        }

        public virtual string ParserRule { get; private set; }
        public virtual bool RequiresBlock { get; private set; }

        Block IParser.Parse(Block parent, Tokenizer tokenizer)
        {
            return Parse(parent, tokenizer);
        }

        public abstract T Parse(Block parent, Tokenizer tokenizer);

        #region Tokenizer Utilities

        protected void MoveNext(Tokenizer tokenizer)
        {
            if (!tokenizer.MoveNext())
                throw new ParseException("Unexpected end of file");
        }

        protected void AssertToken(Tokenizer tokenizer, params TokenType[] types)
        {
            if (!types.Contains(tokenizer.Current.Type))
                throw new ParseException(tokenizer.Current, "Unexpected token");
        }

        protected void SkipToken(Tokenizer tokenizer, params TokenType[] types)
        {
            if (!types.Contains(tokenizer.Current.Type))
                throw new ParseException(tokenizer.Current, "Unexpected token");

            MoveNext(tokenizer);
        }

        protected void SkipToken(Tokenizer tokenizer, string value, params TokenType[] types)
        {
            if (!types.Contains(tokenizer.Current.Type) || tokenizer.Current.Value != value)
                throw new ParseException(tokenizer.Current, "Unexpected token");

            MoveNext(tokenizer);
        }

        #endregion
    }
}