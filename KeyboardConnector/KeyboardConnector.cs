using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using HardwareCore;

namespace KeyboardConnector
{

    public class KeyboardConnector : IAddressAssignment, IAddressableBlock, IKeyboardInput
    {
        public const ushort STATUS_REGISTER = 0x0000;
        public const ushort CONTROL_REGISTER = 0x0001;
        public const ushort DATA_REGISTER = 0x0002;
        public const ushort SCAN_CODE_REGISTER = 0x0003;

        [Flags]
        public enum StatusBits
        {
            KeyDown = 0x1,
            KeyUp = 0x2,
            ScanCodeAvailable = 0x4,
            AsciiAvailable = 0x8
        }

        [Flags]
        public enum ControlBits
        {
            Disable = 0x1,
            CapsLock = 0x2,
            ShiftLock = 0x4
        }

        public bool CanRead => true;

        public bool CanWrite => true;

        public ushort StartAddress { get; private set; }

        public uint Size => 0x04;

        public EventHandler<byte> OnKeyDown { get; set; }
        public EventHandler<byte> OnKeyUp { get; set; }

        public EventHandler OnStatusRequest {get; set;}

        public IKeyboardOutput Transmitter {get; set;}

        public List<IAddressableBlock> Blocks => new List<IAddressableBlock> {this};

        public IAddressAssignment Device => this;

        public int BlockId => 0;

        private byte[] _registers = new byte[4];

        public KeyboardConnector(ushort startAddress)
        {
            StartAddress = startAddress;
        }

        public async Task Initialise()
        {
            _registers[0] = 0x00;
            _registers[1] = 0x00;
            _registers[2] = 0x00;
            _registers[3] = 0x00;
            await Task.Delay(0);
        }

        public byte Read(ushort address)
        {
            Debug.Assert(address < Size);

            return _registers[address];
        }

        public void Write(ushort address, byte value)
        {
            // Only the control register is writable
            if(address == CONTROL_REGISTER)
            {
                _registers[CONTROL_REGISTER] = (byte)(value & 0x7);
                Transmitter?.SendControl(_registers[CONTROL_REGISTER]);
            }
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
