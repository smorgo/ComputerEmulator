namespace _6502
{
    public interface IWriteOnce
    {
        void Burn(byte[] content, ushort startAddress);
    }
}