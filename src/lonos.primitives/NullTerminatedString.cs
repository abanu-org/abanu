using System;

namespace Lonos
{

    unsafe public struct NullTerminatedString
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
            while (bytes[++n] != 0) ;
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

    }

}
