using System.Collections.Generic;
using System.Threading.Tasks;
using HardwareCore;

namespace RemoteDisplayConnector
{
    public interface IMemoryMappedDisplay : IAddressAssignment
    {
        void Locate(ushort address, uint size);
        DisplayMode Mode { get; }
        void Clear();
        void SetMode(DisplayMode mode);
    }
}