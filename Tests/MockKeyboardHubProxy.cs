using HardwareCore;
using System.Threading.Tasks;

namespace Tests
{
    public class MockKeyboardHubProxy : IKeyboardHub
    {
        public async Task SendKeyboardControl(byte status)
        {
            await Task.Delay(0);
        }
    }
}