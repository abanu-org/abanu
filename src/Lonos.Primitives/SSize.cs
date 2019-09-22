// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Runtime.Versioning;

namespace Lonos
{
    [Serializable]
    public struct SSize
    {
        private unsafe void* _value; // Do not rename (binary serialization)

        public static readonly SSize Zero;

        [NonVersionable]
        public unsafe SSize(uint value)
        {
            _value = (void*)value;
        }

        [NonVersionable]
        public unsafe SSize(int value)
        {
            _value = (void*)value;
        }

        [NonVersionable]
        public unsafe SSize(ulong value)
        {
            _value = (void*)(uint)value;
        }

        [NonVersionable]
        public unsafe SSize(long value)
        {
            _value = (void*)value;
        }

        [NonVersionable]
        public unsafe SSize(void* value)
        {
            _value = value;
        }

        public unsafe override bool Equals(object obj)
        {
            if (obj is SSize)
            {
                return _value == ((SSize)obj)._value;
            }
            return false;
        }

        public unsafe override int GetHashCode()
        {
            return (int)_value;
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

        [NonVersionable]
        public static implicit operator SSize(uint value)
        {
            return new SSize(value);
        }

        [NonVersionable]
        public static implicit operator SSize(ulong value)
        {
            return new SSize(value);
        }

        [NonVersionable]
        public static implicit operator SSize(int value)
        {
            return new SSize(value);
        }

        [NonVersionable]
        public static implicit operator SSize(long value)
        {
            return new SSize(value);
        }

        [NonVersionable]
#pragma warning disable CA2225 // Operator overloads have named alternates
        public static unsafe implicit operator SSize(void* value)
#pragma warning restore CA2225 // Operator overloads have named alternates
        {
            return new SSize(value);
        }

        [NonVersionable]
#pragma warning disable CA2225 // Operator overloads have named alternates
        public static unsafe implicit operator void*(SSize value)
#pragma warning restore CA2225 // Operator overloads have named alternates
        {
            return value._value;
        }

        [NonVersionable]
        public static unsafe implicit operator uint(SSize value)
        {
            return (uint)value._value;
        }

        [NonVersionable]
        public static unsafe implicit operator ulong(SSize value)
        {
            return (ulong)value._value;
        }

        [NonVersionable]
        public static unsafe bool operator ==(SSize value1, SSize value2)
        {
            return value1._value == value2._value;
        }

        [NonVersionable]
        public static unsafe bool operator !=(SSize value1, SSize value2)
        {
            return value1._value != value2._value;
        }

        [NonVersionable]
        public static SSize Add(SSize pointer, int offset)
        {
            return pointer + offset;
        }

        [NonVersionable]
        public static unsafe SSize operator +(SSize pointer, int offset)
        {
            return new SSize((ulong)((long)pointer._value + offset));
        }

        [NonVersionable]
        public static SSize Subtract(SSize pointer, int offset)
        {
            return pointer - offset;
        }

        [NonVersionable]
        public static unsafe SSize operator -(SSize pointer, int offset)
        {
            return new SSize((ulong)((long)pointer._value - offset));
        }

        public static unsafe int Size
        {
            [NonVersionable]
            get
            {
                return sizeof(void*);
            }
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

        public static SSize FromUInt32(uint value) => new SSize(value);
        public static SSize FromInt32(int value) => new SSize(value);
        public static SSize FromUInt64(ulong value) => new SSize(value);
        public static SSize FromInt64(long value) => new SSize(value);

        public unsafe int ToInt32() => (int)_value;
        public unsafe long ToInt64() => (long)_value;
    }
}
