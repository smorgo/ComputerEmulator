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
        public class KeyboardEvent
        {
            public byte Status {get; private set;}
            public byte Data {get; private set;}
            public byte ScanCode {get; private set;}

            public KeyboardEvent()
            {
            }

            public KeyboardEvent(byte status, byte data, byte scanCode)
            {
                Status = status;
                Data = data;
                ScanCode = scanCode;
            }
        }
        public EventHandler RequestInterrupt {get; set;}
        public const ushort STATUS_REGISTER = 0x0000;
        public const ushort CONTROL_REGISTER = 0x0001;
        public const ushort DATA_REGISTER = 0x0002;
        public const ushort SCAN_CODE_REGISTER = 0x0003;
        private int _lastKeyPressId = int.MinValue;
        private FifoBuffer<KeyboardEvent> _eventBuffer;
        private KeyboardEvent _current;
        private byte _controlRegister = 0x00;

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

        private IRemoteConnection _connection;
        private IRemoteKeyboard _keyboard;

        public MemoryMappedKeyboard(ushort startAddress, IRemoteConnection connection)
        {
            StartAddress = startAddress;
            _connection = connection;
            _keyboard = (IRemoteKeyboard)connection;
            _eventBuffer = new FifoBuffer<KeyboardEvent>(16);
        }

        public async Task Initialise()
        {
            _eventBuffer.Clear();
            _controlRegister = 0x00;

            _current = null;

            _keyboard.OnKeyUp += async (s,e) => {await OnKeyUp(e);}; 
            _keyboard.OnKeyDown += async (s,e) => {await OnKeyDown(e);}; 
            _keyboard.OnRequestControl += async (s,e) => {await SendControlRegister();}; 

            await _connection.ConnectAsync("https://localhost:5001/display");
            Debug.Assert(_connection.IsConnected);

            await SendControlRegister();
        }
        private async Task OnKeyUp(KeyPress keyPress)
        {
            lock(this)
            {
                var key = keyPress.Key;

                if(keyPress.Id == _lastKeyPressId)
                {
                    Debug.WriteLine("Ignoring duplicate keypress");
                    return;
                }

                _lastKeyPressId = keyPress.Id;

                Debug.WriteLine("OnKeyUp");
                byte keyCode = 0x00;
                if(key.Length == 1)
                {
                    keyCode = (byte)key[0];
                }

                _eventBuffer.Write(
                    new KeyboardEvent(
                        (byte)(StatusBits.AsciiAvailable | StatusBits.KeyUp | StatusBits.ScanCodeAvailable),
                        keyCode,
                        keyCode));

                KeyUp?.Invoke(this, keyCode);

                RequestInterrupt?.Invoke(this,null);
            }
            await Task.Delay(0);
        }

        private async Task OnKeyDown(KeyPress keyPress)
        {
            lock(this)
            {
                var key = keyPress.Key;

                if(keyPress.Id == _lastKeyPressId)
                {
                    Debug.WriteLine("Ignoring duplicate keypress");
                    return;
                }

                _lastKeyPressId = keyPress.Id;

                Debug.WriteLine("OnKeyDown");
                byte keyCode = 0x00;
                if(key.Length == 1)
                {
                    keyCode = (byte)key[0];
                }

                _eventBuffer.Write(
                    new KeyboardEvent(
                        (byte)(StatusBits.AsciiAvailable | StatusBits.KeyDown | StatusBits.ScanCodeAvailable),
                        keyCode,
                        keyCode));

                KeyDown?.Invoke(this, keyCode);

                RequestInterrupt?.Invoke(this,null);
            }

            await Task.Delay(0);
        }

        private async Task SendControlRegister()
        {
            try
            {
                await _keyboard.SendControlRegister(_controlRegister);
            }
            catch (Exception ex)
            {                
                Debug.WriteLine(ex.Message);
            }
        }
        public byte Read(ushort address)
        {
            lock(this)
            {
                Debug.Assert(address < Size);
                KeyboardEvent evt;

                switch(address)
                {
                    case CONTROL_REGISTER:
                        return _controlRegister;
                    case STATUS_REGISTER:
                        if(_eventBuffer.Read(out evt))
                        {
                            _current = evt;
                            return evt.Status;
                        }        
                        _current = null;
                        break;
                    case DATA_REGISTER:
                        if(_current != null)
                        {
                            return _current.Data;
                        }
                        return 0x00;
                    case SCAN_CODE_REGISTER:
                        if(_current != null)
                        {
                            return _current.Data;
                        }
                        return 0x00;
                }
            }

            return 0x00;
        }

        public void Write(ushort address, byte value)
        {
            lock(this)
            {
                switch(address)
                {
                    case STATUS_REGISTER:
                        _current = null;
                        break;
                    case CONTROL_REGISTER:
                        _controlRegister = (byte)(value & 0x7);
                        Task.Run(SendControlRegister);
                        break;
                }
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
