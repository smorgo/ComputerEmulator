using System;
using HardwareCore;

namespace Debugger
{
    public class DebugRegisterTracker : IRegisterTracker
    {
        public EventHandler<RegisterUpdatedEventArgs> RegisterUpdated { get; set; }

        public void PostRegisterUpdated(string register, ushort value)
        {
            RegisterUpdated?.Invoke(this, new RegisterUpdatedEventArgs(register, value));
        }
    }
}
