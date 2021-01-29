using System;
using System.Diagnostics;

namespace _6502
{
    public class MemoryMappedDisplay : IAddressAssignment
    {
        public bool CanRead => true;
        public bool CanWrite => true;
        public ushort StartAddress {get; private set;}
        public UInt32 Size {get; private set;}
        private Byte[] Memory;

        public MemoryMappedDisplay(ushort absoluteAddress, UInt32 size) 
        {
            Debug.Assert(absoluteAddress + size <= 0x10000);
            StartAddress = absoluteAddress;
            Size = size;
            Memory = new byte[size];
        }

        public void Write(ushort address, byte value)
        {
            // address is now relative to start address
            Debug.Assert(address < Size);
            Memory[address] = value;
            Render();
        }

        public byte Read(ushort address)
        {
            // address is now relative to start address
            Debug.Assert(address < Size);
            return Memory[address];
        }

        private void Render()
        {
            Debug.WriteLine($"Display: {System.Text.Encoding.ASCII.GetString(Memory, 0, (int)Size)}");
        }

        public void Clear()
        {
            for(var ix = 0; ix < Size; ix++)
            {
                Memory[ix] = (byte)' ';
            }
        }
    }
}