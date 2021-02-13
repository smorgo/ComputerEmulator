using System;

namespace HardwareCore
{
    public interface IRegisterTracker
    {

        EventHandler<RegisterUpdatedEventArgs> RegisterUpdated {get; set;}
        void PostRegisterUpdated(string register, ushort value);
    }
}