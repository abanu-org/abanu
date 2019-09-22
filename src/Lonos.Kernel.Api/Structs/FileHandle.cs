// This file is part of Lonos Project, an Operating System written in C#. Web: https://www.lonos.io
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Runtime.Versioning;

namespace Lonos.Kernel.Core
{

    [Serializable]
    public struct FileHandle
    {
        private unsafe int _value; // Do not rename (binary serialization)

        public static readonly FileHandle Zero;

        [NonVersionable]
        public unsafe FileHandle(int value)
        {
            _value = value;
        }

        public unsafe override bool Equals(object obj)
        {
            if (obj is FileHandle)
            {
                return _value == ((FileHandle)obj)._value;
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
        public unsafe int ToInt32()
        {
            return _value;
        }

        [NonVersionable]
        public static implicit operator FileHandle(int value)
        {
            return new FileHandle(value);
        }

        [NonVersionable]
        public static unsafe implicit operator uint(FileHandle value)
        {
            return (uint)value._value;
        }

        [NonVersionable]
        public static unsafe implicit operator int(FileHandle value)
        {
            return value._value;
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

        public unsafe override string ToString()
        {
            return _value.ToString();
        }

        public static FileHandle FromInt32(int value) => new FileHandle(value);
    }
}
