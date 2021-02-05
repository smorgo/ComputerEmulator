using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using HardwareCore;

namespace _6502
{
    public class Rom : IAddressAssignment, IAddressableBlock, IWriteOnce, IErasable
    {
        private bool _burn;
        public bool Burn 
        {
            get
            {
                return _burn;
            }
            set
            {
                if(value != _burn)
                {
                    if(value)
                    {
                        if(Burned)
                        {
                            throw new InvalidOperationException("ROM must be erased before it can be re-burned");
                        }

                        _burn = value;
                    }
                    else
                    {
                        Burned = true;
                        _burn = value;
                    }
                }
            }
        }

        public bool Burned {get; private set;}
        public bool CanRead => true;
        public bool CanWrite => Burn;
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
            if(Burn)
            {
                Memory[address] = value;
            }
        }

        public byte Read(ushort address)
        {
            // address is now relative to start address
            Debug.Assert(address < Size);
            return Memory[address];
        }

        public void Erase()
        {
            Array.Fill<byte>(Memory, 0xFF);

            Burned = false;
            Burn = false;
        }

        public void BurnContent(byte[] content, ushort startAddress)
        {
            if(Burned)
            {
                throw new InvalidOperationException("Cannot burn an already burned ROM");
            }

            var bytesToBurn = Math.Min(content.Length, Size - startAddress);

            Array.Copy(content, 0, Memory, startAddress, bytesToBurn); // Check if there's a risk of overflow.

            Burned = true;
        }

        public async Task Initialise()
        {
            Erase();
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