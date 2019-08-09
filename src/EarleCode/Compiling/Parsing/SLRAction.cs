using EarleCode.Compiling.Parsing.Grammars.Productions;

namespace EarleCode.Compiling.Parsing
{
    /// <summary>
    /// Represents an action of an <see cref="SLRParsingTable"/>.
    /// </summary>
    public struct SLRAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SLRAction"/> struct.
        /// </summary>
        /// <param name="value">The value of the action.</param>
        /// <param name="reduce">The reduced production rule. This value is only used when <paramref name="type"/> is set to <see cref="SLRActionType.Reduce"/>.</param>
        /// <param name="type">The type of the action.</param>
        private SLRAction(int value, ProductionRule reduce, SLRActionType type)
        {
            Value = value;
            Reduce = reduce;
            Type = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SLRAction"/> struct.
        /// </summary>
        /// <param name="reduce">The reduced production rule.</param>
        public SLRAction(ProductionRule reduce) : this(0, reduce, SLRActionType.Reduce)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SLRAction"/> struct.
        /// </summary>
        /// <param name="value">The value of the action.</param>
        /// <param name="type">The type of the action.</param>
        public SLRAction(int value, SLRActionType type) : this(value, null, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SLRAction"/> struct.
        /// </summary>
        /// <param name="type">The type of the action.</param>
        public SLRAction(SLRActionType type) : this(0, null, type)
        {
        }

        /// <summary>
        /// An error action.
        /// </summary>
        public static SLRAction Error { get; } = new SLRAction(SLRActionType.Error);

        /// <summary>
        /// The value of the action.
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// The reduced production rule.
        /// </summary>
        /// <remarks>This value is only used when <see cref="Type"/> is set to <see cref="SLRActionType.Reduce"/>.</remarks>
        public ProductionRule Reduce { get; }

        /// <summary>
        /// The type of the action.
        /// </summary>
        public SLRActionType Type { get; }

        #region Overrides of ValueType

        /// <summary>Returns the fully qualified type name of this instance.</summary>
        /// <returns>The fully qualified type name.</returns>
        public override string ToString()
        {
            switch (Type)
            {
                case SLRActionType.Shift: return "SHIFT " + Value;
                case SLRActionType.Reduce: return "REDUCE " + Reduce;
                default: return base.ToString();
            }
        }

        #endregion
    }
}