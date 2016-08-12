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
using System.Linq;
using System.Reflection;

namespace EarleCode.Runtime.Attributes
{
    /// <summary>
    ///     Represents a scanner which will seek for members with certain attributes in order for them to load them into the
    ///     <see cref="Runtime" />.
    /// </summary>
    public class EarleAttributeScanner
    {
        protected const BindingFlags AnyStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        protected const BindingFlags AnyInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EarleAttributeScanner" /> class.
        /// </summary>
        /// <param name="runtime">The runtime.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <see cref="runtime" /> is null</exception>
        public EarleAttributeScanner(EarleRuntime runtime)
        {
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));

            Runtime = runtime;
        }

        /// <summary>
        ///     Gets the runtime.
        /// </summary>
        public EarleRuntime Runtime { get; }

        /// <summary>
        ///     Scans the specified <see cref="scope" /> in the specified <typeparamref name="T" /> type.
        /// </summary>
        /// <typeparam name="T">The type to scan in.</typeparam>
        /// <param name="scope">The scope to scan in.</param>
        public void Scan<T>(EarleAttributeScanScope scope = EarleAttributeScanScope.Class) where T : class
        {
            Scan(typeof (T), scope);
        }

        /// <summary>
        ///     Scans the specified <see cref="scope" /> in the specified <see cref="type" />.
        /// </summary>
        /// <param name="type">The type to scan in.</param>
        /// <param name="scope">The scope to scan in.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <see cref="type" /> is null.</exception>
        public void Scan(Type type, EarleAttributeScanScope scope = EarleAttributeScanScope.Class)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            switch (scope)
            {
                case EarleAttributeScanScope.Class:
                    ScanType(type);
                    break;
                case EarleAttributeScanScope.Assembly:
                    ScanAssembly(type.Assembly);
                    break;
            }
        }

        /// <summary>
        ///     Scans the specified <see cref="assembly" />.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <see cref="assembly" /> is null.</exception>
        protected void ScanAssembly(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            foreach (var type in assembly.GetTypes())
                ScanType(type);
        }

        /// <summary>
        ///     Scans the specified <see cref="type" />.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <see cref="type" /> is null.</exception>
        protected virtual void ScanType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            ScanMethodsForAttribute<EarleNativeFunctionAttribute>(type, AnyStatic, true, OnStaticNativeFound);
        }

        /// <summary>
        ///     Called when a static native method was found.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="attribute">The attribute.</param>
        protected virtual void OnStaticNativeFound(MethodInfo method, EarleNativeFunctionAttribute attribute)
        {
            var name = attribute.Name?.ToLower() ?? method.Name.ToLower();

            Runtime.Natives.Register(EarleNativeFunction.Create(name, null, method));
        }

        /// <summary>
        ///     Scans the methods for the specified <typeparamref name="TAttribute" /> attribute type.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="type">The type.</param>
        /// <param name="bindingAttr">The binding attribute.</param>
        /// <param name="inherit">if set to <c>true</c> also look for inherited attributes.</param>
        /// <param name="onResult">The result handler.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <see cref="type" /> or <see cref="onResult" /> is null.</exception>
        protected void ScanMethodsForAttribute<TAttribute>(Type type, BindingFlags bindingAttr, bool inherit,
            Action<MethodInfo, TAttribute> onResult) where TAttribute : Attribute
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (onResult == null) throw new ArgumentNullException(nameof(onResult));

            foreach (var method in type.GetMethods(bindingAttr))
            {
                var attributes = method.GetCustomAttributes(typeof (TAttribute), inherit);
                var attribute = attributes.FirstOrDefault() as TAttribute;
                if (attribute != null)
                {
                    onResult(method, attribute);
                }
            }
        }

        /// <summary>
        ///     Scans the properties for the specified <typeparamref name="TAttribute" /> attribute type.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="type">The type.</param>
        /// <param name="bindingAttr">The binding attribute.</param>
        /// <param name="inherit">if set to <c>true</c> also look for inherited attributes.</param>
        /// <param name="onResult">The result handler.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <see cref="type" /> or <see cref="onResult" /> is null.</exception>
        protected void ScanPropertiesForAttribute<TAttribute>(Type type, BindingFlags bindingAttr, bool inherit,
            Action<PropertyInfo, TAttribute> onResult) where TAttribute : Attribute
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            foreach (var property in type.GetProperties(bindingAttr))
            {
                var attribute = property.GetCustomAttributes(typeof (TAttribute), inherit) as TAttribute;
                if (attribute != null)
                {
                    onResult(property, attribute);
                }
            }
        }
    }
}