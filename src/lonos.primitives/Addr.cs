using System.Runtime.Versioning;
using System;

namespace lonos
{
    [Serializable]
    public struct Addr
    {
        public static readonly Addr Invalid = new Addr(0xFFFFFFFE);
        public static readonly Addr Zero;

        public static unsafe int Size
        {
            [NonVersionable]
            get
            {
                return sizeof(void*);
            }
        }

        [NonVersionable]
        public unsafe Addr(uint value)
        {
            _value = (void*)value;
        }

        [NonVersionable]
        public unsafe Addr(ulong value)
        {
            _value = (void*)((uint)value);
        }

        [NonVersionable]
        public unsafe Addr(void* value)
        {
            _value = value;
        }

        [NonVersionable]
        public unsafe Addr(IntPtr value)
        {
            _value = (void*)value;
        }

        [NonVersionable]
        public unsafe Addr(UIntPtr value)
        {
            _value = (void*)value;
        }

        [NonVersionable]
        public static Addr Add(Addr pointer, int offset)
        {
            return pointer + offset;
        }

        [NonVersionable]
        public static implicit operator Addr(uint value)
        {
            return new Addr(value);
        }

        [NonVersionable]
        public static implicit operator Addr(ulong value)
        {
            return new Addr(value);
        }

        [NonVersionable]
        public static implicit operator Addr(IntPtr value)
        {
            return new Addr(value);
        }

        [NonVersionable]
        public static implicit operator Addr(UIntPtr value)
        {
            return new Addr(value);
        }

        [NonVersionable]
        public static unsafe implicit operator Addr(void* value)
        {
            return new Addr(value);
        }

        [NonVersionable]
        public static unsafe implicit operator IntPtr(Addr value)
        {
            return (IntPtr)value._value;
        }

        [NonVersionable]
        public static unsafe implicit operator uint(Addr value)
        {
            return (uint)value._value;
        }

        [NonVersionable]
        public static unsafe implicit operator UIntPtr(Addr value)
        {
            return (UIntPtr)value._value;
        }

        [NonVersionable]
        public static unsafe implicit operator ulong(Addr value)
        {
            return (ulong)value._value;
        }

        [NonVersionable]
        public static unsafe implicit operator void*(Addr value)
        {
            return value._value;
        }

        [NonVersionable]
        public static unsafe Addr operator -(Addr pointer, int offset)
        {
            return new Addr((ulong)((long)pointer._value - offset));
        }

        [NonVersionable]
        public static unsafe bool operator !=(Addr value1, Addr value2)
        {
            return value1._value != value2._value;
        }

        [NonVersionable]
        public static unsafe Addr operator +(Addr pointer, int offset)
        {
            return new Addr((ulong)((long)pointer._value + offset));
        }

        [NonVersionable]
        public static unsafe bool operator ==(Addr value1, Addr value2)
        {
            return value1._value == value2._value;
        }

        [NonVersionable]
        public static Addr Subtract(Addr pointer, int offset)
        {
            return pointer - offset;
        }

        public unsafe override bool Equals(Object obj)
        {
            if (obj is Addr)
            {
                return (_value == ((Addr)obj)._value);
            }
            return false;
        }

        public unsafe override int GetHashCode()
        {
            return ((int)_value);
        }

        [NonVersionable]
        public unsafe void* ToPointer()
        {
            return _value;
        }

        public unsafe override string ToString()
        {
            return ((long)_value).ToString();
        }

        [NonVersionable]
        public unsafe uint ToUInt32()
        {
            return ((uint)_value);
        }

        [NonVersionable]
        public unsafe ulong ToUInt64()
        {
            return (ulong)_value;
        }

        private unsafe void* _value; // Do not rename (binary serialization)
    }
}
