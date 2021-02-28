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
    public class CpuEventTests
    {
        private CPU6502 _cpu;
        private ICpuHoldEvent _cpuHoldEvent;
        private ICpuStepEvent _cpuStepEvent;

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
        public CpuEventTests()
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
                 .AddScoped<IRegisterTracker, NoRegisterTracker>()
                 .AddTransient<ILoader, Loader>()
                 .AddScoped<ICpuHoldEvent,CpuHoldEvent>()
                 .AddScoped<ICpuStepEvent,CpuStepEvent>()
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
            _cpuHoldEvent = _serviceProvider.GetService<ICpuHoldEvent>();
            _cpuStepEvent = _serviceProvider.GetService<ICpuStepEvent>();
            _cancellationTokenWrapper = _serviceProvider.GetService<CancellationTokenWrapper>();
            _cancellationTokenWrapper.Reset();
        }

        [Test]
        public void CanTimeoutStepEvent()
        {
            _cpuStepEvent.Reset();
            Assert.IsFalse(_cpuStepEvent.WaitOne(TimeSpan.FromSeconds(2)));
        }

        [Test]
        public void CanTimeoutHoldEvent()
        {
            _cpuHoldEvent.Reset();
            Assert.IsFalse(_cpuHoldEvent.WaitOne(TimeSpan.FromSeconds(2)));
        }

        [Test]
        public void CpuFailsOnHoldEventTimeout()
        {
            using(var _ = mem.Load(0x8000))
            {
                _
                .NOP("loop")
                .JMP_ABSOLUTE("loop");
            }

            _cpu.MaxEventDuration = TimeSpan.FromSeconds(2);
            _cpuHoldEvent.Reset();
            _cpuStepEvent.Set();

            try
            {
                _cpu.Reset(TimeSpan.FromSeconds(30));
            }
            catch(TimeoutException)
            {
                Assert.Pass();
            }
            Assert.Fail();
        }
        [Test]
        public void CpuFailsOnStepEventTimeout()
        {
            using(var _ = mem.Load(0x8000))
            {
                _
                .NOP("loop")
                .JMP_ABSOLUTE("loop");
            }

            _cpu.MaxEventDuration = TimeSpan.FromSeconds(2);
            _cpuHoldEvent.Set();
            _cpuStepEvent.Reset();
            
            try
            {
                _cpu.Reset(TimeSpan.FromSeconds(30));
            }
            catch(TimeoutException)
            {
                Assert.Pass();
            }
            Assert.Fail();
        }
    }
}