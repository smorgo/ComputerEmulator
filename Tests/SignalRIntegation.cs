using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Tests
{
    public class SignalRIntegration
    {
        private HubConnection _connection;
        public async Task Initialise()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5001/display")
                .Build();
        
            _connection.Closed += async (error) =>
            {
                await Task.Delay(1000);
                try
                {
                    await _connection.StartAsync();
                    Debug.Assert(_connection.State == HubConnectionState.Connected);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Console.WriteLine("Unable to reach remote display");
                }
            };

            try
            {
                await _connection.StartAsync();
                Debug.Assert(_connection.State == HubConnectionState.Connected);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Console.WriteLine("Unable to reach remote display");
            }
        }

        public async Task KeyUp(string key)
        {
            await _connection.InvokeAsync("KeyUp", key);
        }

        public async Task KeyDown(string key)
        {
            await _connection.InvokeAsync("KeyDown", key);
        }
    }
}