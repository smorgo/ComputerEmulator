using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HardwareCore;
using Microsoft.Extensions.Logging;
using SignalRConnection;

namespace KeyboardConnector
{

    public class RemoteKeyboardConnection : IRemoteConnection, IRemoteKeyboard
    {
        public EventHandler<KeyPress> OnKeyUp {get; set;}
        public EventHandler<KeyPress> OnKeyDown {get; set;}
        public EventHandler OnRequestControl {get; set;}

        private ISignalRHubConnection _connection;

        public bool IsConnected => _connection == null ? false : _connection.IsConnected;
        private ILogger _logger;

        public RemoteKeyboardConnection(ILogger<RemoteKeyboardConnection> logger, ISignalRHubConnection connection)
        {
            _logger = logger;
            _connection = connection;
        }
        public async Task ConnectAsync(string url)
        {
            await _connection.Connect(url);

            _connection.On<string, int>("KeyUp", (e,i) => OnKeyUp?.Invoke(this, new KeyPress(e,i))); 
            _connection.On<string, int>("KeyDown", (e,i) => OnKeyDown?.Invoke(this, new KeyPress(e,i))); 
            _connection.On("RequestControl", () => OnRequestControl?.Invoke(this, null)); 
        }

        public async Task SendControlRegister(byte value)
        {
            if(_connection.IsConnected)
            {
                await _connection.InvokeAsync("ReceiveKeyboardControl", value);
            }
        }

        public async Task GenerateKeyUp(string key)
        {
            if(_connection.IsConnected)
            {
                await _connection.InvokeAsync("KeyUp", key);
            }
        }

        public async Task GenerateKeyDown(string key)
        {
            if(_connection.IsConnected)
            {
               await _connection.InvokeAsync("KeyDown", key);
            }
        }
    }
}