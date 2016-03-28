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

using System;
using System.Collections.Generic;
using System.Linq;
using EarleCode.Values;
using EarleCode.Values.ValueTypes;

namespace EarleCode
{
    public class Runtime : RuntimeScope
    {
        private readonly Dictionary<string, EarleFile> _files = new Dictionary<string, EarleFile>();
        private readonly Dictionary<string, EarleFunctionCollection> _natives = new Dictionary<string, EarleFunctionCollection>();
        private readonly Dictionary<Type, IEarleValueType> _valueTypes = new Dictionary<Type, IEarleValueType>();

        public Runtime() : base(null)
        {
            Compiler = new Compiler(this);

            RegisterNative(new NativeFunction("print", values =>
            {
                Console.WriteLine(values.FirstOrDefault().Value);
                return EarleValue.Null;
            }, "value"));
            RegisterNative(new NativeFunction("operator*", values =>
            {
                var supportedTypes = new[] {typeof (int), typeof (float)};

                var left = values[0];
                var right = values[1];

                left.AssertOfType(supportedTypes);
                right.AssertOfType(supportedTypes);

                return left.Is<float>() || right.Is<float>()
                    ? new EarleValue(left.To<float>(this)*right.To<float>(this))
                    : new EarleValue(left.To<int>(this)*right.To<int>(this));
            }, "left", "right"));
            RegisterNative(new NativeFunction("operator+", values =>
            {
                var supportedTypes = new[] {typeof (int), typeof (float), typeof (string)};

                var left = values[0];
                var right = values[1];

                left.AssertOfType(supportedTypes);
                right.AssertOfType(supportedTypes);

                if (left.Is<string>() || right.Is<string>())
                    return (new EarleValue(left.To<string>(this) + right.To<string>(this)));
                if (left.Is<float>() || right.Is<float>())
                    return (new EarleValue(left.To<float>(this) + right.To<float>(this)));
                return (new EarleValue(left.To<int>(this) + right.To<int>(this)));
            }, "left", "right"));
            RegisterNative(new NativeFunction("operator-", values =>
            {
                var supportedTypes = new[] {typeof (int), typeof (float)};

                var left = values[0];
                var right = values[1];

                left.AssertOfType(supportedTypes);
                right.AssertOfType(supportedTypes);

                return left.Is<float>() || right.Is<float>()
                    ? new EarleValue(left.To<float>(this) - right.To<float>(this))
                    : new EarleValue(left.To<int>(this) - right.To<int>(this));
            }, "left", "right"));

            RegisterValueType(new EarleIntegerValueType());
            RegisterValueType(new EarleFloatValueType());
            RegisterValueType(new EarleBoolValueType());
            RegisterValueType(new EarleStringValueType());
        }

        public Compiler Compiler { get; }

        #region Natives

        public void RegisterNative(NativeFunction native)
        {
            if (native == null) throw new ArgumentNullException(nameof(native));

            EarleFunctionCollection collection;
            if(!_natives.TryGetValue(native.Name, out collection))
                _natives[native.Name] = collection = new EarleFunctionCollection();

            collection.Add(native);
        }

        #endregion

        #region Running

        private void RunLoop(RuntimeLoop loop)
        {
            // TODO: Store loop if execution did not complete.
            loop.Run();
        }

        #endregion

        #region Value types

        public void RegisterValueType(IEarleValueType valueType)
        {
            if (valueType == null) throw new ArgumentNullException(nameof(valueType));
            _valueTypes[valueType.Type] = valueType;
        }

        public IEarleValueType GetValueTypeForType(Type type)
        {
            IEarleValueType valueType;
            return _valueTypes.TryGetValue(type, out valueType) ? valueType : null;
        }

        #endregion

        #region Files

        public void AddFile(EarleFile file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            _files[file.Name] = file;
        }

        public EarleFile CompileFile(string fileName, string script)
        {
            var file = Compiler.CompileFile(fileName, script);

            AddFile(file);
            return file;
        }

        public EarleFile GetFile(string fileName)
        {
            EarleFile file;
            _files.TryGetValue(fileName, out file);
            return file;
        }

        #endregion

        #region Invoking

        public void Invoke(EarleFunction function)
        {
            Invoke(function, null);
        }

        public void Invoke(EarleFunction function, IEnumerable<EarleValue> arguments)
        {
            if (function == null) throw new ArgumentNullException(nameof(function));
            RunLoop(function.CreateLoop(this, arguments?.ToArray() ?? new EarleValue[0]));
        }
        
        #endregion

        #region Overrides of RuntimeScope

        public override EarleValue? GetValue(EarleVariableReference reference)
        {
            if (!string.IsNullOrEmpty(reference.File))
                return GetFile(reference.File)?.GetFunctions(reference.Name)?.ToEarleValue();

            EarleFunctionCollection natives;
            if (_natives.TryGetValue(reference.Name, out natives))
                return new EarleValue(natives);

            // TODO: Check global variables

            return null;
        }

        protected override bool CanAssignReferenceAsLocal(EarleVariableReference reference)
        {
            return false;
        }

        #endregion
    }
}