// EarleCode
// Copyright 2016 Parkitect, Tim Potze

using System;
using System.Collections.Generic;
using System.Linq;
using EarleCode.Retry.Instructions;
using EarleCode.Retry.Lexing;

namespace EarleCode.Retry.Parsers
{
    public abstract class ExtendedParser<T> : IParser where T : IParser, new()
    {
        private readonly T _parent;

        protected ExtendedParser()
        {
            _parent = Activator.CreateInstance<T>();
        }

        protected ExtendedParser(T parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            _parent = parent;
        }

        #region Implementation of IParser

        public IEnumerable<byte> Parse(Runtime runtime, Compiler compiler, EarleFile file, ILexer lexer)
        {
            return
                _parent.Parse(runtime, compiler, file, lexer).Concat(ParseMore(runtime, compiler, file, lexer));
        }

        #endregion

        protected abstract IEnumerable<byte> ParseMore(Runtime runtime, Compiler compiler, EarleFile file,
            ILexer lexer);
    }
}