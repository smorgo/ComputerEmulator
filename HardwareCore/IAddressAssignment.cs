using System;
using System.Threading.Tasks;

namespace HardwareCore
{
    public interface IAddressAssignment
    {
        bool CanRead {get;}
        bool CanWrite {get;}
        ushort StartAddress {get;}
        UInt32 Size {get;}
        void Write(ushort address, Byte value);
        Byte Read(ushort address);
        Task Initialise();
    }
}