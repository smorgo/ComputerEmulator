using System;

namespace HardwareCore
{
    public interface IKeyboardInput
    {
        EventHandler<byte> KeyDown {get;set;}
        EventHandler<byte> KeyUp {get;set;}
    }
}
