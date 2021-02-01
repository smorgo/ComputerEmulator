using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HardwareCore
{
    public interface IAddressAssignment
    {
        List<IAddressableBlock> Blocks {get;}
        void Write(int blockId, ushort address, Byte value);
        Byte Read(int blockId, ushort address);
        Task Initialise();
    }
}