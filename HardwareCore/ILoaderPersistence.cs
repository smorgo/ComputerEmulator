namespace HardwareCore
{
    public interface ILoaderPersistence
    {
        void Load(string name, IAddressAssignment mem);
        void LoadAt(string name, ushort startAddress, IAddressAssignment mem);
        void Save(string name, ushort startAddress, ushort length, IAddressAssignment mem);

    }
}