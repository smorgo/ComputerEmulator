using System;

namespace HardwareCore
{
    public static class BoolExtensions
    {
        public static void ThenIfFalse(this bool value, Action action)
        {
            if(!value) action();
        }
    }
}