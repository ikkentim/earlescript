using System;
using System.Collections.Generic;
using EarleCode.Retry.Instructions;

namespace EarleCode.Retry
{
    public class RuntimeLoop
    {
        private readonly Runtime _runtime;
        private RuntimeLoop _subLoop;
        private readonly byte[] _pCode;
        private readonly Stack<EarleValue> _stack;
        private readonly Stack<RuntimeScope> _scopes = new Stack<RuntimeScope>();
        private int _cip;

        public RuntimeLoop(Runtime runtime, RuntimeScope superScope, byte[] instructions) : this(runtime, superScope, instructions, null)
        {
        }

        public RuntimeLoop(Runtime runtime, RuntimeScope superScope, byte[] instructions, IDictionary<string,EarleValue> initialLocals)
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
            
            for (; _cip < _pCode.Length; )
            {
                var instruction = _pCode[_cip++];

                Console.WriteLine($"RUN OP: {(OpCode)instruction}");
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
                        
                        _stack.Push(result.Value);
                        break;
                    }
                    case OpCode.Assign:
                    {
                        var variableName = _stack.Pop();
                        variableName.AssertOfType(EarleValueType.Reference);
                        var variableReference = (EarleVariableReference)variableName.Value;
                        var value = _stack.Pop();
                        SetValue(variableReference, value);
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

                        _stack.Push(new EarleValue(new EarleVariableReference(file, name)));
                        break;
                    }
                    case OpCode.PushString:
                        {
                            var value = "";

                            while (_pCode[_cip] != 0)
                                value += (char)_pCode[_cip++];
                            _cip++;

                            _stack.Push(new EarleValue(value));
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
                    default:
                        throw new Exception("Unkown opcode " + (OpCode)instruction);
                }
            }

            return _stack.Pop();
        }
    }
}