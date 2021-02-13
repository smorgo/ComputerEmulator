using System;

namespace HardwareCore
{
    public class NoRegisterTracker : IRegisterTracker
    {
        public EventHandler<RegisterUpdatedEventArgs> RegisterUpdated { get; set; }

        public void PostRegisterUpdated(string register, ushort value)
        {
            // Do nothing
        }
    }
}