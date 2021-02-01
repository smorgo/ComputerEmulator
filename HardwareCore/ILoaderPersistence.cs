namespace HardwareCore
{
    public interface ILoaderPersistence
    {
        void Load(string name, AddressMap mem);
        void LoadAt(string name, ushort startAddress, AddressMap mem);
        void Save(string name, ushort startAddress, ushort length, AddressMap mem);

    }
}