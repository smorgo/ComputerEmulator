using System;

namespace _6502
{
    public interface IAddressAssignment
    {
        bool CanRead {get;}
        bool CanWrite {get;}
        ushort StartAddress {get;}
        UInt32 Size {get;}
        void Write(ushort address, Byte value);
        Byte Read(ushort address);
    }
}