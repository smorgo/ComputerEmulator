using HardwareCore;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace RemoteDisplay.Hubs
{
    public class DisplayHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        public async Task SetMode(DisplayMode mode)
        {
            await Clients.All.SendAsync("SetMode", mode.Width, mode.Height);
        }
        public async Task Write(ushort offset, byte value)
        {
            await Clients.All.SendAsync("Write", offset, value);
        }
        public async Task KeyDown(string key)
        {
            await Clients.All.SendAsync("KeyDown", key);
        }
        public async Task KeyUp(string key)
        {
            await Clients.All.SendAsync("KeyUp", key);
        }
        public async Task RequestControl()
        {
            await Clients.All.SendAsync("RequestControl");
        }
        public async Task ReceiveKeyboardControl(byte status)
        {
            await Clients.All.SendAsync("ReceiveKeyboardControl", status);
        }
        public async Task Clear()
        {
            await Clients.All.SendAsync("Clear");
        }
    }
}