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
    public class EarleAttributeScanner
    {
        protected const BindingFlags AnyStatic = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        protected const BindingFlags AnyInstance = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        public EarleAttributeScanner(EarleRuntime runtime)
        {
            if (runtime == null) throw new ArgumentNullException(nameof(runtime));

            Runtime = runtime;
        }

        public EarleRuntime Runtime { get; set; }

        public void Scan<T>(EarleAttributeScanScope scope = EarleAttributeScanScope.Class) where T : class
        {
            Scan(typeof (T), scope);
        }

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

        protected void ScanAssembly(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            foreach (var type in assembly.GetTypes())
                ScanType(type);
        }

        protected virtual void ScanType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            ScanMethodsForAttribute<EarleNativeFunctionAttribute>(type, AnyStatic, true, OnNativeFound);
        }

        protected virtual void OnNativeFound(MethodInfo method, EarleNativeFunctionAttribute attribute)
        {
            var name = attribute.Name?.ToLower() ?? method.Name.ToLower();

            Runtime.Natives.Register(EarleNativeFunction.Create(name, null, method));
        }

        protected void ScanMethodsForAttribute<TAttribute>(Type type, BindingFlags bindingAttr, bool inherit,
            Action<MethodInfo, TAttribute> onResult) where TAttribute : Attribute
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

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