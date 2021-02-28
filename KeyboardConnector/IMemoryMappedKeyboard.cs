using System;
using System.Threading.Tasks;
using HardwareCore;

namespace KeyboardConnector
{
    public interface IMemoryMappedKeyboard
    {
        EventHandler<byte> KeyDown {get; set;}
        EventHandler<byte> KeyUp {get; set;}
        EventHandler RequestInterrupt {get; set;}
        ushort StartAddress {get; set;}
        void GenerateKeyDown(string key);
        void GenerateKeyUp(string key);
        byte Read(ushort address);
        void Write(ushort address, byte value);
        Task SendControlRegister();
        Task SendControlRegister(byte value);
        Task OnKeyUp(KeyPress keyPress);
        Task OnKeyDown(KeyPress keyPress);

    }
}
