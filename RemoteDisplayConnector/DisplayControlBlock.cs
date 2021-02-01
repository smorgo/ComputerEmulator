using System;
using System.Threading.Tasks;
using HardwareCore;

namespace RemoteDisplayConnector
{
    public class DisplayControlBlock : IAddressableBlock
    {
        public EventHandler<byte> OnControlChanged {get; set;}
        public EventHandler<byte> OnModeChanged {get; set;}
        public EventHandler<CursorPosition> OnCursorMoved {get; set;}
        public const ushort CONTROL_ADDR = 0x00;

        public static class ControlBits
        {
            const byte CURSOR_ENABLED = 0x01;
            const byte CURSOR_FLASHING = 0x02;
        }

        public const ushort MODE_ADDR = 0x01;
        public const ushort CURSOR_X_ADDR = 0x02;
        public const ushort CURSOR_Y_ADDR = 0x03;

        public IAddressAssignment Device {get; private set;}

        public bool CanRead => true;

        public bool CanWrite => true;

        public int BlockId {get; private set;}

        public ushort StartAddress {get; private set;}

        public uint Size => 4;
        public Byte[] Memory {get; private set;}

        public DisplayControlBlock(IAddressAssignment device, int blockId, ushort startAddress)
        {
            Device = device;
            BlockId = blockId;
            StartAddress = startAddress;
            Memory = new byte[Size];
        }

        public void Write(ushort address, byte value)
        {
            if(value != Memory[address])
            {
                Memory[address] = value;

                switch(address)
                {
                    case CONTROL_ADDR:
                        OnControlChanged?.Invoke(this, value);
                        break;
                    case MODE_ADDR:
                        OnModeChanged?.Invoke(this, value);
                        break;
                    default: // Cursor
                        OnCursorMoved?.Invoke(this, new CursorPosition(Memory[CURSOR_X_ADDR], Memory[CURSOR_Y_ADDR]));
                        break;
                }
            }
        }

        public byte Read(ushort address)
        {
            return Memory[address];
        }

        public async Task Initialise()
        {
            Array.Fill<byte>(Memory, 0x00);
            await Task.Delay(0);
        }
    }
}