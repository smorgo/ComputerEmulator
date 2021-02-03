using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HardwareCore;
using KeyboardConnector;
using Microsoft.AspNetCore.SignalR.Client;

namespace Tests
{
    public class SignalRIntegration
    {
        private IRemoteConnection _connection;
        private IRemoteKeyboard _keyboard;
        public SignalRIntegration(IRemoteConnection connection)
        {
            _connection = connection;
            _keyboard = (IRemoteKeyboard)connection;
        }

        public async Task Initialise()
        {
            await _connection.ConnectAsync("https://localhost:5001/display");
        }

        public async Task KeyUp(string key)
        {
            await _keyboard.GenerateKeyUp(key);
        }

        public async Task KeyDown(string key)
        {
            await _keyboard.GenerateKeyDown(key);
        }
    }
}