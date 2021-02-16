using System.Threading.Tasks;
using HardwareCore;

namespace RemoteDisplayConnector
{
    public interface IRemoteDisplayConnection
    {
        bool IsConnected {get;}
        Task Clear();
        Task Initialise();
        Task RenderCharacter(ushort address, byte value);
        Task SendDisplayMode(DisplayMode mode);
        Task SendCursorPosition(CursorPosition position);
        Task SendControl(byte control);
    }
}