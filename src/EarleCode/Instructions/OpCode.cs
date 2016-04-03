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

namespace EarleCode.Instructions
{
    /// <summary>
    ///     All OpCodes which can be handled by the <see cref="Runtime" />.
    /// </summary>
    public enum OpCode : byte
    {
        /// <summary>
        ///     Call the function referenced by the value on top of the stack with the specified number of arguments.
        /// </summary>
        [OpCode("CALL $int", typeof (CallInstruction))] Call,

        /// <summary>
        ///     Write the value under the top of the stack to the variable referenced by the top of the stack.
        /// </summary>
        [OpCode("WRITE", typeof (WriteInstruction))] Write,

        /// <summary>
        ///     Read the value from the variable referenced by the top of the stack.
        /// </summary>
        [OpCode("READ", typeof (ReadInstruction))] Read,

        /// <summary>
        ///     Push the specified int to the stack.
        /// </summary>
        [OpCode("PUSH_INT $int", typeof (PushIntegerInstruction))] PushInteger,

        /// <summary>
        ///     Push the specified float to the stack.
        /// </summary>
        [OpCode("PUSH_FLOAT $float", typeof (PushFloatInstruction))] PushFloat,

        /// <summary>
        ///     Push the specified string to the stack.
        /// </summary>
        [OpCode("PUSH_STR $string", typeof (PushStringInstruction))] PushString,

        /// <summary>
        ///     Push the specified reference to a function or variable to the stack.
        /// </summary>
        [OpCode("PUSH_REF $string", typeof (PushReferenceInstruction))] PushReference,

        /// <summary>
        ///     Push a null value to the stack.
        /// </summary>
        [OpCode("PUSH_NULL", typeof (PushNullInstruction))] PushUndefined,

        /// <summary>
        ///     Pop a value off the stack.
        /// </summary>
        [OpCode("POP", typeof (PopInstruction))] Pop,

        /// <summary>
        ///     Push a new scope onto the scopes stack.
        /// </summary>
        [OpCode("PUSHS", typeof (PushScopeInstruction))] PushScope,

        /// <summary>
        ///     Pop a scope off the scopes stack.
        /// </summary>
        [OpCode("POPS", typeof (PopScopeInstruction))] PopScope,
        // Binary invert
        /// <summary>
        ///     Replace the value on top of the stack with its logical NOT value.
        /// </summary>
        [OpCode("NOT", typeof (NotInstruction))] Not,

        /// <summary>
        ///     Jump the specified number of instructions relative to the next instruction if the value on the top of the stack is
        ///     false.
        /// </summary>
        [OpCode("JUMP_FALSE $int", typeof(JumpIfFalseInstruction))]
        JumpIfFalse,

        /// <summary>
        ///     Jump the specified number of instructions relative to the next instruction if the value on the top of the stack is
        ///     true.
        /// </summary>
        [OpCode("JUMP_TRUE $int", typeof(JumpIfTrueInstruction))]
        JumpIfTrue,

        /// <summary>
        ///     Jump the specified number of instructions relative to the next instruction,
        /// </summary>
        [OpCode("JUMP $int", typeof (JumpInstruction))] Jump,

        /// <summary>
        ///     Move the CIP to the end of the P-code.
        /// </summary>
        [OpCode("RET", typeof (ReturnInstruction))] Return,

        /// <summary>
        ///     Duplicates the top value on the stack.
        /// </summary>
        [OpCode("DUP", typeof(DuplicateInstruction))]Duplicate
    }
}