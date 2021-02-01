using System.Threading.Tasks;

namespace HardwareCore
{
    public interface IAddressableBlock
    {
        IAddressAssignment Device {get;}
        bool CanRead {get;}
        bool CanWrite {get;}
        int BlockId {get;}
        ushort StartAddress {get;}
        uint Size {get;}
        void Write(ushort address, byte value);
        byte Read(ushort address);
        Task Initialise();
    }
}