// Copyright (c) MOSA Project. Licensed under the New BSD License.

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.Versioning;
using System;

namespace Lonos.Kernel.Core
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
        public unsafe USize(ulong value)
        {
            _value = (void*)((uint)value);
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
        public static unsafe implicit operator USize(void* value)
        {
            return new USize(value);
        }

        [NonVersionable]
        public static unsafe implicit operator void*(USize value)
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

        public unsafe override bool Equals(Object obj)
        {
            if (obj is USize)
            {
                return (_value == ((USize)obj)._value);
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
