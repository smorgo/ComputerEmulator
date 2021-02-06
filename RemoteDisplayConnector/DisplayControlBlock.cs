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
        public EventHandler OnClearScreen {get; set;}
        public const ushort CONTROL_ADDR = 0x00;

        public static class ControlBits
        {
            public const byte CURSOR_ENABLED = 0x01;
            public const byte CURSOR_FLASHING = 0x02;
            public const byte CLEAR_SCREEN = 0x04;
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

        public byte CursorX 
        {
            get 
            {
                return Memory[CURSOR_X_ADDR];
            }

            set
            {
                Memory[CURSOR_X_ADDR] = value;
            }
        }
        public byte CursorY
        {
            get 
            {
                return Memory[CURSOR_Y_ADDR];
            }

            set
            {
                Memory[CURSOR_Y_ADDR] = value;
            }
        }
        public bool CursorEnabled
        {
            get
            {
                return ((Memory[CONTROL_ADDR] & ControlBits.CURSOR_ENABLED) == ControlBits.CURSOR_ENABLED);
            }

            set
            {
                if(value)
                {
                    Memory[CONTROL_ADDR] |= ControlBits.CURSOR_ENABLED;
                }
                else
                {
                    Memory[CONTROL_ADDR] &= (byte)(~(uint)ControlBits.CURSOR_ENABLED & 0xFF);
                }
            }
        }
        public bool CursorFlashing
        {
            get
            {
                return ((Memory[CONTROL_ADDR] & ControlBits.CURSOR_FLASHING) == ControlBits.CURSOR_FLASHING);
            }

            set
            {
                if(value)
                {
                    Memory[CONTROL_ADDR] |= ControlBits.CURSOR_FLASHING;
                }
                else
                {
                    Memory[CONTROL_ADDR] &= (byte)(~(uint)ControlBits.CURSOR_FLASHING & 0xFF);
                }
            }
        }
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
                switch(address)
                {
                    case CONTROL_ADDR:
                        if(!HandleClearScreen(value))
                        {
                            Memory[address] = value;
                            OnControlChanged?.Invoke(this, value);
                        }
                        break;
                    case MODE_ADDR:
                        Memory[address] = value;
                        OnModeChanged?.Invoke(this, value);
                        break;
                    default: // Cursor
                        Memory[address] = value;
                        OnCursorMoved?.Invoke(this, new CursorPosition(Memory[CURSOR_X_ADDR], Memory[CURSOR_Y_ADDR]));
                        break;
                }
            }
        }

        public byte Read(ushort address)
        {
            return Memory[address];
        }

        private bool HandleClearScreen(byte value)
        {
            if((value & ControlBits.CLEAR_SCREEN) == ControlBits.CLEAR_SCREEN)
            {
                OnClearScreen?.Invoke(this, null);
                return true;
            }

            return false;
        }

        public async Task Initialise()
        {
            Array.Fill<byte>(Memory, 0x00);
            await Task.Delay(0);
        }
    }
}