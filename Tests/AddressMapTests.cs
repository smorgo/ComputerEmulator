using NUnit.Framework;
using _6502;
using HardwareCore;
using System;
using RemoteDisplayConnector;
using System.Threading.Tasks;
using Memory;
using System.Threading;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.DependencyInjection;
using Debugger;
using System.Collections;
using SignalRConnection;

namespace Tests
{
    public class AddressMapTests
    {
        private CPU6502 _cpu;
        private IMemoryMappedDisplay _display;
        private IAddressMap mem;
        private IDebuggableMemory MemoryDebugger => (IDebuggableMemory)mem;
        const ushort DISPLAY_BASE_ADDR = 0xF000;
        const ushort PROG_START = 0x8000;
        const ushort DISPLAY_SIZE = 0x400;  // 1kB
        const byte NoCarryNoOverflow = 0x00;
        const byte CarryNoOverflow = 0x01;
        const byte NoCarryOverflow = 0x40;
        const byte CarryOverflow = 0x41;
        private ServiceProvider _serviceProvider;
        private UnitTestLogger<CPU6502> _logger;
        public AddressMapTests()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
            ServiceProviderLocator.ServiceProvider = _serviceProvider;
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services
                 .AddScoped(typeof(ILogger<CPU6502>), typeof(UnitTestLogger<CPU6502>))
                 .AddScoped(typeof(ILogger<Loader>), typeof(UnitTestLogger<Loader>))
                 .AddSingleton<ILoaderLabelTable>(new LoaderLabelTable())
                 .AddTransient<IDebuggableCpu, CPU6502>()
                 .AddScoped<IAddressMap, AddressMap>()
                 .AddScoped<IMemoryMappedDisplay, MockMemoryMappedDisplay>()
                 .AddScoped<IRemoteDisplayConnection, NoRemoteDisplayConnection>()
                 .AddTransient<ISignalRHubConnection,MockSignalRHubConnection>()
                 .AddScoped<IRegisterTracker, NoRegisterTracker>()
                 .AddTransient<ILoader, Loader>()
                 .AddScoped<ICpuHoldEvent,MockCpuHoldEvent>()
                 .AddScoped<ICpuStepEvent,MockCpuStepEvent>()
                 .AddSingleton<CancellationTokenWrapper>(new CancellationTokenWrapper());
        }
        [SetUp]
        public void Setup()
        {
            _logger = (UnitTestLogger<CPU6502>)_serviceProvider.GetService<ILogger<CPU6502>>();
            _logger.GetOutput(); // Clear any buffered output

            mem = _serviceProvider.GetService<IAddressMap>();
            mem.Install(new Ram(0x0000, 0x10000));
            
            MemoryDebugger.ClearBreakpoints();

            _display = _serviceProvider.GetService<IMemoryMappedDisplay>();
            _display.Locate(DISPLAY_BASE_ADDR, DISPLAY_SIZE);
            mem.Install(_display);
            AsyncUtil.RunSync(mem.Initialise);
            _display.Clear();
            _cpu = (CPU6502)_serviceProvider.GetService<IDebuggableCpu>();
            _cpu.LogLevel = LogLevel.Trace;
            mem.WriteWord(_cpu.RESET_VECTOR, PROG_START);
            mem.Labels.Clear();
            mem.Labels.Add("DISPLAY_CONTROL_ADDR", MemoryMappedDisplay.DISPLAY_CONTROL_BLOCK_ADDR);
            mem.Labels.Add("DISPLAY_BASE_ADDR", DISPLAY_BASE_ADDR);
            mem.Labels.Add("DISPLAY_SIZE", DISPLAY_SIZE);
            mem.Labels.Add("RESET_VECTOR", _cpu.RESET_VECTOR);
            mem.Labels.Add("IRQ_VECTOR", _cpu.IRQ_VECTOR);
            mem.Labels.Add("NMI_VECTOR", _cpu.NMI_VECTOR);
        }

        [Test]
        public void CanTriggerMemoryChangedBreakpoint()
        {
            string description = string.Empty;
            ushort address = 0x0000;
            byte value = 0x00;

            MemoryDebugger.AddBreakpoint(new MemoryChangedBreakpoint(0x1234,1));

            MemoryDebugger.BreakpointTriggered += (s,e) => {
                description = e.Breakpoint.Describe(null);
                address = e.Address;
                value = e.Value;
            };

            mem.Write(0x1234, 0x5a);

            Assert.AreEqual("01 MEM_CHANGE $1234", description);
            Assert.AreEqual(0x1234, address);
            Assert.AreEqual(0x5a, value);
        }

        [Test]
        public void CanTriggerMemoryValueEqualsBreakpoint()
        {
            string description = string.Empty;
            ushort address = 0x0000;
            byte value = 0x00;

            MemoryDebugger.AddBreakpoint(new MemoryValueEqualsBreakpoint(0x1234,1,0x5a));

            MemoryDebugger.BreakpointTriggered += (s,e) => {
                description = e.Breakpoint.Describe(null);
                address = e.Address;
                value = e.Value;
            };

            mem.Write(0x1234, 0xa5);
            Assert.AreEqual(0x0000, address);
            Assert.AreEqual(0x00, value);

            mem.Write(0x1234, 0x5a);

            Assert.AreEqual("01 MEM_EQUALS $1234==$5A (90)", description);
            Assert.AreEqual(0x1234, address);
            Assert.AreEqual(0x5a, value);
        }

        [Test]
        public void CanGetMemoryBreakpointByIndex()
        {
            MemoryDebugger.AddBreakpoint(new MemoryChangedBreakpoint(0x1234, 1));
            MemoryDebugger.AddBreakpoint(new MemoryValueEqualsBreakpoint(0x2345,1,0x55));

            Assert.IsTrue(MemoryDebugger.Breakpoints[1] is MemoryValueEqualsBreakpoint);
        }

        [Test]
        public void CantSetMemoryBreakpointByIndex()
        {
            MemoryDebugger.AddBreakpoint(new MemoryChangedBreakpoint(0x1234, 1));

            try
            {
                MemoryDebugger.Breakpoints[0] = new MemoryValueEqualsBreakpoint(0x2345,1,0x55);
            }
            catch(InvalidOperationException)
            {
                Assert.Pass();
            }

            Assert.Fail();
        }

        [Test]
        public void MemoryBreakpointsAreReadonly()
        {
            Assert.IsTrue(MemoryDebugger.Breakpoints.IsReadOnly);
        }

        [Test]
        public void MemoryBreakpointsFindContains()
        {
            var breakpoint = new MemoryChangedBreakpoint(0x1234, 1);
            MemoryDebugger.AddBreakpoint(breakpoint);
            Assert.IsTrue(MemoryDebugger.Breakpoints.Contains(breakpoint));
        }

        [Test]
        public void MemoryBreakpointsCanCopyToArray()
        {
            MemoryDebugger.AddBreakpoint(new MemoryChangedBreakpoint(0x1234, 1));
            MemoryDebugger.AddBreakpoint(new MemoryValueEqualsBreakpoint(0x2345,1,0x55));

            var array = new MemoryBreakpoint[2];
            MemoryDebugger.Breakpoints.CopyTo(array, 0);

            Assert.IsTrue(array[0] is MemoryChangedBreakpoint);
            Assert.IsTrue(array[1] is MemoryValueEqualsBreakpoint);
        }

        [Test]
        public void MemoryBreakpointsIndexOfFindsBreakpoints()
        {
            var breakpoint1 = new MemoryChangedBreakpoint(0x1234, 1);
            var breakpoint2 = new MemoryValueEqualsBreakpoint(0x2345,1,0x55);
            var breakpoint3 = new MemoryChangedBreakpoint(0x0123, 10);

            MemoryDebugger.AddBreakpoint(breakpoint1);
            MemoryDebugger.AddBreakpoint(breakpoint2);

            Assert.AreEqual(0, MemoryDebugger.Breakpoints.IndexOf(breakpoint1));
            Assert.AreEqual(1, MemoryDebugger.Breakpoints.IndexOf(breakpoint2));
            Assert.AreEqual(-1, MemoryDebugger.Breakpoints.IndexOf(breakpoint3));

        }

        [Test]
        public void CantInsertMemoryBreakpoint()
        {
            var breakpoint1 = new MemoryChangedBreakpoint(0x1234, 1);
            var breakpoint2 = new MemoryValueEqualsBreakpoint(0x2345,1,0x55);
            var breakpoint3 = new MemoryChangedBreakpoint(0x0123, 10);

            MemoryDebugger.AddBreakpoint(breakpoint1);
            MemoryDebugger.AddBreakpoint(breakpoint2);

            try
            {
                MemoryDebugger.Breakpoints.Insert(1, breakpoint3);
            }
            catch(NotImplementedException)
            {
                Assert.Pass();
            }

            Assert.Fail();
        }

        [Test]
        public void CanRemoveMemoryBreakpoint()
        {
            var breakpoint1 = new MemoryChangedBreakpoint(0x1234, 1);
            var breakpoint2 = new MemoryValueEqualsBreakpoint(0x2345,1,0x55);
            var breakpoint3 = new MemoryChangedBreakpoint(0x0123, 10);

            MemoryDebugger.AddBreakpoint(breakpoint1);
            MemoryDebugger.AddBreakpoint(breakpoint2);
            MemoryDebugger.AddBreakpoint(breakpoint3);

            MemoryDebugger.Breakpoints.Remove(breakpoint2);

            Assert.AreEqual(2, MemoryDebugger.Breakpoints.Count);
            Assert.IsTrue(MemoryDebugger.Breakpoints[0] is MemoryChangedBreakpoint);
            Assert.IsTrue(MemoryDebugger.Breakpoints[1] is MemoryChangedBreakpoint);
        }
        [Test]
        public void CanRemoveAtMemoryBreakpoint()
        {
            var breakpoint1 = new MemoryChangedBreakpoint(0x1234, 1);
            var breakpoint2 = new MemoryValueEqualsBreakpoint(0x2345,1,0x55);
            var breakpoint3 = new MemoryChangedBreakpoint(0x0123, 10);

            MemoryDebugger.AddBreakpoint(breakpoint1);
            MemoryDebugger.AddBreakpoint(breakpoint2);
            MemoryDebugger.AddBreakpoint(breakpoint3);

            MemoryDebugger.Breakpoints.RemoveAt(1);

            Assert.AreEqual(2, MemoryDebugger.Breakpoints.Count);
            Assert.IsTrue(MemoryDebugger.Breakpoints[0] is MemoryChangedBreakpoint);
            Assert.IsTrue(MemoryDebugger.Breakpoints[1] is MemoryChangedBreakpoint);
        }
        [Test]
        public void CanEnumerateMemoryBreakpoints()
        {
            var breakpoint1 = new MemoryChangedBreakpoint(0x1234, 1);
            var breakpoint2 = new MemoryValueEqualsBreakpoint(0x2345,1,0x55);
            var breakpoint3 = new MemoryChangedBreakpoint(0x0123, 10);

            MemoryDebugger.AddBreakpoint(breakpoint1);
            MemoryDebugger.AddBreakpoint(breakpoint2);
            MemoryDebugger.AddBreakpoint(breakpoint3);

            int count = 0;

            var enumerator = MemoryDebugger.Breakpoints.GetEnumerator();

            while(true)
            {
                if(!enumerator.MoveNext())
                {
                    break;
                }
                Assert.IsTrue(enumerator.Current is MemoryBreakpoint);
                count++;
            }

            Assert.AreEqual(3, count);
        }
        [Test]
        public void CanEnumerateByTypeMemoryBreakpoints()
        {
            var breakpoint1 = new MemoryChangedBreakpoint(0x1234, 1);
            var breakpoint2 = new MemoryValueEqualsBreakpoint(0x2345,1,0x55);
            var breakpoint3 = new MemoryChangedBreakpoint(0x0123, 10);

            MemoryDebugger.AddBreakpoint(breakpoint1);
            MemoryDebugger.AddBreakpoint(breakpoint2);
            MemoryDebugger.AddBreakpoint(breakpoint3);

            int count = 0;

            var enumerator = ((IEnumerable)(MemoryDebugger.Breakpoints)).GetEnumerator();

            while(true)
            {
                if(!enumerator.MoveNext())
                {
                    break;
                }
                Assert.IsTrue(enumerator.Current is MemoryBreakpoint);
                count++;
            }

            Assert.AreEqual(3, count);
        }
        [Test]
        public void CanForEachMemoryBreakpoints()
        {
            var breakpoint1 = new MemoryChangedBreakpoint(0x1234, 1);
            var breakpoint2 = new MemoryValueEqualsBreakpoint(0x2345,1,0x55);
            var breakpoint3 = new MemoryChangedBreakpoint(0x0123, 10);

            MemoryDebugger.AddBreakpoint(breakpoint1);
            MemoryDebugger.AddBreakpoint(breakpoint2);
            MemoryDebugger.AddBreakpoint(breakpoint3);

            int count = 0;

            foreach(var breakpoint in MemoryDebugger.Breakpoints)
            {
                Assert.IsTrue(breakpoint is MemoryBreakpoint);
                count++;
            }

            Assert.AreEqual(3, count);
        }

        [Test]
        public void AddressMapCanReadReturnsCorrectResult()
        {
            Assert.IsTrue(mem.CanRead);
        }

        [Test]
        public void AddressMapCanWriteReturnsCorrectResult()
        {
            Assert.IsTrue(mem.CanWrite);
        }

        [Test]
        public void AddressMapStartAddressReturnsCorrectResult()
        {
            Assert.AreEqual(0x0000, mem.StartAddress);
        }

        [Test]
        public void AddressMapReadUnmappedByteReturnsNull()
        {
            mem.Reset();
            Assert.AreEqual(0, mem.Read(0x01));
        }

        [Test]
        public void CanReadBlock()
        {
            mem.WriteWord(0x0000, 0x1234);
            mem.WriteWord(0x0002, 0x2345);
            mem.WriteWord(0x0004, 0x3456);
            mem.WriteWord(0x0006, 0x4567);

            var result = mem.ReadBlock(0x0000, 0x0007);

            Assert.AreEqual(0x34, result[0]);
            Assert.AreEqual(0x12, result[1]);
            Assert.AreEqual(0x45, result[2]);
            Assert.AreEqual(0x23, result[3]);
            Assert.AreEqual(0x56, result[4]);
            Assert.AreEqual(0x34, result[5]);
            Assert.AreEqual(0x67, result[6]);
            Assert.AreEqual(0x45, result[7]);
        }
        [Test]
        public void CanDeleteMemoryBreakpoint()
        {
            var breakpoint1 = new MemoryChangedBreakpoint(0x1234, 1);
            var breakpoint2 = new MemoryValueEqualsBreakpoint(0x2345,1,0x55);
            var breakpoint3 = new MemoryChangedBreakpoint(0x0123, 10);

            MemoryDebugger.AddBreakpoint(breakpoint1);
            MemoryDebugger.AddBreakpoint(breakpoint2);
            MemoryDebugger.AddBreakpoint(breakpoint3);

            MemoryDebugger.DeleteBreakpoint(breakpoint2);

            Assert.AreEqual(2, MemoryDebugger.Breakpoints.Count);
            Assert.IsTrue(MemoryDebugger.Breakpoints[0] is MemoryChangedBreakpoint);
            Assert.IsTrue(MemoryDebugger.Breakpoints[1] is MemoryChangedBreakpoint);
        }

        [Test]
        public void CanReceiveMemoryChangedNotifications()
        {
            ushort address = 0x0000;
            byte value = 0x00;

            mem.MemoryChanged += (s,e) => {
                address = e.Address;
                value = e.Value;
            };

            mem.Write(0x1234, 0x56);

            Assert.AreEqual(0x1234, address);
            Assert.AreEqual(0x56, value);
        }
    }
}