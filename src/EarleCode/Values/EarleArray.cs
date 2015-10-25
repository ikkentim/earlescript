using System;

namespace EarleCode.Values
{
    public class EarleArray : EarleVariable
    {
        private EarleVariable[] _data;

        public EarleArray() : this(0)
        {
            
        }

        public EarleArray(EarleValue[] values) : this(values?.Length ?? 0)
        {
            if (values != null)
                Array.Copy(values, _data, values.Length);
        }

        public EarleArray(int length)
        {
            if(length < 0) throw new ArgumentOutOfRangeException(nameof(length));
            _data = new EarleVariable[length];
        }

        public EarleVariable Get(EarleValue index)
        {
            var i = (int)index;
            
            return i < 0 || _data.Length >= i ? null : _data[i];
        }
        
        private static int NextSize(int previousSize)
        {
            var size = 1;
            while (size <= previousSize) size <<= 1;
            return size;
        }

        public void Set(EarleValue index, EarleValue value)
        {
            var i = (int)index;
            if (i < 0) throw new ArgumentOutOfRangeException(nameof(index));

            if (_data.Length <= i)
                Array.Resize(ref _data, NextSize(i));
        }
    }
}