using System;
using System.Threading.Tasks;

namespace HardwareCore
{
    public interface IKeyboardHub
    {
        Task SendKeyboardControl(byte status);
    }
}