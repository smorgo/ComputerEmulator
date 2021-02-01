using System;
using System.Threading.Tasks;
using HardwareCore;

namespace RemoteDisplayConnector
{
    public class VideoRam : IAddressableBlock
    {
        public EventHandler<AddressedByte> OnRender {get; set;}
        public IAddressAssignment Device {get; private set;}

        public bool CanRead => true;

        public bool CanWrite => true;

        public int BlockId {get; private set;}

        public ushort StartAddress {get; private set;}

        public uint Size {get; private set;}
        public Byte[] Memory {get; private set;}

        public VideoRam(IAddressAssignment device, int blockId, ushort startAddress, uint size)
        {
            Device = device;
            BlockId = blockId;
            StartAddress = startAddress;
            Size = size;
            Memory = new byte[size];
        }

        public void Write(ushort address, byte value)
        {
            Memory[address] = value;
            OnRender?.Invoke(this, new AddressedByte(address, value));
        }

        public byte Read(ushort address)
        {
            return Memory[address];
        }

        public async Task Initialise()
        {
            Clear();
            await Task.Delay(0);
        }

        public void Clear()
        {
            Array.Fill<byte>(Memory, 0x00);
        }
    }
}