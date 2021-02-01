using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HardwareCore
{

    public class AddressMap
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public ushort StartAddress => 0x0000;

        public uint Size => 0x10000;

        public ushort LowWaterMark {get; private set;}
        public ushort HighWaterMark {get; private set;}

        private IAddressableBlock[] RedirectionTable = new IAddressableBlock[0x10000]; // This is going to be woefully inefficient in terms of memory
        private List<IAddressAssignment> _installedModules = new List<IAddressAssignment>();

        public AddressMap()
        {
            ResetWatermarks();
        }

        public async Task Initialise()
        {
            foreach(var module in _installedModules)
            {
                await module.Initialise();
            }
        }

        public void Install(IAddressAssignment device)
        {
            _installedModules.Add(device);

            foreach(var block in device.Blocks)
            {
                var jx = block.StartAddress;
                for(var ix = 0; ix < block.Size; ix++)
                {
                    RedirectionTable[jx++] = block;
                }
            }
        }

        public Loader Load(ushort startAddress = 0x0000)
        {
            return new Loader(this, startAddress);
        }

        public void ResetWatermarks()
        {
            LowWaterMark = 0xFFFF;
            HighWaterMark = 0x0000;
        }

        public byte Read(ushort address)
        {
            var block = RedirectionTable[address];

            if(block != null && block.CanRead)
            {
                return block.Device.Read(block.BlockId, (ushort)(address - block.StartAddress));
            }

            return 0; // Could return some random value for noise.
        }

        public void Write(ushort address, byte value)
        {
            var block = RedirectionTable[address];

            if(block != null && block.CanWrite)
            {
                block.Device.Write(block.BlockId, (ushort)(address - block.StartAddress), value);
            }

            Debug.WriteLine($"[{address:X4}] <- {value:X2}");

            if(address < LowWaterMark)
            {
                LowWaterMark = address;
            }

            if(address > HighWaterMark)
            {
                HighWaterMark = address;
            }
        }

        public void WriteWord(ushort address, ushort value)
        {
            Debug.Assert(address < Size - 1);
            Write(address, (byte)(value & 0xff));
            Write((ushort)(address+1), (byte)(value >> 8));
        }

        public ushort ReadWord(ushort address)
        {
            Debug.Assert(address < Size - 1);
            return (ushort)(Read(address) + 256 * Read((ushort)(address+1)));
        }
    }
}