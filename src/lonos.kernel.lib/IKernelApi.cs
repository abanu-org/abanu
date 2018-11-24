using System;

namespace lonos.kernel.core
{

    public interface IKernelApi
    {
        /// <summary>
        /// fwrite
        /// </summary>
        int FileWrite(IntPtr ptr, USize elementSize, USize elements, FileHandle stream);

        /// <summary>
        /// fputs
        /// </summary>
        int FileWrite(NullTerminatedString str, FileHandle stream);

        /// <summary>
        /// write
        /// </summary>
        /// <returns>The write.</returns>
        SSize FileWrite(FileHandle file, IntPtr buf, USize count);
    }

}
