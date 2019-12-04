// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

using System;
using System.Runtime.Versioning;

namespace Abanu.Kernel.Core
{

    [Serializable]
    public struct FileHandle
    {
        private int _value; // Do not rename (binary serialization)

        public static readonly FileHandle Zero;

        public static FileHandle StandaradInput => new FileHandle(0);
        public static FileHandle StandaradOutput => new FileHandle(1);
        public static FileHandle StandaradError => new FileHandle(2);

        [NonVersionable]
        public FileHandle(int value)
        {
            _value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj is FileHandle)
            {
                return _value == ((FileHandle)obj)._value;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return (int)_value;
        }

        [NonVersionable]
        public uint ToUInt32()
        {
            return (uint)_value;
        }

        [NonVersionable]
        public int ToInt32()
        {
            return _value;
        }

        [NonVersionable]
        public static implicit operator FileHandle(int value)
        {
            return new FileHandle(value);
        }

        [NonVersionable]
        public static implicit operator uint(FileHandle value)
        {
            return (uint)value._value;
        }

        [NonVersionable]
        public static implicit operator int(FileHandle value)
        {
            return value._value;
        }

        [NonVersionable]
        public static bool operator ==(FileHandle value1, FileHandle value2)
        {
            return value1._value == value2._value;
        }

        [NonVersionable]
        public static bool operator !=(FileHandle value1, FileHandle value2)
        {
            return value1._value != value2._value;
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public static FileHandle FromInt32(int value) => new FileHandle(value);
    }
}
