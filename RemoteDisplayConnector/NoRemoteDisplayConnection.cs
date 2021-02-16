using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HardwareCore;
using Microsoft.AspNetCore.SignalR.Client;

namespace RemoteDisplayConnector
{

    public class NoRemoteDisplayConnection : IRemoteDisplayConnection
    {
        public bool IsConnected {get; private set;}
        public async Task Clear()
        {
            await Task.Delay(0);
        }

        public async Task Initialise()
        {
            IsConnected = true;
            await Task.Delay(0);
        }
        public async Task SendDisplayMode(DisplayMode mode)
        {
            await Task.Delay(0);
        }
        public async Task RenderCharacter(ushort address, byte value)
        {
            await Task.Delay(0);
        }

        public async Task SendCursorPosition(CursorPosition position)
        {
            await Task.Delay(0);
        }

        public async Task SendControl(byte control)
        {
            await Task.Delay(0);
        }
    }
}