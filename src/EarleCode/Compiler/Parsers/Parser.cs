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
using System.Text;
using EarleCode.Compiler.Lexing;
using EarleCode.Runtime;
using EarleCode.Runtime.Instructions;
using EarleCode.Runtime.Values;
using EarleCode.Utilities;

namespace EarleCode.Compiler.Parsers
{
    internal abstract class Parser : IParser
    {
        private readonly string[] ReservedKeywords = {
            "thread",
            "true",
            "false",
            "undefined"
        };

        private readonly List<byte> _result = new List<byte>();
        private Dictionary<int, int> _callLines;
        private readonly List<string> _usedFiles = new List<string>();
        private readonly List<int> _breaks = new List<int>();
        private readonly List<int> _continues = new List<int>();
        private EarleCompileOptions _enforcedCompileOptions;

        protected EarleRuntime Runtime { get; private set; }

        protected EarleFile File { get; private set; }

        protected ILexer Lexer { get; private set; }

        protected virtual bool RequiresScope { get; }

        #region Implementation of IParser

        public CompiledBlock Parse(EarleRuntime runtime, EarleFile file, ILexer lexer, EarleCompileOptions enforcedCompileOptions)
        {
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (lexer == null) throw new ArgumentNullException(nameof(lexer));
            Runtime = runtime;
            File = file;
            Lexer = lexer;
            _enforcedCompileOptions = enforcedCompileOptions;

            _result.Clear();
            _callLines = new Dictionary<int, int>();
            _usedFiles.Clear();
            _breaks.Clear();
            _continues.Clear();

            var startLine = lexer.Current.Line;

            Parse();

            return new CompiledBlock(_result.ToArray(), _callLines, _usedFiles.ToArray(), _breaks.ToArray(), _continues.ToArray(), RequiresScope);
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
            return $"{GetType().Name} {{Lexer = \"{Lexer}\"}}";
        }

        #endregion

        #region Yield

        public void Yield(byte value)
        {
            _result.Add(value);
        }

        public void Yield(OpCode value)
        {
            Yield((byte)value);
        }

        private void Yield(byte[] values)
        {
            if(values == null) throw new ArgumentNullException(nameof(values));
            _result.AddRange(values);
        }

        public void Yield(CompiledBlock block, bool breaks = false, int breakOffset = 0, bool continues = false, int continueOffset = 0)
        {
            if(block == null) throw new ArgumentNullException(nameof(block));

            var startIndex = _result.Count;

            _result.AddRange(block.PCode);
            foreach(var p in block.CallLines)
                _callLines.Add(p.Key + startIndex, p.Value);

            _usedFiles.AddRange(block.ReferencedFiles.Where(f => !_usedFiles.Contains(f)));

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

        public void Yield(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            Yield(Encoding.ASCII.GetBytes(value));
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

        #region Read/write

        public void PushRead(string variableName)
        {
            if(variableName == null) throw new ArgumentNullException(nameof(variableName));

            if(ReservedKeywords.Contains(variableName))
                ThrowParseException($"Cannot read from reserved keyword `{variableName}`");
            
            Yield(OpCode.Read);
            Yield(variableName);
        }

        public void PushRead(string variableName, CompiledBlock dereferenceBuffer)
        {
            PushRead(variableName);
            if(dereferenceBuffer != null)
                Yield(dereferenceBuffer);
        }

        public void PushWrite(string variableName)
        {
            if(variableName == null) throw new ArgumentNullException(nameof(variableName));

            if(ReservedKeywords.Contains(variableName))
                ThrowParseException($"Cannot write to reserved keyword `{variableName}`");
            
            Yield(OpCode.Write);
            Yield(variableName);
        }

        public void PushWrite(string variableName, CompiledBlock dereferenceBuffer)
        {
            if(variableName == null) throw new ArgumentNullException(nameof(variableName));

            if(ReservedKeywords.Contains(variableName))
                ThrowParseException($"Cannot write to reserved keyword `{variableName}`");
            
            if(dereferenceBuffer == null || dereferenceBuffer.PCode.Length == 0)
            {
                PushWrite(variableName);
            }
            else
            {
                PushRead(variableName);
                Yield(dereferenceBuffer);
            }
        }

        #endregion

        #region Push call

        public void PushFunctionReference(string path, string name)
        {
            if(name == null) throw new ArgumentNullException(nameof(name));

            if(!string.IsNullOrEmpty(path) && !_usedFiles.Contains(path))
                _usedFiles.Add(path);

            Yield(OpCode.PushFunctionReference);
            Yield($"{path}::{name}".ToLower());
        }

        public void PushCall(int arguments, int lineNumber, bool thread = false)
        {
            _callLines[_result.Count] = lineNumber;
            Yield(thread ? OpCode.Thread : OpCode.Call);
            Yield(arguments);
        }

        public void PushCallWithoutTarget(int arguments, int lineNumber, bool thread = false)
        {
            _callLines[_result.Count] = lineNumber;
            Yield(thread ? OpCode.ThreadNoTarget : OpCode.CallNoTarget);
            Yield(arguments);
        }

        public void PushCallWithoutTarget(string path, string name, int arguments, int lineNumber)
        {
            if(name == null) throw new ArgumentNullException(nameof(name));

            PushFunctionReference(path, name);
            PushCallWithoutTarget(arguments, lineNumber);
        }

        #endregion

        #region Push jump

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

        #endregion

        #region Push value

        public void PushString(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            Yield(OpCode.PushValue);
            Yield(File.GetIndexForValueInStore(value.ToEarleValue()));

            //Yield(OpCode.PushString);
            //Yield(value);
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

        #region Parse

        public int Parse<T>(EarleCompileOptions compileOptions = EarleCompileOptions.None) where T : IParser
        {
            var block = ParseToBuffer<T>(compileOptions);
            Yield(block);
            return block.PCode.Length;
        }

        public CompiledBlock ParseToBuffer<T>(EarleCompileOptions compileOptions = EarleCompileOptions.None) where T : IParser
        {
            var parser = Activator.CreateInstance<T>();
            return parser.Parse(Runtime, File, Lexer, _enforcedCompileOptions | compileOptions);
        }

        #endregion

        #region Syntax matches

        public bool SyntaxMatches(string rule)
        {
            return Runtime.Compiler.SyntaxGrammarProcessor.IsMatch(Lexer, rule);
        }

        public void AssertSyntaxMatches(string rule)
        {
            if(!SyntaxMatches(rule))
                ThrowUnexpectedTokenWithExpected(rule.ToLower());
        }

        #endregion

        #region Compile block

        public CompiledBlock CompileBlock(EarleCompileOptions compileOptions)
        {
            return Runtime.Compiler.Compile(Lexer, File, _enforcedCompileOptions | compileOptions);
        }

        #endregion
    }
}