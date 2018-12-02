namespace lonos.kernel.core
{
    public interface IBufferWriter
    {
        unsafe SSize Write(byte* buf, USize count);
    }

}
