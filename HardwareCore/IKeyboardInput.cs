using System;

namespace HardwareCore
{
    public interface IKeyboardInput
    {
        EventHandler<byte> OnKeyDown {get;set;}
        EventHandler<byte> OnKeyUp {get;set;}
        EventHandler OnStatusRequest {get;}        
    }
}
