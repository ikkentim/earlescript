// EarleCode
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

using EarleCode.Blocks;

namespace EarleCode
{
    public class OperatorIncompleteInvocationResult : IncompleteInvocationResult
    {
        public OperatorIncompleteInvocationResult(IEarleContext context, IncompleteInvocationResult innerResult,
            IExpression expression1, IExpression expression2, int stage)
            : base(context, innerResult, stage, null)
        {
            Expression1 = expression1;
            Expression2 = expression2;
        }

        public OperatorIncompleteInvocationResult(IEarleContext context, IncompleteInvocationResult innerResult,
            IExpression expression1, IExpression expression2, EarleValue value1, int stage)
            : base(context, innerResult, stage,null)
        {
            Expression1 = expression1;
            Expression2 = expression2;
            Value1 = value1;
        }

        public IExpression Expression1 { get; }
        public EarleValue Value1 { get; set; }
        public IExpression Expression2 { get; }
    }
}