using System;
using System.Threading.Tasks;
using HardwareCore;
using Microsoft.AspNetCore.SignalR;
using Repl.Hubs;

namespace Repl
{

    public class DisplayHubProxy : IDisplayHub
    {
        private IHubContext<DisplayHub> _hubContext;
        
        public DisplayHubProxy(IHubContext<DisplayHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public async Task SendMessage(string user, string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        public async Task ReceiveDisplayMode(DisplayMode mode)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveDisplayMode", mode.Width, mode.Height);
        }
        public async Task Write(ushort offset, byte value)
        {
            await _hubContext.Clients.All.SendAsync("Write", offset, value);
        }
        public async Task RequestControl()
        {
            await _hubContext.Clients.All.SendAsync("RequestControl");
        }
        public async Task ReceiveCursorPosition(int x, int y)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveCursorPosition", x, y);
        }
        public async Task Clear()
        {
            await _hubContext.Clients.All.SendAsync("Clear");
        }
        public async Task SendControl(byte value)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveDisplayControl", value);
        }
    }
}
