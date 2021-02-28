using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HardwareCore;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR;

namespace RemoteDisplayConnector
{

    public class RemoteDisplayConnection : IRemoteDisplayConnection
    {
        private IDisplayHub _hub;
        public bool IsConnected => true;
        public RemoteDisplayConnection(IDisplayHub hub)
        {
            _hub = hub;
        }
        public async Task Clear()
        {
            await _hub.Clear();
        }
        public async Task Initialise()
        {
            await Task.Delay(0);
        }
        public async Task SendDisplayMode(DisplayMode mode)
        {
            await _hub.ReceiveDisplayMode(mode);
        }
        public async Task RenderCharacter(ushort address, byte value)
        {
            await _hub.Write(address, value);
        }
        public async Task SendCursorPosition(CursorPosition position)
        {
            await _hub.ReceiveCursorPosition(position.X, position.Y);
        }
        public async Task SendControl(byte value)
        {
            await _hub.SendControl(value);
        }
    }
}