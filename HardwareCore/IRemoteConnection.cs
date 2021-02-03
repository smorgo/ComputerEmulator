using System;
using System.Threading.Tasks;

namespace HardwareCore
{
    public interface IRemoteConnection
    {
        bool IsConnected {get;}
        Task ConnectAsync(string url);
    }
}