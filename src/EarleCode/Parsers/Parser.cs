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
using System.Diagnostics;
using System.Linq;
using EarleCode.Instructions;
using EarleCode.Lexing;
using EarleCode.Utilities;

namespace EarleCode.Parsers
{
    public abstract class Parser : IParser
    {
        private readonly List<byte> _result = new List<byte>();
        private readonly List<int> _breaks = new List<int>();
        private readonly List<int> _continues = new List<int>();
        private bool _canBreak;
        private bool _canContinue;
        protected Runtime Runtime { get; private set; }

        protected EarleFile File { get; private set; }

        protected ILexer Lexer { get; private set; }

        #region Implementation of IParser

        public CompiledBlock Parse(Runtime runtime, EarleFile file, ILexer lexer, bool canBreak, bool canContinue)
        {
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (lexer == null) throw new ArgumentNullException(nameof(lexer));
            Runtime = runtime;
            File = file;
            Lexer = lexer;
            _canBreak = canBreak;
            _canContinue = canContinue;

            _result.Clear();
            _breaks.Clear();
            _continues.Clear();

            Parse();

            return new CompiledBlock(_result.ToArray(), _breaks.ToArray(), _continues.ToArray());
        }

        #endregion

        protected abstract void Parse();

        #region Overrides of Object

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return $@"{GetType().Name} {{Lexer = ""{Lexer}""}}";
        }

        #endregion

        #region Yield

        public void Yield(byte value)
        {
            _result.Add(value);
        }

        public void Yield(CompiledBlock block, bool breaks = false, int breakOffset = 0, bool continues = false, int continueOffset = 0)
        {
            if(block == null) throw new ArgumentNullException(nameof(block));

            int startIndex = _result.Count;
            _result.AddRange(block.PCode);

            if(breaks)
            {
                foreach(var b in block.Breaks)
                {
                    var jump = block.Length - b - 5 + breakOffset;
                    var bytes = BitConverter.GetBytes(jump);
                    for(var i = 0; i < bytes.Length; i++)
                        _result[startIndex + b + 1 + i] = bytes[i];
                }
            }
            else
            {
                _breaks.AddRange(block.Breaks.Select(b => b + startIndex));
            }

            if(continues)
            {
                foreach(var c in block.Continues)
                {
                    var jump = block.Length - c - 5 + continueOffset;
                    var bytes = BitConverter.GetBytes(jump);
                    for(var i = 0; i < bytes.Length; i++)
                        _result[startIndex + c + 1 + i] = bytes[i];
                }
            }
            else
            {
                _continues.AddRange(block.Continues.Select(c => c + startIndex));
            }
        }

        private void Yield(byte[] values)
        {
            if(values == null) throw new ArgumentNullException(nameof(values));
            _result.AddRange(values);
        }

        public void Yield(OpCode value)
        {
            Yield((byte) value);
        }

        public void Yield(char value)
        {
            Yield((byte) value);
        }

        public void Yield(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            foreach (var c in value)
                Yield(c);

            Yield((byte) 0);
        }

        public void Yield(int value)
        {
            Yield(BitConverter.GetBytes(value));
        }

        public void Yield(float value)
        {
            Yield(BitConverter.GetBytes(value));
        }

        #endregion

        #region Push

        public void PushReference(string path, string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            Yield(OpCode.PushReference);
            Yield($"{path}::{name}".ToLower());
        }

        public void PushCall(int arguments)
        {
            Yield(OpCode.Call);
            Yield(arguments);
        }

        public void PushCallWithoutTarget(int arguments)
        {
            Yield(OpCode.CallNoTarget);
            Yield(arguments);
        }

        public void PushCallWithoutTarget(string path, string name, int arguments)
        {
            if(name == null) throw new ArgumentNullException(nameof(name));

            PushReference(path, name);
            PushCallWithoutTarget(arguments);
        }

        public void PushJump(bool condition, int count)
        {
            Yield(condition ? OpCode.JumpIfTrue : OpCode.JumpIfFalse);
            Yield(count);
        }

        public void PushJump(int count)
        {
            Yield(OpCode.Jump);
            Yield(count);
        }

        public void PushDereferenceField(string field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            Yield(OpCode.DereferenceField);
            Yield(field);
        }

        public void PushString(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            Yield(OpCode.PushString);
            Yield(value);
        }

        public void PushFloat(float value)
        {
            Yield(OpCode.PushFloat);
            Yield(value);
        }

        public void PushInteger(int value)
        {
            Yield(OpCode.PushInteger);
            Yield(value);
        }

        #endregion

        #region Exception Helpers

        [DebuggerHidden]
        public void ThrowParseException(string error)
        {
            throw new ParseException(Lexer.Current, error);
        }

        [DebuggerHidden]
        public void ThrowUnexpectedToken()
        {
            ThrowParseException($"Unexpected token \"{Lexer.Current.Value}\"");
        }

        [DebuggerHidden]
        public void ThrowUnexpectedTokenWithExpected(string expectedString)
        {
            ThrowParseException($"Expected {expectedString}, but found \"{Lexer.Current.Value}\"");
        }

        [DebuggerHidden]
        public void ThrowUnexpectedToken(params TokenType[] expectedTypes)
        {
            ThrowUnexpectedTokenWithExpected(expectedTypes.Select(v => $"\"-{v.ToUpperString()}-\"").CreateOrList());
        }

        [DebuggerHidden]
        public void ThrowUnexpectedToken(params string[] expectedValues)
        {
            ThrowUnexpectedTokenWithExpected(expectedValues.Select(v => $"\"{v}\"").CreateOrList());
        }

        #endregion

        #region Misc Helpers

        public int Parse<T>(bool canBreak = false, bool canContinue = false) where T : IParser
        {
            var block = ParseToBuffer<T>(canBreak, canContinue);
            Yield(block);
            return block.PCode.Length;
        }

        public CompiledBlock ParseToBuffer<T>(bool canBreak = false, bool canContinue = false) where T : IParser
        {
            var parser = Activator.CreateInstance<T>();
            return parser.Parse(Runtime, File, Lexer, _canBreak || canBreak, _canContinue || canContinue);
        }

        public bool SyntaxMatches(string rule)
        {
            return Runtime.Compiler.SyntaxGrammarProcessor.IsMatch(Lexer, rule);
        }

        public CompiledBlock CompileBlock(bool canBreak = false, bool canContinue = false)
        {
            return Runtime.Compiler.Compile(Lexer, File, false, canBreak || _canBreak, canContinue || _canContinue);
        }
        #endregion
    }
}