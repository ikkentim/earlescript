using System;
namespace EarleCode.Utilities
{
    public static class ArrayUtility
    {
        public static T[] Repeat<T>(T value, int count)
        {
            T[] array = new T[count];
            for(var i = 0; i < count; i++)
                array[i] = value;

            return array;
        }
    }
}

