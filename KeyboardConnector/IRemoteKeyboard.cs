using System;
using System.Threading.Tasks;

namespace KeyboardConnector
{
    public interface IRemoteKeyboard
    {
        EventHandler<string> OnKeyUp {get; set;}
        EventHandler<string> OnKeyDown {get; set;}
        EventHandler OnRequestControl {get; set;}
        Task SendControlRegister(byte value);
        Task GenerateKeyUp(string key);
        Task GenerateKeyDown(string key);
    }


}