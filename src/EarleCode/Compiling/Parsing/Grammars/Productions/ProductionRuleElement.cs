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

using EarleCode.Compiling.Lexing;

namespace EarleCode.Compiling.Parsing.Grammars.Productions
{
    public struct ProductionRuleElement
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProductionRuleElement" /> class.
        /// </summary>
        /// <param name="tokenType">Type of the token.</param>
        /// <param name="value">The value.</param>
        public ProductionRuleElement(TokenType tokenType, string value) : this(ProductionRuleElementType.Terminal,
            tokenType, value)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProductionRuleElement" /> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="tokenType">Type of the token.</param>
        /// <param name="value">The value.</param>
        public ProductionRuleElement(ProductionRuleElementType type, TokenType tokenType, string value)
        {
            Type = type;
            TokenType = tokenType;
            Value = value;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProductionRuleElement" /> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        public ProductionRuleElement(ProductionRuleElementType type, string value)
        {
            Type = type;
            Value = value;
	        TokenType = default(TokenType);
        }
		
        /// <summary>
        ///     Gets or sets the type of this element.
        /// </summary>
        public ProductionRuleElementType Type { get; }

        /// <summary>
        ///     Gets or sets the type of the token in this element.
        /// </summary>
        public TokenType TokenType { get; }

        /// <summary>
        ///     Gets or sets the value of this element.
        /// </summary>
        public string Value { get; }

	    /// <summary>
	    ///     Gets the token of this element.
	    /// </summary>
	    public Terminal Terminal
	    {
		    get
		    {
			    switch (Type)
			    {
					case ProductionRuleElementType.Terminal:
						return Value == null && TokenType == TokenType.Symbol ? Terminal.EndOfFile : new Terminal(TokenType, Value);
					default:
						throw new GrammarException("A NonTerminal element does not have a token");
			    }
		    }
	    }
		
        #region Overrides of Object

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            switch (Type)
            {
                case ProductionRuleElementType.NonTerminal:
                    return Value;
                case ProductionRuleElementType.Terminal:
                    return $"{Terminal}";
                default:
                    return "???";
            }
        }

        #endregion
    }
}