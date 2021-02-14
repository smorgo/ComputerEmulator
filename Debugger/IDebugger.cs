using System.Threading.Tasks;

namespace Debugger
{
    public interface IDebugger
    {
        void Start();
        ILabelMap Labels {get;}
        Task OnProgramBreakpointTriggered(object sender, ProgramBreakpointEventArgs e);
        Task OnMemoryBreakpointTriggered(object sender, MemoryBreakpointEventArgs e);
    }
}
