// Copyright (c) MOSA Project. Licensed under the New BSD License.

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.Versioning;
using System;

namespace Lonos.Kernel.Core
{

    [Serializable]
    public struct FileHandle
    {
        private unsafe void* _value; // Do not rename (binary serialization)

        public static readonly FileHandle Zero;  

        [NonVersionable]
        public unsafe FileHandle(uint value)
        {
            _value = (void*)value;
        }

        [NonVersionable]
		public unsafe FileHandle(ulong value)
        {
            _value = (void*)((uint)value);
        }

        [NonVersionable]
		public unsafe FileHandle(void* value)
        {
            _value = value;
        }

        public unsafe override bool Equals(Object obj)
        {
			if (obj is FileHandle)
            {
				return (_value == ((FileHandle)obj)._value);
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
		public static implicit operator FileHandle(uint value)
        {
			return new FileHandle(value);
        }

        [NonVersionable]
		public static implicit operator FileHandle(ulong value)
        {
			return new FileHandle(value);
        }

        [NonVersionable]
		public static unsafe implicit operator FileHandle(void* value)
        {
            return new FileHandle(value);
        }

        [NonVersionable]
		public static unsafe implicit operator void* (FileHandle value)
        {
            return value._value;
        }

        [NonVersionable]
		public static unsafe implicit operator uint(FileHandle value)
        {
            return (uint)value._value;
        }

        [NonVersionable]
		public static unsafe implicit operator ulong(FileHandle value)
        {
            return (ulong)value._value;
        }

        [NonVersionable]
        public static unsafe bool operator ==(FileHandle value1, FileHandle value2)
        {
            return value1._value == value2._value;
        }

        [NonVersionable]
        public static unsafe bool operator !=(FileHandle value1, FileHandle value2)
        {
            return value1._value != value2._value;
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
