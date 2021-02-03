namespace HardwareCore
{
    public interface ILoaderPersistence
    {
        void Load(string name, IAddressMap mem);
        void LoadAt(string name, ushort startAddress, IAddressMap mem);
        void Save(string name, ushort startAddress, ushort length, IAddressMap mem);

    }
}