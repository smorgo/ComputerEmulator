using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SignalRConnection;

namespace Tests
{
    public class MockSignalRHubConnection : ISignalRHubConnection
    {
        public bool IsConnected {get; private set;}
        public MockSignalRHubConnection()
        {
        }
        public async Task Connect(string url)
        {
            IsConnected = true;
            await Task.Delay(0);
        }

        public async Task InvokeAsync(string methodName)
        {
            await Task.Delay(0);
        }
        public async Task InvokeAsync(string methodName, object arg1)
        {
            await Task.Delay(0);
        }
        public async Task InvokeAsync(string methodName, object arg1, object arg2)
        {
            await Task.Delay(0);
        }
        public void On<T,P>(string methodName, Action<T,P> action)
        {
        }
        public void On(string methodName, Action action)
        {
        }
    }
}