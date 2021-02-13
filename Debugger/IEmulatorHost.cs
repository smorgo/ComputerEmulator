using System.Threading;
using System.Threading.Tasks;
using HardwareCore;

namespace Debugger
{
    public interface IEmulatorHost
    {
        IDebuggableCpu Cpu {get;}
        IAddressMap Memory {get;}
        ILabelMap Labels {get;}
        void Start();
        bool Running {get;}
    }
}
