using System.Diagnostics;

namespace HardwareCore
{
    public class AddressMap : IAddressAssignment
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public ushort StartAddress => 0x0000;

        public uint Size => 0x10000;

        public ushort LowWaterMark {get; private set;}
        public ushort HighWaterMark {get; private set;}

        private IAddressAssignment[] RedirectionTable = new IAddressAssignment[0x10000]; // This is going to be woefully inefficient in terms of memory

        public AddressMap()
        {
            ResetWatermarks();
        }

        public void Install(IAddressAssignment device)
        {
            Debug.Assert(device.StartAddress + device.Size <= Size);

            var jx = device.StartAddress;
            for(var ix = 0; ix < device.Size; ix++)
            {
                RedirectionTable[jx++] = device;
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
            var device = RedirectionTable[address];

            if(device != null && device.CanRead)
            {
                return device.Read((ushort)(address - device.StartAddress));
            }

            return 0; // Could return some random value for noise.
        }

        public void Write(ushort address, byte value)
        {
            var device = RedirectionTable[address];

            if(device != null && device.CanWrite)
            {
                device.Write((ushort)(address - device.StartAddress), value);
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