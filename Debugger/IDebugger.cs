using System.Threading.Tasks;

namespace Debugger
{
    public interface IDebugger
    {
        void Start();
        ILabelMap Labels {get;}
        Task OnHasExecuted(object sender, ExecutedEventArgs e);
    }
}
