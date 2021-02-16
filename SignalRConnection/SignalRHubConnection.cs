using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace SignalRConnection
{
    public class SignalRHubConnection : ISignalRHubConnection
    {
        public bool IsConnected {get; private set;}
        private HubConnection _connection;
        private ILogger<SignalRHubConnection> _logger;
        public SignalRHubConnection(ILogger<SignalRHubConnection> logger)
        {
            _logger = logger;
        }
        public async Task Connect(string url)
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
                catch(Exception)
                {
                    _logger.LogWarning("Unable to reach remote display");
                }
            };

            try
            {
                await _connection.StartAsync();
                Debug.Assert(_connection.State == HubConnectionState.Connected);
                IsConnected = true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogWarning("Unable to reach remote display");
                IsConnected = false;
            }
        }

        public async Task InvokeAsync(string methodName)
        {
            await _connection.InvokeAsync(methodName);
        }
        public async Task InvokeAsync(string methodName, object arg1)
        {
            await _connection.InvokeAsync(methodName, arg1);
        }
        public async Task InvokeAsync(string methodName, object arg1, object arg2)
        {
            await _connection.InvokeAsync(methodName, arg1, arg2);
        }
        public void On<T,P>(string methodName, Action<T,P> action)
        {
            _connection.On<T,P>(methodName, action);
        }
        public void On(string methodName, Action action)
        {
            _connection.On(methodName, action);
        }
    }
}