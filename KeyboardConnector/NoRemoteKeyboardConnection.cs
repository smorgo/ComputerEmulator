using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HardwareCore;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace KeyboardConnector
{

    public class NoRemoteKeyboardConnection : IRemoteConnection, IRemoteKeyboard
    {
        public EventHandler<KeyPress> OnKeyUp {get; set;}
        public EventHandler<KeyPress> OnKeyDown {get; set;}
        public EventHandler OnRequestControl {get; set;}

        public bool IsConnected => true;
        private ILogger _logger;

        public NoRemoteKeyboardConnection(ILogger<RemoteKeyboardConnection> logger)
        {
            _logger = logger;
        }
        public async Task ConnectAsync(string url)
        {
            await Task.Delay(0);
        }

        public async Task SendControlRegister(byte value)
        {
            await Task.Delay(0);
        }

        public async Task GenerateKeyUp(string key)
        {
            await Task.Delay(0);
        }

        public async Task GenerateKeyDown(string key)
        {
            await Task.Delay(0);
        }
    }
}