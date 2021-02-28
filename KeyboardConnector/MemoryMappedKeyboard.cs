using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using HardwareCore;
using Microsoft.AspNetCore.SignalR.Client;

namespace KeyboardConnector
{
    public class MemoryMappedKeyboard : IAddressAssignment, IAddressableBlock, IMemoryMappedKeyboard
    {
        private int _nextKeyPressId = 1;
        public EventHandler RequestInterrupt {get; set;}
        public const ushort STATUS_REGISTER = 0x0000;
        public const ushort CONTROL_REGISTER = 0x0001;
        public const ushort DATA_REGISTER = 0x0002;
        public const ushort SCAN_CODE_REGISTER = 0x0003;
        private int _lastKeyPressId = int.MinValue;
        private FifoBuffer<KeyboardEvent> _eventBuffer;
        private KeyboardEvent _current;
        private byte _controlRegister = 0x00;
        private IKeyboardHub _hub;

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

        public ushort StartAddress { get; set; }

        public uint Size => 0x04;

        public EventHandler<byte> KeyDown { get; set; }
        public EventHandler<byte> KeyUp { get; set; }

        public List<IAddressableBlock> Blocks {get; private set;}

        public IAddressAssignment Device => this;

        public int BlockId => 0;

        public MemoryMappedKeyboard(IKeyboardHub hub)
        {
            _hub = hub;
            _eventBuffer = new FifoBuffer<KeyboardEvent>(16);
             Blocks = new List<IAddressableBlock> {this};
        }

        public async Task Initialise()
        {
            _eventBuffer.Clear();
            _controlRegister = 0x00;
            _nextKeyPressId = 0;
            _current = null;

            await SendControlRegister();
        }
        public async Task OnKeyUp(KeyPress keyPress)
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

        public async Task OnKeyDown(KeyPress keyPress)
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

        public async Task SendControlRegister()
        {
            await SendControlRegister(_controlRegister);
        }
        public async Task SendControlRegister(byte value)
        {
            await _hub.SendKeyboardControl(value);
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
                        AsyncUtil.RunSync(SendControlRegister);
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

        public void GenerateKeyUp(string key)
        {
            InjectKeyUp(new KeyPress(key, _nextKeyPressId++));
        }

        public void GenerateKeyDown(string key)
        {
            InjectKeyDown(new KeyPress(key, _nextKeyPressId++));
        }

        public void InjectKeyUp(KeyPress keyPress)
        {
            AsyncUtil.RunSync(() => OnKeyUp(keyPress));
        }
        public void InjectKeyDown(KeyPress keyPress)
        {
            AsyncUtil.RunSync(() => OnKeyDown(keyPress));
        }
    }
}
