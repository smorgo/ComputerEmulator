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
using System.Collections.Generic;
using KeyboardConnector;

namespace Tests
{
    public class MemoryMappedDisplayTests
    {
        private CPU6502 _cpu;
        private MockCpuHoldEvent _cpuHoldEvent;
        private MockCpuStepEvent _cpuStepEvent;

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
        private CancellationTokenWrapper _cancellationTokenWrapper;
        public MemoryMappedDisplayTests()
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
                 .AddScoped<IMemoryMappedDisplay, MemoryMappedDisplay>()
                 .AddScoped<IRemoteDisplayConnection, MockRemoteDisplayConnection>()
                 .AddScoped<IRegisterTracker, DebugRegisterTracker>()
                 .AddTransient<ILoader, Loader>()
                 .AddScoped<ICpuHoldEvent,MockCpuHoldEvent>()
                 .AddScoped<ICpuStepEvent,MockCpuStepEvent>()
                 .AddScoped<CancellationTokenWrapper, CancellationTokenWrapper>();
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
            _display.SetMode(DisplayMode.Mode7);
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
            _cpuHoldEvent = (MockCpuHoldEvent)_serviceProvider.GetService<ICpuHoldEvent>();
            _cpuStepEvent = (MockCpuStepEvent)_serviceProvider.GetService<ICpuStepEvent>();
            _cpuHoldEvent.Init();
            _cpuStepEvent.Init();
            _cancellationTokenWrapper = _serviceProvider.GetService<CancellationTokenWrapper>();
            _cancellationTokenWrapper.Reset();
        }

        [Test]
        public void CanRenderViaMemoryMappedDisplay()
        {
            ushort address = 0x0000;
            byte value = 0x00;

            var connection = (MockRemoteDisplayConnection)_serviceProvider.GetService<IRemoteDisplayConnection>();

            connection.OnRender += (s,e) => {
                address = e.Address;
                value = e.Value;
            };

            mem.Write(DISPLAY_BASE_ADDR + 1, 0x55);

            Assert.AreEqual(0x0001, address);
            Assert.AreEqual(0x55, value);
        }

        [Test]
        public void CanClearScreenViaMemoryMappedDisplay()
        {
            var cleared = false;

            var connection = (MockRemoteDisplayConnection)_serviceProvider.GetService<IRemoteDisplayConnection>();
            connection.OnClear += (s,e) => { cleared = true; };

            var address = (ushort)(MemoryMappedDisplay.DISPLAY_CONTROL_BLOCK_ADDR + DisplayControlBlock.CONTROL_ADDR);

            var value = mem.Read(address);
            value |= DisplayControlBlock.ControlBits.CLEAR_SCREEN;

            mem.Write(address, value);
            
            Assert.IsTrue(cleared);
        }

        [Test]
        public void RequestForDisplayMode7ReturnsCorrectMode()
        {
            var mode = DisplayMode.GetMode(7);
            Assert.AreEqual(7, mode.Mode);
        }
        [Test]
        public void RequestForInvalidDisplayMode7ReturnsNull()
        {
            var mode = DisplayMode.GetMode(99);
            Assert.IsNull(mode);
        }
        [Test]
        public void CanGetDisplayMode()
        {
            Assert.AreEqual(7, _display.Mode.Mode);
        }
        [Test]
        public void CanSetDisplayMode()
        {
            var address = (ushort)(MemoryMappedDisplay.DISPLAY_CONTROL_BLOCK_ADDR + DisplayControlBlock.MODE_ADDR);
            mem.Write(address, 8);
            Assert.AreEqual(8, _display.Mode.Mode);
        }

        [Test]
        public void CanMoveCursorViaMemoryMappedDisplayXPosition()
        {
            var x = 0;
            var y = 0;

            var connection = (MockRemoteDisplayConnection)_serviceProvider.GetService<IRemoteDisplayConnection>();
            connection.OnCursorPosition += (s,e) => { 
                x = e.X;
                y = e.Y; 
            };

            var address = (ushort)(MemoryMappedDisplay.DISPLAY_CONTROL_BLOCK_ADDR + DisplayControlBlock.CURSOR_X_ADDR);

            mem.Write(address, 20);
            
            Assert.AreEqual(20,x);
            Assert.AreEqual(0,y);
        }
        [Test]
        public void CanMoveCursorViaMemoryMappedDisplayYPosition()
        {
            var x = 0;
            var y = 0;

            var connection = (MockRemoteDisplayConnection)_serviceProvider.GetService<IRemoteDisplayConnection>();
            connection.OnCursorPosition += (s,e) => { 
                x = e.X;
                y = e.Y; 
            };

            var address = (ushort)(MemoryMappedDisplay.DISPLAY_CONTROL_BLOCK_ADDR + DisplayControlBlock.CURSOR_Y_ADDR);

            mem.Write(address, 20);
            
            Assert.AreEqual(0,x);
            Assert.AreEqual(20,y);
        }

        [Test]
        public void CanControlDisplayViaMemoryMappedDisplay()
        {
            byte control = 0x00;

            var connection = (MockRemoteDisplayConnection)_serviceProvider.GetService<IRemoteDisplayConnection>();
            connection.OnControl += (s,e) => { control = e; };

            var address = (ushort)(MemoryMappedDisplay.DISPLAY_CONTROL_BLOCK_ADDR + DisplayControlBlock.CONTROL_ADDR);

            mem.Write(address, 0xff);
            
            // Clear screen bit will have been zeroed
            Assert.AreEqual(0xfb, control);
        }

        [Test]
        public void CanSetModeDirectly()
        {
            _display.SetMode(DisplayMode.Mode7);
            Assert.AreEqual(7, _display.Mode.Mode);
            _display.SetMode(DisplayMode.Mode8);
            Assert.AreEqual(8, _display.Mode.Mode);
        }

        [Test]
        public void CheckCanReadVideoRam()
        {
            var ram = new VideoRam(null, 0, 0x0000, 0x100);
            Assert.IsTrue(ram.CanRead);
        }

        [Test]
        public void CheckVideoRamReadReturnsWritten()
        {
            var ram = new VideoRam(null, 0, 0x0000, 0x100);
            ram.Write(0x0001, 0x59);
            Assert.AreEqual(0x59, ram.Read(0x0001));
        }
   }
}