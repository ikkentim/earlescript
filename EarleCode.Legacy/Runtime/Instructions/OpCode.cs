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

using EarleCode.Runtime.Operators;

namespace EarleCode.Runtime.Instructions
{
    /// <summary>
    ///     All operation codes which can be handled by the <see cref="EarleRuntime" />.
    /// </summary>
    public enum OpCode : byte
    {
        /// <summary>
        ///     Do nothing.
        /// </summary>
        [OpCode("NOP", typeof (NopInstuction))]
        Nop,

        /// <summary>
        ///     Call the function stored on the top of the stack with the target stored below the top of the stack and with the
        ///     specified number of arguments stored below the target.
        /// </summary>
        [OpCode("CALL $int", typeof (CallInstruction))]
        Call,

        /// <summary>
        ///     Call the function stored on the top of the stack with with the specified number of arguments stored below the top
        ///     of the stack.
        /// </summary>
        [OpCode("CALL.T $int", typeof (CallWithoutTargetInstruction))]
        CallNoTarget,

        /// <summary>
        ///     Spawn a new thread and call the function stored on the top of the stack with the target stored below the top of the
        ///     stack and with the specified number of arguments stored below the target.
        /// </summary>
        [OpCode("THREAD $int", typeof (ThreadInstruction))]
        Thread,

        /// <summary>
        ///     Spawn a new thread and call the function stored on the top of the stack with the specified number of arguments
        ///     stored below the top of the stack.
        /// </summary>
        [OpCode("THREAD.T $int", typeof (ThreadWithoutTargetInstruction))]
        ThreadNoTarget,

        /// <summary>
        ///     Write the value on the top of the stack to the specified variable.
        /// </summary>
        [OpCode("WRITE $string", typeof (WriteInstruction))]
        Write,

        /// <summary>
        ///     Read the value from the specified variable.
        /// </summary>
        [OpCode("READ $string", typeof (ReadInstruction))]
        Read,

        /// <summary>
        ///     Push the value from the specified field from the structure on the top of the stack.
        /// </summary>
        [OpCode("READ.F $string", typeof (ReadFieldInstuction))]
        ReadField,

        /// <summary>
        ///     Read the value from the index on the top of the stack in the array stored below the top of the stack.
        /// </summary>
        [OpCode("READ.I", typeof (ReadIndexInstruction))]
        ReadIndex,

        /// <summary>
        ///     Write the value from below the top of the stack to the specified field to the structure on the top of the stack.
        /// </summary>
        [OpCode("WRITE.F $string", typeof (WriteFieldInstruction))]
        WriteField,

        /// <summary>
        ///     Write the value stored below the array at the index specified on the top of the stack in the array stored below the
        ///     index.
        /// </summary>
        [OpCode("WRITE.I", typeof (WriteIndexInstruction))]
        WriteIndex,

        /// <summary>
        ///     Push the function stored in the specified variable to the stack.
        /// </summary>
        [OpCode("UNBOX.C $string", typeof (UnboxFunctionReferenceInstruction))]
        UnboxFunctionReference,

        /// <summary>
        ///     Push the specified int to the stack.
        /// </summary>
        [OpCode("PUSH.I $int", typeof (PushIntegerInstruction))]
        PushInteger,

        /// <summary>
        ///     Push the specified float to the stack.
        /// </summary>
        [OpCode("PUSH.F $float", typeof (PushFloatInstruction))]
        PushFloat,

        /// <summary>
        ///     Push the specified string to the stack.
        /// </summary>
        [OpCode("PUSH.S $string", typeof (PushStringInstruction))]
        PushString,

        /// <summary>
        ///     Push the specified function to the stack.
        /// </summary>
        [OpCode("PUSH.C $string", typeof (PushFunctionInstruction))]
        PushFunction,

        /// <summary>
        ///     Push a null value to the stack.
        /// </summary>
        [OpCode("PUSH.N", typeof (PushNullInstruction))]
        PushUndefined,

        /// <summary>
        ///     Push a new array to the stack.
        /// </summary>
        [OpCode("PUSH.A", typeof (PushArrayInstruction))]
        PushArray,

        /// <summary>
        ///     Push the integer '1' to the stack.
        /// </summary>
        [OpCode("PUSH.1", typeof (PushOneInstruction))]
        PushOne,

        /// <summary>
        ///     Push the value at the specified index stored in the file's value store to the stack.
        /// </summary>
        [OpCode("PUSH.V $int", typeof (PushValueInstruction))]
        PushValue,

        /// <summary>
        ///     Pop a value off the stack.
        /// </summary>
        [OpCode("POP", typeof (PopInstruction))]
        Pop,

        /// <summary>
        ///     Duplicate the value on the top of the stack.
        /// </summary>
        [OpCode("DUP", typeof (DuplicateInstruction))]
        Duplicate,

        /// <summary>
        ///     Push a new scope onto the scopes stack.
        /// </summary>
        [OpCode("PUSH.SC", typeof (PushScopeInstruction))]
        PushScope,

        /// <summary>
        ///     Pop a scope off the scopes stack.
        /// </summary>
        [OpCode("POP.SC", typeof (PopScopeInstruction))]
        PopScope,

        /// <summary>
        ///     Move the CIP by the specified number of instructions if the value on the top of the stack is false.
        /// </summary>
        [OpCode("JUMP.F $int", typeof (JumpIfFalseInstruction))]
        JumpIfFalse,

        /// <summary>
        ///     Move the CIP by the specified number of instructions if the value on the top of the stack is true.
        /// </summary>
        [OpCode("JUMP.T $int", typeof (JumpIfTrueInstruction))]
        JumpIfTrue,

        /// <summary>
        ///     Move the CIP by the specified number of instructions.
        /// </summary>
        [OpCode("JUMP $int", typeof (JumpInstruction))]
        Jump,

        /// <summary>
        ///     Move the CIP to the end of the P-code.
        /// </summary>
        [OpCode("RET", typeof (ReturnInstruction))]
        Return,

        /// <summary>
        ///     Run the binary '+'-operator on the two values on the top of the stack.
        /// </summary>
        [OpCode("ADD", typeof (BinaryOperatorInstruction))]
        [Operator("+",
            EarleOperatorType.BinaryOperator | EarleOperatorType.AssignmentModOperator |
            EarleOperatorType.AssignmentOperator, 7)
        ]
        Add,

        /// <summary>
        ///     Run the binary '-'-operator on the two values on the top of the stack.
        /// </summary>
        [OpCode("SUB", typeof (BinaryOperatorInstruction))]
        [Operator("-",
            EarleOperatorType.BinaryOperator | EarleOperatorType.AssignmentModOperator |
            EarleOperatorType.AssignmentOperator, 7)
        ]
        Subtract,

        /// <summary>
        ///     Run the binary '*'-operator on the two values on the top of the stack.
        /// </summary>
        [OpCode("MUL", typeof (BinaryOperatorInstruction))]
        [Operator("*", EarleOperatorType.BinaryOperator | EarleOperatorType.AssignmentOperator, 8)]
        Multiply,

        /// <summary>
        ///     Run the binary '%'-operator on the two values on the top of the stack.
        /// </summary>
        [OpCode("MOD", typeof (BinaryOperatorInstruction))]
        [Operator("%", EarleOperatorType.BinaryOperator | EarleOperatorType.AssignmentOperator, 8)]
        Modulo,

        /// <summary>
        ///     Run the binary '/'-operator on the two values on the top of the stack.
        /// </summary>
        [OpCode("DIV", typeof (BinaryOperatorInstruction))]
        [Operator("/", EarleOperatorType.BinaryOperator | EarleOperatorType.AssignmentOperator, 8)]
        Divide,

        /// <summary>
        ///     Run the binary '^'-operator on the two values on the top of the stack.
        /// </summary>
        [OpCode("XOR", typeof (BinaryOperatorInstruction))]
        [Operator("^", EarleOperatorType.BinaryOperator | EarleOperatorType.AssignmentOperator, 2)]
        BitwiseXor,

        /// <summary>
        ///     Run the binary '|'-operator on the two values on the top of the stack.
        /// </summary>
        [OpCode("OR", typeof (BinaryOperatorInstruction))]
        [Operator("|", EarleOperatorType.BinaryOperator | EarleOperatorType.AssignmentOperator, 1)]
        BitwiseOr,

        /// <summary>
        ///     Run the binary '&amp;'-operator on the two values on the top of the stack.
        /// </summary>
        [OpCode("AND", typeof (BinaryOperatorInstruction))]
        [Operator("&", EarleOperatorType.BinaryOperator | EarleOperatorType.AssignmentOperator, 3)]
        BitwiseAnd,

        /// <summary>
        ///     Run the binary '&lt;&lt;'-operator on the two values on the top of the stack.
        /// </summary>
        [OpCode("SHL", typeof (BinaryOperatorInstruction))]
        [Operator("<<", EarleOperatorType.BinaryOperator | EarleOperatorType.AssignmentOperator, 6)]
        ShiftLeft,

        /// <summary>
        ///     Run the binary '>>'-operator on the two values on the top of the stack.
        /// </summary>
        [OpCode("SHR", typeof (BinaryOperatorInstruction))]
        [Operator(">>", EarleOperatorType.BinaryOperator | EarleOperatorType.AssignmentOperator, 6)]
        ShiftRight,

        /// <summary>
        ///     Run the binary '&lt;'-operator on the two values on the top of the stack.
        /// </summary>
        [OpCode("CLT", typeof (BinaryOperatorInstruction))]
        [Operator("<", EarleOperatorType.BinaryOperator, 5)]
        CheckLessThan,

        /// <summary>
        ///     Run the binary '>'-operator on the two values on the top of the stack.
        /// </summary>
        [OpCode("CGT", typeof (BinaryOperatorInstruction))]
        [Operator(">", EarleOperatorType.BinaryOperator, 5)]
        CheckGreaterThan,

        /// <summary>
        ///     Run the binary '&lt;='-operator on the two values on the top of the stack.
        /// </summary>
        [OpCode("CLEQ", typeof (BinaryOperatorInstruction))]
        [Operator("<=", EarleOperatorType.BinaryOperator, 5)]
        CheckLessOrEqual,

        /// <summary>
        ///     Run the binary '>='-operator on the two values on the top of the stack.
        /// </summary>
        [OpCode("CGEQ", typeof (BinaryOperatorInstruction))]
        [Operator(">=", EarleOperatorType.BinaryOperator, 5)]
        CheckGreaterOrEqual,

        /// <summary>
        ///     Run the binary '=='-operator on the two values on the top of the stack.
        /// </summary>
        [OpCode("CEQ", typeof (BinaryOperatorInstruction))]
        [Operator("==", EarleOperatorType.BinaryOperator, 4)]
        CheckEqual,

        /// <summary>
        ///     Run the binary '!='-operator on the two values on the top of the stack.
        /// </summary>
        [OpCode("CNEQ", typeof (BinaryOperatorInstruction))]
        [Operator("!=", EarleOperatorType.BinaryOperator, 4)]
        CheckNotEqual,

        /// <summary>
        ///     Run the unary '-'-operator on the value on the top of the stack.
        /// </summary>
        [OpCode("NEG", typeof (UnaryOperatorInstruction))]
        [Operator("-", EarleOperatorType.UnaryOperator)]
        Negate,

        /// <summary>
        ///     Run the unary '!'-operator on the value on the top of the stack.
        /// </summary>
        [OpCode("NOT.L", typeof (UnaryOperatorInstruction))]
        [Operator("!", EarleOperatorType.UnaryOperator)]
        LogicalNot,

        /// <summary>
        ///     Run the unary '~'-operator on the value on the top of the stack.
        /// </summary>
        [OpCode("NOT.B", typeof (UnaryOperatorInstruction))]
        [Operator("~", EarleOperatorType.UnaryOperator)]
        BitwiseNot,

        /// <summary>
        ///     Run the unary '@'-operator on the value on the top of the stack.
        /// </summary>
        [OpCode("CONV", typeof (UnaryOperatorInstruction))]
        [Operator("@", EarleOperatorType.UnaryOperator)]
        Convert
    }
}