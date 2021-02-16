using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HardwareCore;
using Microsoft.AspNetCore.SignalR.Client;
using RemoteDisplayConnector;

namespace Tests
{
    public class MockRemoteDisplayConnection : IRemoteDisplayConnection
    {
        public EventHandler<MockRemoteDisplayRenderEventArgs> OnRender {get; set;}
        public EventHandler OnClear {get; set;}
        public EventHandler<CursorPosition> OnCursorPosition{get; set;}
        public EventHandler<byte> OnControl {get; set;}
        public bool IsConnected {get; private set;}
        public async Task Clear()
        {
            OnClear?.Invoke(this, null);
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
            OnRender?.Invoke(this, new MockRemoteDisplayRenderEventArgs(address, value));
            await Task.Delay(0);
        }

        public async Task SendCursorPosition(CursorPosition position)
        {
            OnCursorPosition?.Invoke(this, position);
            await Task.Delay(0);
        }

        public async Task SendControl(byte control)
        {
            OnControl?.Invoke(this, control);
            await Task.Delay(0);
        }
    }
}