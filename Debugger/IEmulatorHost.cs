using System.Threading;
using System.Threading.Tasks;

namespace Debugger
{
    public interface IEmulatorHost
    {
        IDebuggableCpu Cpu {get;}
        IMemoryDebug Memory {get;}
        ILabelMap Labels {get;}
        void Start();
        bool Running {get;}
    }
}
