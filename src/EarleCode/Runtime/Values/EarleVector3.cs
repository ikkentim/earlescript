﻿// EarleCode
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

namespace EarleCode.Runtime.Values
{
    public class EarleVector3 : IEarleStructure
    {
        public EarleVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float X { get; }

        public float Y { get; }

        public float Z { get; }

        #region Implementation of IEarleStructure

        public EarleValue GetField(string name)
        {
            switch(name)
            {
                case "x":
                    return (EarleValue)X;
                case "y":
                    return (EarleValue)Y;
                case "z":
                    return (EarleValue)Z;
                default:
                    return EarleValue.Undefined;
            }
        }

        public void SetField(string name, EarleValue value)
        {
            // Values cannot be set.
        }

        #endregion

        #region Overrides of Object

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return $"({X}, {Y}, {Z})";
        }

        #endregion
    }
}