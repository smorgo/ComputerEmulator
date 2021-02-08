using System.Collections.Generic;
using System.Threading.Tasks;
using HardwareCore;

namespace RemoteDisplayConnector
{
    public interface IMemoryMappedDisplay : IAddressAssignment
    {
        void Locate(ushort address, uint size);
        DisplayMode Mode { get; }
        List<IAddressableBlock> Blocks { get; }
        void Clear();
        Task Initialise();
        byte Read(int blockId, ushort address);
        Task SetMode(DisplayMode mode);
        void Write(int blockId, ushort address, byte value);
    }
}