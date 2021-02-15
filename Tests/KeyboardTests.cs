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

namespace Tests
{
    public class KeyboardTests
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

        public KeyboardTests()
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
                 .AddScoped<IRemoteConnection, MockRemoteKeyboardConnection>()
                 .AddScoped<IMemoryMappedDisplay, MockMemoryMappedDisplay>()
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
        public async Task CanGetKeyDownUp()
        {
            // This test doesn't seem to like being debugged!
            var start = DateTime.Now;
            var timeout = start.AddSeconds(30);
            bool done = false;
            bool didKeyDown = false;
            bool triggered = false;
            byte key = 0x00;

            Debug.WriteLine("Starting");

            _keyboard.KeyDown += async (s, e) => {
                didKeyDown = true;
                key = e;
                Debug.WriteLine($"KeyDown({e})");
                Console.WriteLine($"KeyDown({e})");
                await Task.Delay(10);
                await _signalr.KeyUp(new string((char)(e),1));
             };

            _keyboard.KeyUp += async (s, e) => {
                done = true;
                key = e;
                Debug.WriteLine($"KeyUp({e})");
                Console.WriteLine($"KeyUp({e})");
                await Task.Delay(0);
             };

            Debug.WriteLine("Starting loop");
            while(!done)
            {
                await Task.Delay(10);

                if(DateTime.Now > timeout)
                {
                    Debug.WriteLine("Timeout waiting for KeyDown");
                    Console.WriteLine("Timeout waiting for KeyDown");
                    break;
                }

                if(!triggered)
                {
                    await _signalr.KeyDown("a");
                    triggered = true;
                }

                Debug.Write(".");
            }

            Debug.WriteLine("Done");
            Assert.IsTrue(done);
            Assert.IsTrue(didKeyDown);
            Assert.AreEqual('a', key);
        }

        [Test]
        public async Task CanGetKeyInInterruptServiceRoutine()
        {
            mem.Labels.Push();
            _cpu.NopDelayMilliseconds = 100;

            using(var loader = mem.Load(0x8000))
            {
                loader

                // Main program
                .LDA_ABSOLUTE("CharacterBuffer", "LoopStart")
                .BNE("GotCharacter")
                .NOP()
                .NOP()
                .NOP()
                .NOP()
                .JMP_ABSOLUTE("LoopStart")
                .BRK("GotCharacter")

                // ISR
                // Save the registers (P and SP are already saved)
                .Macro(SaveRegisters, "ISR")

                // Check for keyboard data
                .LDA_ZERO_PAGE("KEYBOARD_STATUS_REGISTER")
                .AND_IMMEDIATE((byte)MemoryMappedKeyboard.StatusBits.KeyUp) // Only interested in KeyUp events
                .BEQ("NoKeyboardCharacter")

                // Copy the new character into the buffer
                .LDA_ZERO_PAGE("KEYBOARD_DATA_REGISTER")
                .STA_ABSOLUTE("CharacterBuffer")

                // Clear the keyboard status bits
                .LDA_IMMEDIATE(0x00)
                .STA_ZERO_PAGE("KEYBOARD_STATUS_REGISTER")

                // Done
                .Macro(RestoreRegisters, "NoKeyboardCharacter")

                // Data
                .Write(0xE00, 0x00, "CharacterBuffer")

                // Wire up ISR
                .Ref(_cpu.IRQ_VECTOR, "ISR");
            }

            // We use a timer to generate a KeyUp event, 3 seconds after the CPU
            // has started running the main code.
            var timer = new System.Timers.Timer(3000) // Wait 3 seconds, then trigger KeyUp
            {
                AutoReset = false
            };

            timer.Elapsed += async (s,e) => 
            {
                await _signalr.KeyUp("a");
            };

            _cpu.OnStarted += (s,e) => {timer.Start();};

            _cpu.Reset(TimeSpan.FromMinutes(1)); // Run for a maximum of one minute

            var buf = mem.Labels.Resolve("CharacterBuffer");

            Assert.AreEqual('a', mem.Read(buf));

            mem.Labels.Pop();

            await Task.Delay(0);
        }

        private void SaveRegisters(ushort address, Loader _)
        {
            _
                .PHA()
                .TXA()
                .PHA()
                .TYA()
                .PHA();
        }
        private void RestoreRegisters(ushort address, Loader _)
        {
            _
                .PLA()
                .TAY()
                .PLA()
                .TAX()
                .PLA();
        }
    }
}