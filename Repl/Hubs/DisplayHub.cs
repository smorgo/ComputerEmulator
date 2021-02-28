using HardwareCore;
using KeyboardConnector;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Repl.Hubs
{

    public class DisplayHub : Hub
    {
        private IMemoryMappedKeyboard _keyboard;

        public DisplayHub(IMemoryMappedKeyboard keyboard)
        {
            _keyboard = keyboard;
        }
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        public async Task ReceiveDisplayMode(DisplayMode mode)
        {
            await Clients.All.SendAsync("ReceiveDisplayMode", mode.Width, mode.Height);
        }
        public async Task Write(ushort offset, byte value)
        {
            await Clients.All.SendAsync("Write", offset, value);
        }
        public void KeyDown(string key, int id)
        {
            _keyboard?.OnKeyDown(new KeyPress(key, id));
        }
        public void KeyUp(string key, int id)
        {
            _keyboard?.OnKeyUp(new KeyPress(key, id));
        }
        public void RequestControl()
        {
            _keyboard?.SendControlRegister();
        }
        public async Task ReceiveDisplayControl(byte status)
        {
            await Clients.All.SendAsync("ReceiveDisplayControl", status);
        }
        public async Task ReceiveKeyboardControl(byte status)
        {
            await Clients.All.SendAsync("ReceiveKeyboardControl", status);
        }
        public async Task ReceiveCursorPosition(int x, int y)
        {
            await Clients.All.SendAsync("ReceiveCursorPosition", x, y);
        }
        public async Task Clear()
        {
            await Clients.All.SendAsync("Clear");
        }
    }
}