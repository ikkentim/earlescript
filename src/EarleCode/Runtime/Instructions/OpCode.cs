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

namespace EarleCode.Runtime.Instructions
{
    /// <summary>
    ///     All OpCodes which can be handled by the <see cref="EarleRuntime" />.
    /// </summary>
    public enum OpCode : byte
    {
        [OpCode("NOP", typeof(NopInstuction))]
        Nop,
        /// <summary>
        ///     Call the function referenced by the value below the top of the stack and with the value on the top of the stack as a 
        ///     target with the specified number of arguments.
        /// </summary>
        [OpCode("CALL $int", typeof(CallInstruction))]
        Call,

        /// <summary>
        ///     Call the function referenced by the value on the top of the stack with the specified number of arguments.
        /// </summary>
        [OpCode("CALL.T $int", typeof(CallWithoutTargetInstruction))]
        CallNoTarget,

        /// <summary>
        ///     Spawn a new thread and call the function referenced by the value below the top of the stack and
        ///     with the value on the top of the stack as a target with the specified number of arguments.
        /// </summary>
        [OpCode("THREAD $int", typeof(ThreadInstruction))]
        Thread,

        /// <summary>
        ///     Spawn a new thread and call the function referenced by the value on the top of the stack with the
        ///     specified number of arguments.
        /// </summary>
        [OpCode("THREAD.T $int", typeof(ThreadWithoutTargetInstruction))]
        ThreadNoTarget,

        /// <summary>
        ///     Write the value under the top of the stack to the variable referenced by the top of the stack.
        /// </summary>
        [OpCode("WRITE", typeof (WriteInstruction))]
        Write,

        /// <summary>
        ///     Read the value from the variable referenced by the top of the stack.
        /// </summary>
        [OpCode("READ", typeof (ReadInstruction))]
        Read,

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
        ///     Push the specified reference to a function or variable to the stack.
        /// </summary>
        [OpCode("PUSH.R $string", typeof (PushReferenceInstruction))]
        PushReference,

        /// <summary>
        ///     Push a null value to the stack.
        /// </summary>
        [OpCode("PUSH.N", typeof (PushNullInstruction))]
        PushUndefined,

        [OpCode("PUSH.A", typeof(PushArrayInstruction))]
        PushArray,

        [OpCode("PUSH.1", typeof(PushOneInstruction))]
        PushOne,

        /// <summary>
        ///     Pop a value off the stack.
        /// </summary>
        [OpCode("POP", typeof (PopInstruction))]
        Pop,

        /// <summary>
        ///     Push a new scope onto the scopes stack.
        /// </summary>
        [OpCode("PUSH.S", typeof (PushScopeInstruction))]
        PushScope,

        /// <summary>
        ///     Pop a scope off the scopes stack.
        /// </summary>
        [OpCode("POP.S", typeof (PopScopeInstruction))]
        PopScope,

        /// <summary>
        ///     Jump the specified number of instructions relative to the next instruction if the value on the top of the stack is
        ///     false.
        /// </summary>
        [OpCode("JUMP.F $int", typeof (JumpIfFalseInstruction))]
        JumpIfFalse,

        /// <summary>
        ///     Jump the specified number of instructions relative to the next instruction if the value on the top of the stack is
        ///     true.
        /// </summary>
        [OpCode("JUMP.T $int", typeof (JumpIfTrueInstruction))]
        JumpIfTrue,

        /// <summary>
        ///     Jump the specified number of instructions relative to the next instruction,
        /// </summary>
        [OpCode("JUMP $int", typeof (JumpInstruction))]
        Jump,

        /// <summary>
        ///     Move the CIP to the end of the P-code.
        /// </summary>
        [OpCode("RET", typeof (ReturnInstruction))]
        Return,

        /// <summary>
        ///     Duplicates the top value on the stack.
        /// </summary>
        [OpCode("DUP", typeof (DuplicateInstruction))]
        Duplicate,

        /// <summary>
        ///     Dereference the specified argument from the structure on the top of the stack.
        /// </summary>
        [OpCode("DEREF.F $string", typeof (DereferenceFieldInstruction))]
        DereferenceField,

        /// <summary>
        ///     Dereference the specified argument from the structure on the top of the stack.
        /// </summary>
        [OpCode("DEREF.I", typeof(DereferenceIndexInstruction))]
        DereferenceIndex,

        [OpCode("ADD", typeof(BinaryOperatorInstruction))]
        [Operator("+", OperatorType.BinaryOperator | OperatorType.AssignmentModOperator | OperatorType.AssignmentOperator, 7)]
        Add,
        [OpCode("SUB", typeof(BinaryOperatorInstruction))]
        [Operator("-", OperatorType.BinaryOperator | OperatorType.AssignmentModOperator | OperatorType.AssignmentOperator, 7)]
        Subtract,
        [OpCode("MUL", typeof(BinaryOperatorInstruction))]
        [Operator("*", OperatorType.BinaryOperator, 8)]
        Multiply,
        [OpCode("MOD", typeof(BinaryOperatorInstruction))]
        [Operator("%", OperatorType.BinaryOperator, 8)]
        Modulo,
        [OpCode("DIV", typeof(BinaryOperatorInstruction))]
        [Operator("/", OperatorType.BinaryOperator, 8)]
        Divide,
        [OpCode("XOR", typeof(BinaryOperatorInstruction))]
        [Operator("^", OperatorType.BinaryOperator, 2)]
        BitwiseXor,
        [OpCode("OR", typeof(BinaryOperatorInstruction))]
        [Operator("|", OperatorType.BinaryOperator, 1)]
        BitwiseOr,
        [OpCode("AND", typeof(BinaryOperatorInstruction))]
        [Operator("&", OperatorType.BinaryOperator, 3)]
        BitwiseAnd,
        [OpCode("SHL", typeof(BinaryOperatorInstruction))]
        [Operator("<<", OperatorType.BinaryOperator, 6)]
        ShiftLeft,
        [OpCode("SHR", typeof(BinaryOperatorInstruction))]
        [Operator(">>", OperatorType.BinaryOperator, 6)]
        ShiftRight,
        [OpCode("CLT", typeof(BinaryOperatorInstruction))]
        [Operator("<", OperatorType.BinaryOperator, 5)]
        CheckLessThan,
        [OpCode("CGT", typeof(BinaryOperatorInstruction))]
        [Operator(">", OperatorType.BinaryOperator, 5)]
        CheckGreaterThan,
        [OpCode("CLEQ", typeof(BinaryOperatorInstruction))]
        [Operator("<=", OperatorType.BinaryOperator, 5)]
        CheckLessOrEqual,
        [OpCode("CGEQ", typeof(BinaryOperatorInstruction))]
        [Operator(">=", OperatorType.BinaryOperator, 5)]
        CheckGreaterOrEqual,
        [OpCode("CEQ", typeof(BinaryOperatorInstruction))]
        [Operator("==", OperatorType.BinaryOperator, 4)]
        CheckEqual,
        [OpCode("CNEQ", typeof(BinaryOperatorInstruction))]
        [Operator("!=", OperatorType.BinaryOperator, 4)]
        CheckNotEqual,
        [OpCode("NEG", typeof(UnaryOperatorInstruction))]
        [Operator("-", OperatorType.UnaryOperator)]
        Negate,

        /// <summary>
        ///     Replace the value on top of the stack with its logical NOT value.
        /// </summary>
        [OpCode("NOT.L", typeof(UnaryOperatorInstruction))]
        [Operator("!", OperatorType.UnaryOperator)]
        LogicalNot,

        [OpCode("NOT.B", typeof(UnaryOperatorInstruction))]
        [Operator("~", OperatorType.UnaryOperator)]
        BitwiseNot,
        [OpCode("CONV", typeof(UnaryOperatorInstruction))]
        [Operator("@", OperatorType.UnaryOperator)]
        Convert,
    }
}