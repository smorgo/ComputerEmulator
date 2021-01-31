namespace HardwareCore
{
    public interface IWriteOnce
    {
        void Burn(byte[] content, ushort startAddress);
    }
}