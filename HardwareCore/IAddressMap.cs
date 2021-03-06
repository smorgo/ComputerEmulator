using System;
using System.Threading.Tasks;

namespace HardwareCore
{
    public interface IAddressMap
    {
        void Reset();
        bool CanRead { get; }
        bool CanWrite { get; }
        ushort StartAddress { get; }
        uint Size { get; }
        ushort LowWaterMark { get; }
        ushort HighWaterMark { get; }
        ILoaderLabelTable Labels { get; set; }
        Task Initialise();
        void Install(IAddressAssignment device);
        byte Read(ushort address);
        ushort ReadWord(ushort address);
        byte[] ReadBlock(ushort startAddress, ushort endAddress);
        void ResetWatermarks();
        void Write(ushort address, byte value);
        void WriteWord(ushort address, ushort value);
        ILoader Load();
        ILoader Load(ushort startAddress);
        EventHandler<MemoryChangedEventArgs> MemoryChanged {get;set;}
    }
}