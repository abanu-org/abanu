namespace Lonos.Kernel.Core
{
    public interface IBufferWriter
    {
        unsafe SSize Write(byte* buf, USize count);
    }

}
