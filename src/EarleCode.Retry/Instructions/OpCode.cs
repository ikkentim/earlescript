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

namespace EarleCode.Retry.Instructions
{
    public enum OpCode : byte
    {
        // Call to value on stack.
        [OpCode("CALL")]
        Call,
        // Write value on stack to var reference on stack.
        [OpCode("WRITE")]
        Write,
        // Read value from var reference on stack to stack.
        [OpCode("READ")]
        Read,
        // Push specified int value.
        [OpCode("PUSH_INT $int")]
        PushInteger,
        // Push specified float value.
        [OpCode("PUSH_FLOAT $float")]
        PushFloat,
        // Push specified string value.
        [OpCode("PUSH_STR $string")]
        PushString,
        // Push name reference of function or variable
        [OpCode("PUSH_REF $string")]
        PushReference,
        // Push null value
        [OpCode("PUSH_NULL")]
        PushNull,
        // Pop value from stack
        [OpCode("POP")]
        Pop,
        // Add a scope to the scopes stack
        [OpCode("PUSHS")]
        PushScope,
        // Pop a scope from the scopes stack
        [OpCode("POPS")]
        PopScope,
        // Binary invert
        [OpCode("NOT")]
        Not,
        // Subtract values
        [OpCode("SUB")]
        Subtract,
        // Add values
        [OpCode("ADD")]
        Add,
        // Multiply
        [OpCode("MUL")]
        Multiply,
        // Jump if true is on stack
        [OpCode("JUMP_IF $int")]
        JumpIf,
        // Jump
        [OpCode("JUMP $int")]
        Jump,
        // Return
        [OpCode("RET")]
        Return,
    }
}