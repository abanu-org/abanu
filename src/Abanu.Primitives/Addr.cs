// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

namespace Abanu
{
    [Serializable]
    public struct Addr
    {
        public static readonly Addr Invalid = new Addr(0xFFFFFFFE);
        public static readonly Addr Zero;

        private unsafe void* _value; // Do not rename (binary serialization)

        public static unsafe int Size
        {
            [NonVersionable]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return sizeof(void*);
            }
        }

        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Addr(uint value)
        {
            _value = (void*)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Addr(int value)
        {
            _value = (void*)value;
        }

        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Addr(ulong value)
        {
            _value = (void*)(uint)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Addr(long value)
        {
            _value = (void*)value;
        }

        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Addr(void* value)
        {
            _value = value;
        }

        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Addr(IntPtr value)
        {
            _value = (void*)value;
        }

        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Addr(UIntPtr value)
        {
            _value = (void*)value;
        }

        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Addr Add(Addr pointer, int offset)
        {
            return pointer + offset;
        }

        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Addr(uint value)
        {
            return new Addr(value);
        }

        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Addr(ulong value)
        {
            return new Addr(value);
        }

        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Addr(IntPtr value)
        {
            return new Addr(value);
        }

        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Addr(UIntPtr value)
        {
            return new Addr(value);
        }

        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CA2225 // Operator overloads have named alternates
        public static unsafe implicit operator Addr(void* value)
#pragma warning restore CA2225 // Operator overloads have named alternates
        {
            return new Addr(value);
        }

        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe implicit operator IntPtr(Addr value)
        {
            return (IntPtr)value._value;
        }

        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe implicit operator uint(Addr value)
        {
            return (uint)value._value;
        }

        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe implicit operator UIntPtr(Addr value)
        {
            return (UIntPtr)value._value;
        }

        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe implicit operator ulong(Addr value)
        {
            return (ulong)value._value;
        }

        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CA2225 // Operator overloads have named alternates
        public static unsafe implicit operator void*(Addr value)
#pragma warning restore CA2225 // Operator overloads have named alternates
        {
            return value._value;
        }

        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Addr operator -(Addr pointer, int offset)
        {
            return new Addr((ulong)((long)pointer._value - offset));
        }

        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool operator !=(Addr value1, Addr value2)
        {
            return value1._value != value2._value;
        }

        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Addr operator +(Addr pointer, int offset)
        {
            return new Addr((ulong)((long)pointer._value + offset));
        }

        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool operator ==(Addr value1, Addr value2)
        {
            return value1._value == value2._value;
        }

        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Addr Subtract(Addr pointer, int offset)
        {
            return pointer - offset;
        }

        public unsafe override bool Equals(object obj)
        {
            if (obj is Addr)
            {
                return _value == ((Addr)obj)._value;
            }
            return false;
        }

        public unsafe override int GetHashCode()
        {
            return (int)_value;
        }

        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void* ToPointer()
        {
            return _value;
        }

        public unsafe override string ToString()
        {
            return ((long)_value).ToString();
        }

        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe uint ToUInt32()
        {
            return (uint)_value;
        }

        [NonVersionable]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ulong ToUInt64()
        {
            return (ulong)_value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Addr FromUInt32(uint value) => new Addr(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Addr FromInt32(int value) => new Addr(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Addr FromUInt64(ulong value) => new Addr(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Addr FromInt64(long value) => new Addr(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Addr FromIntPtr(IntPtr value) => new Addr(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Addr FromUIntPtr(UIntPtr value) => new Addr(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Addr FromPointer(void* value) => new Addr(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe IntPtr ToIntPtr() => (IntPtr)_value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe UIntPtr ToUIntPtr() => (UIntPtr)_value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe int ToInt32() => (int)_value;

    }
}
