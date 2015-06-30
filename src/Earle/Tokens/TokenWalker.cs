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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Earle.Tokens
{
    public class TokenWalker : IEnumerator<Token>
    {
        private readonly Stack<Stack<Token>> _sessions = new Stack<Stack<Token>>();

        public TokenWalker(Tokenizer tokenizer)
        {
            if (tokenizer == null) throw new ArgumentNullException("tokenizer");
            Tokenizer = tokenizer;

            CreateSession();
        }

        public int SessionTokenCount
        {
            get
            {
                return _sessions.Sum(s => s.Count);
            }
        }

        public Tokenizer Tokenizer { get; private set; }

        #region Implementation of IDisposable

        public void Dispose()
        {
            Tokenizer.Dispose();
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
                Tokenizer.Push(session.Pop());

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
                    Tokenizer.Push(session.Pop());
            }

            CreateSession();
        }

        #region Implementation of IEnumerator

        public bool MoveNext()
        {
            _sessions.Peek().Push(Tokenizer.Current);

            var r = Tokenizer.MoveNext();

            return r;
        }

        public void Reset()
        {
            Tokenizer.Reset();
        }

        public Token Current
        {
            get { return Tokenizer.Current; }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        #endregion
    }
}