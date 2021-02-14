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

namespace Tests
{
    public class ComplexCpuTests
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
        long _tickCount;
        long _interruptTickInterval;
        private ServiceProvider _serviceProvider;
        private UnitTestLogger<CPU6502> _logger;
        public ComplexCpuTests()
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
                 .AddScoped<ICpuHoldEvent,MockCpuHoldEvent>()
                 .AddScoped<ICpuStepEvent,MockCpuStepEvent>()
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
        }

        [Test]
        public void GoTriggersEvents()
        {
            _cpu.Go();
            Assert.IsTrue(_cpuHoldEvent.IsSet);
            Assert.IsTrue(_cpuStepEvent.IsSet);
            Assert.AreEqual(1, _cpuHoldEvent.SetCount);
            Assert.AreEqual(1, _cpuStepEvent.SetCount);
            Assert.AreEqual(0, _cpuHoldEvent.ResetCount);
            Assert.AreEqual(0, _cpuStepEvent.ResetCount);
        }

        [Test]
        public void StepTriggersEvents()
        {
            _cpuHoldEvent.Set();
            _cpu.Step();
            Assert.IsTrue(_cpuHoldEvent.IsSet);
            Assert.IsFalse(_cpuStepEvent.IsSet);
            Assert.AreEqual(2, _cpuHoldEvent.SetCount);
            Assert.AreEqual(1, _cpuStepEvent.SetCount);
            Assert.AreEqual(1, _cpuHoldEvent.ResetCount);
            Assert.AreEqual(1, _cpuStepEvent.ResetCount);
        }

        [Test]
        public void StopTriggersEvents()
        {
            _cpuHoldEvent.Set();
            _cpuStepEvent.Set();
            _cpu.Stop();
            Assert.IsFalse(_cpuHoldEvent.IsSet);
            Assert.IsTrue(_cpuStepEvent.IsSet);
            Assert.AreEqual(1, _cpuHoldEvent.SetCount);
            Assert.AreEqual(2, _cpuStepEvent.SetCount);
            Assert.AreEqual(1, _cpuHoldEvent.ResetCount);
            Assert.AreEqual(0, _cpuStepEvent.ResetCount);
        }

    }
}