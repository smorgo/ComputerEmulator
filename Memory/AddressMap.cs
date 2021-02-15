using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using HardwareCore;
using Debugger;
using Microsoft.Extensions.DependencyInjection;

namespace Memory
{

    public class AddressMap : IAddressMap, IDebuggableMemory
    {
        public bool CanRead => true;

        public bool CanWrite => true;

        public ushort StartAddress => 0x0000;

        public uint Size => 0x10000;

        public ushort LowWaterMark { get; private set; }
        public ushort HighWaterMark { get; private set; }
        public ILoaderLabelTable Labels { get; set; }
        public EventHandler<MemoryChangedEventArgs> MemoryChanged { get; set; }

        public MemoryBreakpoints Breakpoints {get;} = new MemoryBreakpoints();

        public EventHandler<MemoryBreakpointEventArgs> BreakpointTriggered {get; set;}
        private IAddressableBlock[] RedirectionTable = new IAddressableBlock[0x10000]; // This is going to be woefully inefficient in terms of memory
        private List<IAddressAssignment> _installedModules = new List<IAddressAssignment>();
        public AddressMap(ILoaderLabelTable labels)
        {
            Labels = labels;
            ResetWatermarks();
        }

        public async Task Initialise()
        {
            foreach (var module in _installedModules)
            {
                await module.Initialise();
            }
        }

        public void Reset()
        {
            _installedModules.Clear();
            RedirectionTable = new IAddressableBlock[0x10000];
        }
        
        public void Install(IAddressAssignment device)
        {
            _installedModules.Add(device);

            foreach (var block in device.Blocks)
            {
                var jx = block.StartAddress;
                for (var ix = 0; ix < block.Size; ix++)
                {
                    RedirectionTable[jx++] = block;
                }
            }
        }

        public ILoader Load()
        {
            return Load(0x0000);
        }


        public ILoader Load(ushort startAddress)
        {
            var serviceProvider = ServiceProviderLocator.ServiceProvider;
            var loader = serviceProvider.GetService<ILoader>();
            loader.Cursor = startAddress;
            return loader;
        }

        public void ResetWatermarks()
        {
            LowWaterMark = 0xFFFF;
            HighWaterMark = 0x0000;
        }

        public byte Read(ushort address)
        {
            var block = RedirectionTable[address];

            if (block != null && block.CanRead)
            {
                return block.Device.Read(block.BlockId, (ushort)(address - block.StartAddress));
            }

            return 0; // Could return some random value for noise.
        }

        public void Write(ushort address, byte value)
        {
            var block = RedirectionTable[address];

            if (block != null && block.CanWrite)
            {
                block.Device.Write(block.BlockId, (ushort)(address - block.StartAddress), value);
            }

            Debug.WriteLine($"[{address:X4}] <- {value:X2}");

            if (address < LowWaterMark)
            {
                LowWaterMark = address;
            }

            if (address > HighWaterMark)
            {
                HighWaterMark = address;
            }

            MemoryChanged?.Invoke(this, new MemoryChangedEventArgs(address, value));
            EvaluateBreakpoints(address, value);
        }

        public void WriteWord(ushort address, ushort value)
        {
            Debug.Assert(address < Size - 1);
            Write(address, (byte)(value & 0xff));
            Write((ushort)(address + 1), (byte)(value >> 8));
        }

        public ushort ReadWord(ushort address)
        {
            Debug.Assert(address < Size - 1);
            return (ushort)(Read(address) + 256 * Read((ushort)(address + 1)));
        }

        public byte[] ReadBlock(ushort startAddress, ushort endAddress)
        {
            var end = Math.Max(startAddress, endAddress);
            var start = Math.Min(startAddress, endAddress);
            var size = end - start + 1;

            var result = new byte[size];

            for(var ix = 0; ix < size; ix++)
            {
                result[ix] = Read((ushort)(start + ix));
            }

            return result;
        }

        public void ClearBreakpoints()
        {
            Breakpoints.Clear();
        }

        public bool AddBreakpoint(MemoryBreakpoint breakpoint)
        {
            Breakpoints.Add(breakpoint);
            return true;
        }

        public bool DeleteBreakpoint(MemoryBreakpoint breakpoint)
        {
            Breakpoints.Remove(breakpoint);
            return true;
        }

        private void EvaluateBreakpoints(ushort address, byte value)
        {
            if(Breakpoints.Count == 0 || BreakpointTriggered == null)
            {
                return;
            }

            foreach(var breakpoint in Breakpoints)
            {
                if(breakpoint.ShouldBreakOnMemoryWrite(address, value))
                {
                    BreakpointTriggered.Invoke(this, new MemoryBreakpointEventArgs(breakpoint, address, value));
                }
            }
        }
    }
}