using System;
using System.Diagnostics;
using System.Threading.Tasks;
using HardwareCore;
using Microsoft.AspNetCore.SignalR.Client;

namespace KeyboardConnector
{
    public class RemoteKeyboardConnection : IRemoteConnection, IRemoteKeyboard, IDisposable
    {
        public EventHandler<string> OnKeyUp {get; set;}
        public EventHandler<string> OnKeyDown {get; set;}
        public EventHandler OnRequestControl {get; set;}

        private HubConnection _connection;

        public bool IsConnected {get; private set;}

        public async Task ConnectAsync(string url)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(url)
                .Build();
        
            _connection.Closed += async (error) =>
            {
                IsConnected = false;

                await Task.Delay(new Random().Next(0,5) * 1000);
                try
                {
                    await _connection.StartAsync();
                    Debug.Assert(_connection.State == HubConnectionState.Connected);
                    IsConnected = true;
                }
                catch     (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Console.WriteLine("Unable to reach remote display");
                }
            };

            _connection.On<string>("KeyUp", (e) => OnKeyUp?.Invoke(this, e)); 
            _connection.On<string>("KeyDown", (e) => OnKeyDown?.Invoke(this, e)); 
            _connection.On("RequestControl", () => OnRequestControl?.Invoke(this, null)); 

            try
            {
                await _connection.StartAsync();
                Debug.Assert(_connection.State == HubConnectionState.Connected);
                IsConnected = true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Console.WriteLine("Unable to reach remote display");
                IsConnected = false;
            }
        }

        public async Task SendControlRegister(byte value)
        {
            await _connection.InvokeAsync("ReceiveKeyboardControl", value);
        }

        public void Dispose()
        {
        }

        public async Task GenerateKeyUp(string key)
        {
            await _connection.InvokeAsync("KeyUp", key);
        }

        public async Task GenerateKeyDown(string key)
        {
            await _connection.InvokeAsync("KeyDown", key);
        }
    }
}