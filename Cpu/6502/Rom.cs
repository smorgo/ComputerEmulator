using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using HardwareCore;

namespace _6502
{
    public class Rom : IAddressAssignment, IAddressableBlock, IWriteOnce, IErasable
    {
        private bool Burned = false;
        public bool CanRead => true;
        public bool CanWrite => false;
        public ushort StartAddress {get; private set;}
        public UInt32 Size {get; private set;}

        public List<IAddressableBlock> Blocks => new List<IAddressableBlock> {this};

        public IAddressAssignment Device => this;

        public int BlockId => 0;

        private Byte[] Memory;

        public Rom(ushort absoluteAddress, UInt32 size) 
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
            // Don't write anything
        }

        public byte Read(ushort address)
        {
            // address is now relative to start address
            Debug.Assert(address < Size);
            return Memory[address];
        }

        public void Erase()
        {
            for(var ix = 0; ix < Size; ix++)
            {
                Memory[ix] = 0x00;
            }

            Burned = false;
        }

        public void Burn(byte[] content, ushort startAddress)
        {
            Debug.Assert(!Burned);

            Array.Copy(content, 0, Memory, startAddress, content.Length); // Check if there's a risk of overflow.

            Burned = true;
        }

        public async Task Initialise()
        {
            Array.Fill<byte>(Memory, 0xFF);
            await Task.Delay(0);
        }

        public void Write(int blockId, ushort address, byte value)
        {
            Write(address, value);
        }

        public byte Read(int blockId, ushort address)
        {
            return Read(address);
        }
    }
}