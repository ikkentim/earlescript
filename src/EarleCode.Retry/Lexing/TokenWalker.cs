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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EarleCode.Retry.Lexing
{
    public class TokenWalker : IEnumerator<Token>
    {
        private readonly Stack<Stack<Token>> _sessions = new Stack<Stack<Token>>();

        public TokenWalker(ILexer lexer)
        {
            if (lexer == null) throw new ArgumentNullException(nameof(lexer));
            Lexer = lexer;

            CreateSession();
        }
        
        public ILexer Lexer { get; }

        #region Implementation of IEnumerator

        public bool MoveNext()
        {
            _sessions.Peek().Push(Lexer.Current);

            var r = Lexer.MoveNext();

            return r;
        }

        public void Reset()
        {
            _sessions.Clear();
            Lexer.Reset();
        }

        public Token Current => Lexer.Current;

        object IEnumerator.Current => Current;

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            Lexer.Dispose();
        }

        #endregion

        public void CreateSession()
        {
            _sessions.Push(new Stack<Token>());
        }

        public void DropSession()
        {
            var session = _sessions.Pop();

            while (session.Count > 0)
                Lexer.Push(session.Pop());

            if (_sessions.Count == 0)
                CreateSession();
        }

        public void FlushSession()
        {
            if (_sessions.Count <= 1) return;

            var session = _sessions.Pop();

            foreach (var item in session.Reverse())
                _sessions.Peek().Push(item);
        }

        public bool FlushOrDropSession(bool flush)
        {
            if (flush) FlushSession();
            else DropSession();

            return flush;
        }

        public void DropAllSessions()
        {
            while (_sessions.Count > 0)
            {
                var session = _sessions.Pop();
                while (session.Count > 0)
                    Lexer.Push(session.Pop());
            }

            CreateSession();
        }
    }
}