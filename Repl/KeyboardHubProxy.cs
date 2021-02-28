using System.Threading.Tasks;
using HardwareCore;
using Microsoft.AspNetCore.SignalR;
using Repl.Hubs;

namespace Repl
{
    public class KeyboardHubProxy : IKeyboardHub
    {
        private IHubContext<DisplayHub> _hubContext;
        
        public KeyboardHubProxy(IHubContext<DisplayHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public async Task SendKeyboardControl(byte status)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveKeyboardControl", status);
        }
    }
}
