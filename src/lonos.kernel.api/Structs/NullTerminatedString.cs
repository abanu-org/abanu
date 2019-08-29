using System;

namespace lonos.Kernel.Core
{

    unsafe public struct NullTerminatedString
    {

        public Byte* Bytes
        {
            get
            {
                fixed (NullTerminatedString* ptr = &this)
                {
                    return (Byte*)ptr;
                }
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
            {
                if (bytes1[i] != bytes2[i])
                    return false;
            }
            return true;
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
            {
                if (bytes1[i] != (byte)value[i])
                    return false;
            }
            return true;
        }
    
    }

}
