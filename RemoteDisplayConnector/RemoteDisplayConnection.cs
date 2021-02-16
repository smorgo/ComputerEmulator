using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HardwareCore;
using Microsoft.AspNetCore.SignalR.Client;
using SignalRConnection;

namespace RemoteDisplayConnector
{

    public class RemoteDisplayConnection : IRemoteDisplayConnection
    {
        public bool IsConnected => _connection.IsConnected;
        private ISignalRHubConnection _connection;
        public RemoteDisplayConnection(ISignalRHubConnection connection)
        {
            _connection = connection;
        }
        public async Task Clear()
        {
            if(_connection.IsConnected)
            {
                await _connection.InvokeAsync("Clear");
            }
        }
        public async Task Initialise()
        {
            await _connection.Connect("https://localhost:5001/display");
        }
        public async Task SendDisplayMode(DisplayMode mode)
        {
            if (_connection.IsConnected)
            {
                await _connection.InvokeAsync("ReceiveDisplayMode", mode);
            }
        }
        public async Task RenderCharacter(ushort address, byte value)
        {
            if (_connection.IsConnected)
            {
                await _connection.InvokeAsync("Write",
                        address, value);
            }
        }
        public async Task SendCursorPosition(CursorPosition position)
        {
            if (_connection.IsConnected)
            {
                await _connection.InvokeAsync("ReceiveCursorPosition", position.X, position.Y);
            }
        }
        public async Task SendControl(byte control)
        {
            if (_connection.IsConnected)
            {
                await _connection.InvokeAsync("ReceiveKeyboardControl", control);
            }
        }
    }
}