using System.Threading.Tasks;

namespace HardwareCore
{
    public interface IDisplayHub
    {
        Task Clear();
        Task ReceiveCursorPosition(int x, int y);
        Task ReceiveDisplayMode(DisplayMode mode);
        Task SendControl(byte value);
        Task SendMessage(string user, string message);
        Task Write(ushort offset, byte value);
    }
}