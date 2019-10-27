// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

using System;

namespace Abanu
{

    public unsafe struct NullTerminatedString
    {

        public byte* Bytes
        {
            get
            {
                fixed (NullTerminatedString* ptr = &this)
                    return (byte*)ptr;
            }
        }

        public int GetLength()
        {
            var n = -1;
            var bytes = Bytes;
            while (bytes[++n] != 0)
            {
            }
            return n;
        }

        public bool Equals(NullTerminatedString* value)
        {
            var len1 = GetLength();
            var len2 = value->GetLength();
            if (len1 != len2)
                return false;
            if (len1 == 0)
                return true;
            var bytes1 = this.Bytes;
            var bytes2 = value->Bytes;

            for (var i = 0; i < len1; i++)
                if (bytes1[i] != bytes2[i])
                    return false;

            return true;
        }

        public bool Equals(NullTerminatedString value)
        {
            return Equals(&value);
        }

        public bool Equals(string value)
        {
            var len1 = GetLength();
            var len2 = 0;
            if (value != null)
                len2 = value.Length;
            if (len1 != len2)
                return false;
            if (len1 == 0)
                return true;
            var bytes1 = this.Bytes;

            for (var i = 0; i < len1; i++)
                if (bytes1[i] != (byte)value[i])
                    return false;

            return true;
        }

        public static unsafe bool operator ==(NullTerminatedString value1, string value2)
        {
            return value1.Equals(value2);
        }

        public static unsafe bool operator !=(NullTerminatedString value1, string value2)
        {
            return !value1.Equals(value2);
        }

        public static unsafe bool operator ==(NullTerminatedString value1, NullTerminatedString* value2)
        {
            return value1.Equals(value2);
        }

        public static unsafe bool operator !=(NullTerminatedString value1, NullTerminatedString* value2)
        {
            return !value1.Equals(value2);
        }

        public static unsafe bool operator ==(NullTerminatedString value1, NullTerminatedString value2)
        {
            return value1.Equals(value2);
        }

        public static unsafe bool operator !=(NullTerminatedString value1, NullTerminatedString value2)
        {
            return !value1.Equals(value2);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is NullTerminatedString)
                return (NullTerminatedString)obj == this;

            return false;
        }

        public override int GetHashCode()
        {
            return FNVHash.ComputeInt32(Bytes, GetLength());
        }

        public void Set(string value)
        {
            for (var i = 0; i < value.Length; i++)
                Bytes[i] = (byte)value[i];
            Bytes[value.Length] = 0;
        }

        public static void Set(byte* destination, string value)
        {
            for (var i = 0; i < value.Length; i++)
                destination[i] = (byte)value[i];
            destination[value.Length] = 0;
        }

        public override string ToString()
        {
            // BUG: This does not work:
            //  var path = ((NullTerminatedString*)msg->Arg1)->ToString();
            // but this works:
            // var addr = msg->Arg1;
            // var str = (NullTerminatedString*)addr;
            // var path = str->ToString();

            // To be sure, use the static version of ToString!

            return new string((sbyte*)Bytes);
        }
        public static string ToString(NullTerminatedString* str)
        {
            return new string((sbyte*)str->Bytes);
        }

        public static string ToString(byte* str)
        {
            return new string((sbyte*)str);
        }

    }

}
