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
using Earle.Tokens;

namespace Earle
{
    public class CodeException : Exception
    {
        public CodeException(Token token, string error)
            : this(token == null ? -1 : token.Line, token == null ? -1 : token.Column, error)
        {
        }

        public CodeException(int line, int column, string error)
            : base(string.Format("ERROR:{0}:{1}: {2}", line, column, error))
        {
        }

        public CodeException(string message)
            : base(message)
        {
        }
    }
}