using System;
using System.Threading.Tasks;
using HardwareCore;
using KeyboardConnector;

namespace Tests
{
    public class MockRemoteKeyboardConnection : IRemoteConnection, IRemoteKeyboard, IDisposable
    {
        private int _lastKeyPressId = 0;
        public bool IsConnected {get; private set;}

        public EventHandler<KeyPress> OnKeyUp { get; set; }
        public EventHandler<KeyPress> OnKeyDown { get; set; }
        public EventHandler OnRequestControl { get; set; }

        public async Task ConnectAsync(string url)
        {
            IsConnected = true;
            await Task.Delay(0);
        }

        public void Dispose()
        {
        }

        public void KeyUp(string key)
        {
            OnKeyUp?.Invoke(this, new KeyPress(key, _lastKeyPressId++));
        }

        public void KeyDown(string key)
        {
            OnKeyDown?.Invoke(this, new KeyPress(key, _lastKeyPressId++));
        }

        public void RequestControl()
        {
            OnRequestControl?.Invoke(this, null);
        }

        public async Task SendControlRegister(byte value)
        {
            await Task.Delay(0);
        }

        public async Task GenerateKeyUp(string key)
        {
            KeyUp(key);
            await Task.Delay(0);
        }

        public async Task GenerateKeyDown(string key)
        {
            KeyDown(key);
            await Task.Delay(0);
        }
    }
}