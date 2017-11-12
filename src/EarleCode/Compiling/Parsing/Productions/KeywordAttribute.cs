// EarleCode
// Copyright 2017 Tim Potze
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

namespace EarleCode.Compiling.Parsing.Productions
{
    /// <summary>
    ///     Indicates an enum value of a token type is the type used for keywords found using the
    ///     <see cref="EnumProductionRuleSet{TProductionRulesEnum,TTokenType}" />.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class KeywordAttribute : Attribute
    {
    }
}