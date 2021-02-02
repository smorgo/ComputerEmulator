using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using HardwareCore;
using Microsoft.AspNetCore.SignalR.Client;

namespace KeyboardConnector
{

    public class MemoryMappedKeyboard : IAddressAssignment, IAddressableBlock, IKeyboardInput
    {
        public EventHandler RequestInterrupt {get; set;}
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

        public EventHandler<byte> KeyDown { get; set; }
        public EventHandler<byte> KeyUp { get; set; }

        public EventHandler StatusRequest {get; set;}

        public IKeyboardOutput Transmitter {get; set;}

        public List<IAddressableBlock> Blocks => new List<IAddressableBlock> {this};

        public IAddressAssignment Device => this;

        public int BlockId => 0;

        private byte[] _registers = new byte[4];
        private HubConnection _connection;

        public MemoryMappedKeyboard(ushort startAddress)
        {
            StartAddress = startAddress;
        }

        public async Task Initialise()
        {
            _registers[0] = 0x00;
            _registers[1] = 0x00;
            _registers[2] = 0x00;
            _registers[3] = 0x00;

            _connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5001/display")
                .Build();
        
            _connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0,5) * 1000);
                try
                {
                    await _connection.StartAsync();
                    Debug.Assert(_connection.State == HubConnectionState.Connected);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Console.WriteLine("Unable to reach remote display");
                }
            };

            _connection.On<string>("KeyUp", (e) => OnKeyUp(e)); 
            _connection.On<string>("KeyDown", (e) => OnKeyDown(e)); 
            _connection.On("RequestStatus", () => SendControlRegister()); 

            try
            {
                await _connection.StartAsync();
                Debug.Assert(_connection.State == HubConnectionState.Connected);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Console.WriteLine("Unable to reach remote display");
            }

            await SendControlRegister();
        }

        private async Task OnKeyUp(string key)
        {
            byte keyCode = 0x00;
            if(key.Length == 1)
            {
                keyCode = (byte)key[0];
            }
            KeyUp?.Invoke(this, keyCode);

            _registers[DATA_REGISTER] = keyCode;
            _registers[SCAN_CODE_REGISTER] = keyCode;
            _registers[STATUS_REGISTER] |= (byte)(StatusBits.AsciiAvailable | StatusBits.KeyUp | StatusBits.ScanCodeAvailable);
            RequestInterrupt?.Invoke(this,null);

            await Task.Delay(0);
        }

        private async Task OnKeyDown(string key)
        {
            byte keyCode = 0x00;
            if(key.Length == 1)
            {
                keyCode = (byte)key[0];
            }
            KeyDown?.Invoke(this, keyCode);

            _registers[DATA_REGISTER] = keyCode;
            _registers[SCAN_CODE_REGISTER] = keyCode;
            _registers[STATUS_REGISTER] |= (byte)(StatusBits.AsciiAvailable | StatusBits.KeyUp | StatusBits.ScanCodeAvailable);
            RequestInterrupt?.Invoke(this,null);

            await Task.Delay(0);
        }

        private async Task SendControlRegister()
        {
            try
            {
                await _connection.InvokeAsync("ReceiveKeyboardStatus", _registers[CONTROL_REGISTER]);
            }
            catch (Exception ex)
            {                
                Debug.WriteLine(ex.Message);
            }
        }
        public byte Read(ushort address)
        {
            Debug.Assert(address < Size);

            return _registers[address];
        }

        public void Write(ushort address, byte value)
        {
            if(address == CONTROL_REGISTER)
            {
                _registers[CONTROL_REGISTER] = (byte)(value & 0x7);
                Task.Run(SendControlRegister);
            }
            else
            {
                _registers[CONTROL_REGISTER] = value;
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
