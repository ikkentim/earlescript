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

        protected Runtime Runtime { get; private set; }

        protected EarleFile File { get; private set; }

        protected ILexer Lexer { get; private set; }

        #region Implementation of IParser

        public IEnumerable<byte> Parse(Runtime runtime, EarleFile file, ILexer lexer)
        {
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (lexer == null) throw new ArgumentNullException(nameof(lexer));
            Runtime = runtime;
            File = file;
            Lexer = lexer;
            _result.Clear();

            Parse();

            return _result.ToArray();
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

        public void Yield(IEnumerable<byte> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
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

        public void PushCall(string path, string name, int arguments)
        {
            PushReference(path, name);
            Yield(OpCode.Call);
            Yield(arguments);
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

        public int Parse<T>() where T : IParser
        {
            var buffer = ParseToBuffer<T>();
            Yield(buffer);
            return buffer.Length;
        }

        public byte[] ParseToBuffer<T>() where T : IParser
        {
            var parser = Activator.CreateInstance<T>();
            return parser.Parse(Runtime, File, Lexer).ToArray();
        }

        public bool SyntaxMatches(string rule)
        {
            return Runtime.Compiler.SyntaxGrammarProcessor.Matches(Lexer, rule);
        }

        #endregion
    }
}