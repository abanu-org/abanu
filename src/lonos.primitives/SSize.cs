// Copyright (c) MOSA Project. Licensed under the New BSD License.

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.Versioning;
using System;

namespace lonos.Kernel.Core
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
		public unsafe SSize(ulong value)
        {
            _value = (void*)((uint)value);
        }

        [NonVersionable]
		public unsafe SSize(void* value)
        {
            _value = value;
        }

        public unsafe override bool Equals(Object obj)
        {
			if (obj is SSize)
            {
				return (_value == ((SSize)obj)._value);
            }
            return false;
        }

        public unsafe override int GetHashCode()
        {
            return ((int)_value);
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
		public static unsafe implicit operator SSize(void* value)
        {
            return new SSize(value);
        }

        [NonVersionable]
		public static unsafe implicit operator void* (SSize value)
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
    }
}
