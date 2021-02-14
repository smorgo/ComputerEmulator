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

namespace Tests
{
    public class ProgramBreakpointTests
    {
        private CPU6502 _cpu;
        private IMemoryMappedDisplay _display;
        private IAddressMap mem;
        const ushort DISPLAY_BASE_ADDR = 0xF000;
        const ushort PROG_START = 0x8000;
        const ushort DISPLAY_SIZE = 0x400;  // 1kB
        const byte NoCarryNoOverflow = 0x00;
        const byte CarryNoOverflow = 0x01;
        const byte NoCarryOverflow = 0x40;
        const byte CarryOverflow = 0x41;
        private ServiceProvider _serviceProvider;
        private UnitTestLogger<CPU6502> _logger;
        public ProgramBreakpointTests()
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
                 .AddScoped<IRegisterTracker, NoRegisterTracker>()
                 .AddTransient<ILoader, Loader>()
                 .AddScoped<ICpuHoldEvent,CpuDontHoldEvent>()
                 .AddScoped<ICpuStepEvent,CpuDontStepEvent>()
                 .AddSingleton<CancellationTokenWrapper>(new CancellationTokenWrapper(default(CancellationToken)));
        }
        [SetUp]
        public void Setup()
        {
            _logger = (UnitTestLogger<CPU6502>)_serviceProvider.GetService<ILogger<CPU6502>>();
            _logger.GetOutput(); // Clear any buffered output

            mem = _serviceProvider.GetService<IAddressMap>();
            mem.Install(new Ram(0x0000, 0x10000));
            
            _display = _serviceProvider.GetService<IMemoryMappedDisplay>();
            _display.Locate(DISPLAY_BASE_ADDR, DISPLAY_SIZE);
            mem.Install(_display);
            AsyncUtil.RunSync(mem.Initialise);
            _display.Clear();
            _cpu = (CPU6502)_serviceProvider.GetService<IDebuggableCpu>();
            _cpu.LogLevel = LogLevel.Trace;
            _cpu.Breakpoints.Clear();
            mem.WriteWord(_cpu.RESET_VECTOR, PROG_START);
            mem.Labels.Clear();
            mem.Labels.Add("DISPLAY_CONTROL_ADDR", MemoryMappedDisplay.DISPLAY_CONTROL_BLOCK_ADDR);
            mem.Labels.Add("DISPLAY_BASE_ADDR", DISPLAY_BASE_ADDR);
            mem.Labels.Add("DISPLAY_SIZE", DISPLAY_SIZE);
            mem.Labels.Add("RESET_VECTOR", _cpu.RESET_VECTOR);
            mem.Labels.Add("IRQ_VECTOR", _cpu.IRQ_VECTOR);
            mem.Labels.Add("NMI_VECTOR", _cpu.NMI_VECTOR);
        }

        
        // [Test]
        // public void CanTriggerMemoryValueEqualsBreakpoint()
        // {
        //     string description = string.Empty;
        //     ushort address = 0x0000;
        //     byte value = 0x00;

        //     MemoryDebugger.AddBreakpoint(new MemoryValueEqualsBreakpoint(0x1234,1,0x5a));

        //     MemoryDebugger.BreakpointTriggered += (s,e) => {
        //         description = e.Breakpoint.Describe(null);
        //         address = e.Address;
        //         value = e.Value;
        //     };

        //     mem.Write(0x1234, 0xa5);
        //     Assert.AreEqual(0x0000, address);
        //     Assert.AreEqual(0x00, value);

        //     mem.Write(0x1234, 0x5a);

        //     Assert.AreEqual("01 MEM_EQUALS $1234==$5A (90)", description);
        //     Assert.AreEqual(0x1234, address);
        //     Assert.AreEqual(0x5a, value);
        // }

        [Test]
        public void CanGetProgramBreakpointByIndex()
        {
            _cpu.AddBreakpoint(new ProgramAddressBreakpoint(0x1234));
            _cpu.AddBreakpoint(new ProgramOpcodeBreakpoint((byte)OPCODE.PHA));

            Assert.IsTrue(_cpu.Breakpoints[1] is ProgramOpcodeBreakpoint);
        }

        [Test]
        public void CantSetProgramBreakpointByIndex()
        {
            _cpu.AddBreakpoint(new ProgramAddressBreakpoint(0x1234));

            try
            {
                _cpu.Breakpoints[0] = new ProgramOpcodeBreakpoint((byte)OPCODE.PHA);
            }
            catch(InvalidOperationException)
            {
                Assert.Pass();
            }

            Assert.Fail();
        }

        [Test]
        public void ProgramBreakpointsAreReadonly()
        {
            Assert.IsTrue(_cpu.Breakpoints.IsReadOnly);
        }

        [Test]
        public void ProgramBreakpointsFindContains()
        {
            var breakpoint = new ProgramAddressBreakpoint(0x1234);
            _cpu.AddBreakpoint(breakpoint);
            Assert.IsTrue(_cpu.Breakpoints.Contains(breakpoint));
        }

        [Test]
        public void ProgramBreakpointsCanCopyToArray()
        {
            _cpu.AddBreakpoint(new ProgramAddressBreakpoint(0x1234));
            _cpu.AddBreakpoint(new ProgramOpcodeBreakpoint((byte)OPCODE.PHA));

            var array = new ProgramBreakpoint[2];
            _cpu.Breakpoints.CopyTo(array, 0);

            Assert.IsTrue(array[0] is ProgramAddressBreakpoint);
            Assert.IsTrue(array[1] is ProgramOpcodeBreakpoint);
        }

        [Test]
        public void ProgramBreakpointsIndexOfFindsBreakpoints()
        {
            var breakpoint1 = new ProgramAddressBreakpoint(0x1234);
            var breakpoint2 = new ProgramOpcodeBreakpoint((byte)OPCODE.PHA);
            var breakpoint3 = new ProgramAddressBreakpoint(0x0123);

            _cpu.AddBreakpoint(breakpoint1);
            _cpu.AddBreakpoint(breakpoint2);

            Assert.AreEqual(0, _cpu.Breakpoints.IndexOf(breakpoint1));
            Assert.AreEqual(1, _cpu.Breakpoints.IndexOf(breakpoint2));
            Assert.AreEqual(-1, _cpu.Breakpoints.IndexOf(breakpoint3));

        }

        [Test]
        public void CantInsertProgramBreakpoint()
        {
            var breakpoint1 = new ProgramAddressBreakpoint(0x1234);
            var breakpoint2 = new ProgramOpcodeBreakpoint((byte)OPCODE.PHA);
            var breakpoint3 = new ProgramAddressBreakpoint(0x0123);

            _cpu.AddBreakpoint(breakpoint1);
            _cpu.AddBreakpoint(breakpoint2);

            try
            {
                _cpu.Breakpoints.Insert(1, breakpoint3);
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
            var breakpoint1 = new ProgramAddressBreakpoint(0x1234);
            var breakpoint2 = new ProgramOpcodeBreakpoint((byte)OPCODE.PHA);
            var breakpoint3 = new ProgramAddressBreakpoint(0x0123);

            _cpu.AddBreakpoint(breakpoint1);
            _cpu.AddBreakpoint(breakpoint2);
            _cpu.AddBreakpoint(breakpoint3);

            _cpu.Breakpoints.Remove(breakpoint2);

            Assert.AreEqual(2, _cpu.Breakpoints.Count);
            Assert.IsTrue(_cpu.Breakpoints[0] is ProgramAddressBreakpoint);
            Assert.IsTrue(_cpu.Breakpoints[1] is ProgramAddressBreakpoint);
        }
        [Test]
        public void CanRemoveAtMemoryBreakpoint()
        {
            var breakpoint1 = new ProgramAddressBreakpoint(0x1234);
            var breakpoint2 = new ProgramOpcodeBreakpoint((byte)OPCODE.PHA);
            var breakpoint3 = new ProgramAddressBreakpoint(0x0123);

            _cpu.AddBreakpoint(breakpoint1);
            _cpu.AddBreakpoint(breakpoint2);
            _cpu.AddBreakpoint(breakpoint3);

            _cpu.Breakpoints.RemoveAt(1);

            Assert.AreEqual(2, _cpu.Breakpoints.Count);
            Assert.IsTrue(_cpu.Breakpoints[0] is ProgramAddressBreakpoint);
            Assert.IsTrue(_cpu.Breakpoints[1] is ProgramAddressBreakpoint);
        }
        [Test]
        public void CanEnumerateProgramBreakpoints()
        {
            var breakpoint1 = new ProgramAddressBreakpoint(0x1234);
            var breakpoint2 = new ProgramOpcodeBreakpoint((byte)OPCODE.PHA);
            var breakpoint3 = new ProgramAddressBreakpoint(0x0123);
            
            _cpu.AddBreakpoint(breakpoint1);
            _cpu.AddBreakpoint(breakpoint2);
            _cpu.AddBreakpoint(breakpoint3);

            int count = 0;

            var enumerator = _cpu.Breakpoints.GetEnumerator();

            while(true)
            {
                if(!enumerator.MoveNext())
                {
                    break;
                }
                Assert.IsTrue(enumerator.Current is ProgramBreakpoint);
                count++;
            }

            Assert.AreEqual(3, count);
        }
        [Test]
        public void CanEnumerateByTypeProgramBreakpoints()
        {
            var breakpoint1 = new ProgramAddressBreakpoint(0x1234);
            var breakpoint2 = new ProgramOpcodeBreakpoint((byte)OPCODE.PHA);
            var breakpoint3 = new ProgramAddressBreakpoint(0x0123);
            
            _cpu.AddBreakpoint(breakpoint1);
            _cpu.AddBreakpoint(breakpoint2);
            _cpu.AddBreakpoint(breakpoint3);

            int count = 0;

            var enumerator = ((IEnumerable)(_cpu.Breakpoints)).GetEnumerator();

            while(true)
            {
                if(!enumerator.MoveNext())
                {
                    break;
                }
                Assert.IsTrue(enumerator.Current is ProgramBreakpoint);
                count++;
            }

            Assert.AreEqual(3, count);
        }

        [Test]
        public void CanForEachProgramBreakpoints()
        {
            var breakpoint1 = new ProgramAddressBreakpoint(0x1234);
            var breakpoint2 = new ProgramOpcodeBreakpoint((byte)OPCODE.PHA);
            var breakpoint3 = new ProgramAddressBreakpoint(0x0123);
            
            _cpu.AddBreakpoint(breakpoint1);
            _cpu.AddBreakpoint(breakpoint2);
            _cpu.AddBreakpoint(breakpoint3);

            int count = 0;

            foreach(var breakpoint in _cpu.Breakpoints)
            {
                Assert.IsTrue(breakpoint is ProgramBreakpoint);
                count++;
            }

            Assert.AreEqual(3, count);
        }

    }
}