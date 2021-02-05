namespace HardwareCore
{
    public interface IWriteOnce
    {
        void BurnContent(byte[] content, ushort startAddress);
    }
}