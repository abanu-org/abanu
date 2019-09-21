// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Runtime.Versioning;

namespace Lonos
{
    [Serializable]
    public struct USize
    {
        public static readonly USize Zero;

        public static unsafe int Size
        {
            [NonVersionable]
            get
            {
                return sizeof(void*);
            }
        }

        [NonVersionable]
        public unsafe USize(uint value)
        {
            _value = (void*)value;
        }

        [NonVersionable]
        public unsafe USize(int value)
        {
            _value = (void*)value;
        }

        [NonVersionable]
        public unsafe USize(ulong value)
        {
            _value = (void*)(uint)value;
        }

        [NonVersionable]
        public unsafe USize(long value)
        {
            _value = (void*)value;
        }

        [NonVersionable]
        public unsafe USize(void* value)
        {
            _value = value;
        }

        [NonVersionable]
        public static USize Add(USize pointer, int offset)
        {
            return pointer + offset;
        }

        [NonVersionable]
        public static unsafe implicit operator uint(USize value)
        {
            return (uint)value._value;
        }

        [NonVersionable]
        public static unsafe implicit operator ulong(USize value)
        {
            return (ulong)value._value;
        }

        [NonVersionable]
        public static implicit operator USize(uint value)
        {
            return new USize(value);
        }

        [NonVersionable]
        public static implicit operator USize(ulong value)
        {
            return new USize(value);
        }

        [NonVersionable]
#pragma warning disable CA2225 // Operator overloads have named alternates
        public static unsafe implicit operator USize(void* value)
#pragma warning restore CA2225 // Operator overloads have named alternates
        {
            return new USize(value);
        }

        [NonVersionable]
#pragma warning disable CA2225 // Operator overloads have named alternates
        public static unsafe implicit operator void*(USize value)
#pragma warning restore CA2225 // Operator overloads have named alternates
        {
            return value._value;
        }

        [NonVersionable]
        public static unsafe USize operator -(USize pointer, int offset)
        {
            return new USize((ulong)((long)pointer._value - offset));
        }

        [NonVersionable]
        public static unsafe bool operator !=(USize value1, USize value2)
        {
            return value1._value != value2._value;
        }

        [NonVersionable]
        public static unsafe USize operator +(USize pointer, int offset)
        {
            return new USize((ulong)((long)pointer._value + offset));
        }

        [NonVersionable]
        public static unsafe bool operator ==(USize value1, USize value2)
        {
            return value1._value == value2._value;
        }

        [NonVersionable]
        public static USize Subtract(USize pointer, int offset)
        {
            return pointer - offset;
        }

        public unsafe override bool Equals(object obj)
        {
            if (obj is USize)
            {
                return _value == ((USize)obj)._value;
            }
            return false;
        }

        public unsafe override int GetHashCode()
        {
            return (int)_value;
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
            return (uint)_value;
        }

        [NonVersionable]
        public unsafe ulong ToUInt64()
        {
            return (ulong)_value;
        }

        private unsafe void* _value; // Do not rename (binary serialization)

        public static USize FromUInt32(uint value) => new USize(value);
        public static USize FromInt32(int value) => new USize(value);
        public static USize FromUInt64(ulong value) => new USize(value);
        public static USize FromInt64(long value) => new USize(value);

        public unsafe int ToInt32() => (int)_value;
        public unsafe long ToInt64() => (long)_value;
    }
}
