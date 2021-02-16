using System;
using System.Threading.Tasks;

namespace SignalRConnection
{
    public interface ISignalRHubConnection
    {
        bool IsConnected {get;}
        Task Connect(string url);
        Task InvokeAsync(string methodName);
        Task InvokeAsync(string methodName, object arg1);
        Task InvokeAsync(string methodName, object arg1, object arg2);
        void On<T,P>(string methodName, Action<T,P> action);
        void On(string methodName, Action action);
    }
}
