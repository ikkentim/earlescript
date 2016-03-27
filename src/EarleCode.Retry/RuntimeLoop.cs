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
using EarleCode.Retry.Instructions;
using EarleCode.Retry.Utilities;

namespace EarleCode.Retry
{
    public class RuntimeLoop : IRuntimeScope
    {
        private readonly byte[] _pCode;
        private readonly Runtime _runtime;
        private readonly Stack<RuntimeScope> _scopes = new Stack<RuntimeScope>();
        private readonly Stack<EarleValue> _stack;
        private int _cip;
        private RuntimeLoop _subLoop;

        public RuntimeLoop(Runtime runtime, RuntimeScope superScope, byte[] instructions)
            : this(runtime, superScope, instructions, null)
        {
        }

        public RuntimeLoop(Runtime runtime, RuntimeScope superScope, byte[] instructions,
            IDictionary<string, EarleValue> initialLocals)
        {
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));
            _runtime = runtime;
            _pCode = instructions;
            _stack = new Stack<EarleValue>();

            _scopes.Push(new RuntimeScope(superScope, initialLocals));
        }

        public virtual EarleValue? GetValue(EarleVariableReference reference)
        {
            return _scopes.Peek().GetValue(reference);
        }

        public virtual bool SetValue(EarleVariableReference reference, EarleValue value)
        {
            return _scopes.Peek().SetValue(reference, value);
        }

        public virtual EarleValue? Run()
        {
            // If a value is returned, loop is complete, if null is returned, the loop has not yet been completed.

            //
            if (_subLoop != null)
            {
                var result = _subLoop.Run();
                if (result != null)
                {
                    _stack.Push(result.Value);
                    _subLoop = null;
                }
                else
                {
                    return null;
                }
            }

            for (; _cip < _pCode.Length;)
            {
                var instruction = _pCode[_cip++];

                // debug
//                var ins = (OpCode) instruction;
//                var cpi = _cip - 1;
//                var runop = ins.GetAttributeOfType<OpCodeAttribute>().BuildString(_pCode, ref cpi);
//                Console.WriteLine($"RUN OP: {(OpCode) instruction}: {runop}");

                switch ((OpCode) instruction)
                {
                    case OpCode.Call:
                    {
                        if (_subLoop == null)
                        {
                            var functionName = _stack.Pop();
                            functionName.AssertOfType(EarleValueType.Reference);
                            var functionReference = (EarleVariableReference) functionName.Value;
                            var function = GetValue(functionReference)?.Value as EarleFunction;

                            if (function == null)
                            {
                                throw new Exception($"unknown function {functionName}");
                            }

                            _subLoop = function.CreateLoop(_runtime, _stack);
                        }
                        
                        var result = _subLoop.Run();
                        if (result == null)
                        {
                            return null;
                        }

                        _subLoop = null;
                        _stack.Push(result.Value);
                        break;
                    }
                    case OpCode.Write:
                    {
                        var variableName = _stack.Pop();
                        variableName.AssertOfType(EarleValueType.Reference);
                        var variableReference = (EarleVariableReference) variableName.Value;
                        SetValue(variableReference, _stack.Pop());
                        break;
                    }
                    case OpCode.Read:
                    {
                        var variableName = _stack.Pop();
                        variableName.AssertOfType(EarleValueType.Reference);
                        var variableReference = (EarleVariableReference) variableName.Value;
                        _stack.Push(GetValue(variableReference) ?? new EarleValue());
                        break;
                    }
                    case OpCode.PushInteger:
                    {
                        var value = BitConverter.ToInt32(_pCode, _cip);
                        _cip += 4;
                        _stack.Push(new EarleValue(value));
                        break;
                    }
                    case OpCode.PushFloat:
                    {
                        var value = BitConverter.ToSingle(_pCode, _cip);
                        _cip += 4;
                        _stack.Push(new EarleValue(value));
                        break;
                    }
                    case OpCode.PushString:
                    {
                        var value = "";

                        while (_pCode[_cip] != 0)
                            value += (char) _pCode[_cip++];
                        _cip++;

                        _stack.Push(new EarleValue(value));
                        break;
                    }
                    case OpCode.PushReference:
                    {
                        var refString = "";

                        while (_pCode[_cip] != 0)
                            refString += (char) _pCode[_cip++];
                        _cip++;

                        string file = null,
                            name;

                        if (refString.Contains("::"))
                        {
                            var spl = refString.Split(new[] {"::"}, StringSplitOptions.None);
                            file = spl[0];
                            name = spl[1];
                        }
                        else
                            name = refString;

                        _stack.Push(new EarleValue(new EarleVariableReference(file.Length == 0 ? null : file, name)));
                        break;
                    }
                    case OpCode.PushNull:
                    {
                        _stack.Push(new EarleValue());
                        break;
                    }
                    case OpCode.Pop:
                    {
                        _stack.Pop();
                        break;
                    }
                    case OpCode.PushScope:
                    {
                        var super = _scopes.Peek();
                        _scopes.Push(new RuntimeScope(super));
                        break;
                    }
                    case OpCode.PopScope:
                    {
                        _scopes.Pop();
                        break;
                    }
                    case OpCode.Not:
                    {
                        var value = _stack.Pop();

                        switch (value.Type)
                        {
                            case EarleValueType.Integer:
                                value = value.As<int>() == 0 ? new EarleValue(1) : new EarleValue(0);
                                break;
                            default:
                                throw new Exception("Invalid type to .Not");
                        }

                        _stack.Push(value);
                        break;
                    }
                    case OpCode.Subtract:
                    {
                        var right = _stack.Pop();
                        var left = _stack.Pop();

                        var supportedTypes = new[] {EarleValueType.Integer, EarleValueType.Float};

                        if (!supportedTypes.Contains(right.Type))
                            throw new Exception("Unsupported right value type");

                        if (!supportedTypes.Contains(left.Type))
                            throw new Exception("Unsupported left value type");

                        if (left.Is<float>() || right.Is<float>())
                            _stack.Push(new EarleValue(left.To<float>() - right.To<float>()));
                        else
                            _stack.Push(new EarleValue(left.To<int>() - right.To<int>()));

                        break;
                    }
                    case OpCode.Add:
                    {
                        var right = _stack.Pop();
                        var left = _stack.Pop();

                        var supportedTypes = new[] {EarleValueType.Integer, EarleValueType.Float, EarleValueType.String};

                        if (!supportedTypes.Contains(right.Type))
                            throw new Exception("Unsupported right value type");

                        if (!supportedTypes.Contains(left.Type))
                            throw new Exception("Unsupported left value type");

                        if (left.Is<string>() || right.Is<string>())
                            _stack.Push(new EarleValue(left.To<string>() + right.To<string>()));
                        else if (left.Is<float>() || right.Is<float>())
                            _stack.Push(new EarleValue(left.To<float>() + right.To<float>()));
                        else
                            _stack.Push(new EarleValue(left.To<int>() + right.To<int>()));

                        break;
                    }
                    case OpCode.Multiply:
                    {
                        var right = _stack.Pop();
                        var left = _stack.Pop();

                        var supportedTypes = new[] {EarleValueType.Integer, EarleValueType.Float};

                        if (!supportedTypes.Contains(right.Type))
                            throw new Exception("Unsupported right value type");

                        if (!supportedTypes.Contains(left.Type))
                            throw new Exception("Unsupported left value type");

                        if (left.Is<float>() || right.Is<float>())
                            _stack.Push(new EarleValue(left.To<float>()*right.To<float>()));
                        else
                            _stack.Push(new EarleValue(left.To<int>()*right.To<int>()));

                        break;
                    }
                    case OpCode.JumpIf:
                    {
                        if (!_stack.Pop().To<bool>())
                        {
                            var value = BitConverter.ToInt32(_pCode, _cip);
                            _cip += value;
                        }

                        _cip += 4;
                        break;
                    }
                    case OpCode.Jump:
                    {
                        var value = BitConverter.ToInt32(_pCode, _cip);
                        _cip += 4 + value;
                        break;
                    }
                    case OpCode.Return:
                    {
                        _cip = _pCode.Length;
                        break;
                    }
                    default:
                        throw new Exception("Unkown opcode " + (OpCode) instruction);
                }
            }

            return _stack.Pop();
        }
    }
}