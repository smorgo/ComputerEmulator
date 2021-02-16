using NUnit.Framework;
using _6502;
using HardwareCore;
using System;
using KeyboardConnector;
using RemoteDisplayConnector;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Timers;
using Memory;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Debugger;
using SignalRConnection;

namespace Tests
{
    public class RemoteKeyboardTests
    {
        private CPU6502 _cpu;
        private IMemoryMappedDisplay _display;
        private MemoryMappedKeyboard _keyboard;
        private IRemoteConnection _keyboardConnection;
        private SignalRIntegration _signalr;
        private IAddressMap mem;
        const ushort DISPLAY_BASE_ADDR = 0xF000;
        const ushort KEYBOARD_BASE_ADDR = 0x84;
        const ushort PROG_START = 0x8000;
        const ushort DISPLAY_SIZE = 0x400;  // 1kB
        const byte NoCarryNoOverflow = 0x00;
        const byte CarryNoOverflow = 0x01;
        const byte NoCarryOverflow = 0x40;
        const byte CarryOverflow = 0x41;
        private ServiceProvider _serviceProvider;

        public RemoteKeyboardTests()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
            ServiceProviderLocator.ServiceProvider = _serviceProvider;
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services
                 .AddLogging()
                 .AddScoped<IDebuggableCpu, CPU6502>()
                 .AddScoped<IAddressMap, AddressMap>()
                 .AddScoped<ILoaderLabelTable, LoaderLabelTable>()
                 .AddScoped<ILabelMap, LabelMap>()
                 .AddScoped<ILogFormatter, DebugLogFormatter>()
                 .AddScoped<IParser, Parser>()
                 .AddScoped<IRemoteConnection, RemoteKeyboardConnection>()
                 .AddScoped<IMemoryMappedDisplay, MockMemoryMappedDisplay>()
                 .AddScoped<IRemoteDisplayConnection, NoRemoteDisplayConnection>()
                 .AddTransient<ISignalRHubConnection,MockSignalRHubConnection>()
                 .AddTransient<ILoader, Loader>()
                 .AddScoped<ICpuHoldEvent,MockCpuHoldEvent>()
                 .AddScoped<ICpuStepEvent,MockCpuStepEvent>()
                 .AddScoped<IRegisterTracker, NoRegisterTracker>()
                 .AddSingleton<CancellationTokenWrapper>(new CancellationTokenWrapper());
        }

        [SetUp]
        public async Task Setup()
        {
            mem = _serviceProvider.GetService<IAddressMap>();
            mem.Install(new Ram(0x0000, 0x10000));
            _display = _serviceProvider.GetService<IMemoryMappedDisplay>();
            _display.Locate(DISPLAY_BASE_ADDR, DISPLAY_SIZE);
            mem.Install(_display);
            _keyboardConnection = _serviceProvider.GetService<IRemoteConnection>();
            _keyboard = new MemoryMappedKeyboard(KEYBOARD_BASE_ADDR, _keyboardConnection);
            mem.Install(_keyboard);
            await mem.Initialise();
            _display.Clear();
            _cpu = (CPU6502)_serviceProvider.GetService<IDebuggableCpu>();
            _cpu.LogLevel = LogLevel.Trace;
            _keyboard.RequestInterrupt += async (s,e) => {await _cpu.Interrupt(s,e);};

            mem.WriteWord(_cpu.RESET_VECTOR, PROG_START);

            mem.Labels.Add("DISPLAY_CONTROL_ADDR", MemoryMappedDisplay.DISPLAY_CONTROL_BLOCK_ADDR);
            mem.Labels.Add("DISPLAY_BASE_ADDR", DISPLAY_BASE_ADDR);
            mem.Labels.Add("DISPLAY_SIZE", DISPLAY_SIZE);
            mem.Labels.Add("KEYBOARD_STATUS_REGISTER", MemoryMappedKeyboard.STATUS_REGISTER + KEYBOARD_BASE_ADDR);
            mem.Labels.Add("KEYBOARD_CONTROL_REGISTER", MemoryMappedKeyboard.CONTROL_REGISTER + KEYBOARD_BASE_ADDR);
            mem.Labels.Add("KEYBOARD_DATA_REGISTER", MemoryMappedKeyboard.DATA_REGISTER + KEYBOARD_BASE_ADDR);
            mem.Labels.Add("KEYBOARD_SCAN_CODE_REGISTER", MemoryMappedKeyboard.SCAN_CODE_REGISTER + KEYBOARD_BASE_ADDR);
            mem.Labels.Add("RESET_VECTOR", _cpu.RESET_VECTOR);
            mem.Labels.Add("IRQ_VECTOR", _cpu.IRQ_VECTOR);
            mem.Labels.Add("NMI_VECTOR", _cpu.NMI_VECTOR);

            _signalr = new SignalRIntegration(_keyboardConnection);
            await _signalr.Initialise();
        }

        [Test]
        public void NoRemoteKeyboardIsConnected()
        {
            Assert.IsTrue(_keyboardConnection.IsConnected);
        }

        [Test]
        public async Task NoRemoteKeyboardDoesNothing()
        {
            var somethingHappened = false;
            var keyboard = (IRemoteKeyboard)_keyboardConnection;
            keyboard.OnKeyDown += (s,e) => {somethingHappened = true;};
            keyboard.OnKeyUp += (s,e) => {somethingHappened = true;};
            keyboard.OnRequestControl += (s,e) => {somethingHappened = true;};

            await _keyboardConnection.ConnectAsync("Any URL");
            await keyboard.GenerateKeyDown("a");
            await keyboard.GenerateKeyUp("b");
            await keyboard.SendControlRegister(1);

            Assert.IsFalse(somethingHappened);

        }
   }
}