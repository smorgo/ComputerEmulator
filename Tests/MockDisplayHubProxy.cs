using System.Threading.Tasks;
using HardwareCore;

namespace Tests
{
    public class MockDisplayHubProxy : IDisplayHub
    {
        public async Task Clear()
        {
            await Task.Delay(0);
        }

        public async Task KeyDown(string key, int id)
        {
            await Task.Delay(0);
        }

        public async Task KeyUp(string key, int id)
        {
            await Task.Delay(0);
        }

        public async Task ReceiveCursorPosition(int x, int y)
        {
            await Task.Delay(0);
        }

        public async Task ReceiveDisplayMode(DisplayMode mode)
        {
            await Task.Delay(0);
        }

        public async Task ReceiveKeyboardControl(byte status)
        {
            await Task.Delay(0);
        }

        public async Task RequestControl()
        {
            await Task.Delay(0);
        }

        public async Task SendControl(byte value)
        {
            await Task.Delay(0);
        }

        public async Task SendMessage(string user, string message)
        {
            await Task.Delay(0);
        }

        public async Task Write(ushort offset, byte value)
        {
            await Task.Delay(0);
        }
    }
}