using NUnit.Framework;
using _6502;
using HardwareCore;
using System;
using KeyboardConnector;
using RemoteDisplayConnector;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Timers;

namespace Tests
{
    public class KeyboardTests
    {
        private CPU6502 _cpu;
        private MemoryMappedDisplay _display;
        private MemoryMappedKeyboard _keyboard;
        private IRemoteConnection _keyboardConnection;
        private SignalRIntegration _signalr;
        private AddressMap mem;
        const ushort DISPLAY_BASE_ADDR = 0xF000;
        const ushort KEYBOARD_BASE_ADDR = 0x84;
        const ushort PROG_START = 0x8000;
        const ushort DISPLAY_SIZE = 0x400;  // 1kB
        const byte NoCarryNoOverflow = 0x00;
        const byte CarryNoOverflow = 0x01;
        const byte NoCarryOverflow = 0x40;
        const byte CarryOverflow = 0x41;

        [SetUp]
        public async Task Setup()
        {
            mem = new AddressMap();
            mem.Install(new Ram(0x0000, 0x10000));
            _display = new MemoryMappedDisplay(DISPLAY_BASE_ADDR, DISPLAY_SIZE);
            mem.Install(_display);
            _keyboardConnection = new MockRemoteKeyboardConnection();
            _keyboard = new MemoryMappedKeyboard(KEYBOARD_BASE_ADDR, _keyboardConnection);
            mem.Install(_keyboard);
            await mem.Initialise();
            _display.Clear();
            _cpu = new CPU6502(mem);
            _cpu.DebugLevel = DebugLevel.Verbose;
            _keyboard.RequestInterrupt = _cpu.Interrupt;

            mem.WriteWord(_cpu.RESET_VECTOR, PROG_START);

            mem.Labels = new LabelTable();
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

            using(var loader = mem.Load(0x8000))
            {
                loader

                // Main program
                .Write(OPCODE.LDA_ABSOLUTE, "LoopStart")
                .Ref("CharacterBuffer")
                .Write(OPCODE.BNE)
                .RelativeRef("GotCharacter")
                .Write(OPCODE.NOP)
                .Write(OPCODE.NOP)
                .Write(OPCODE.NOP)
                .Write(OPCODE.NOP)
                .Write(OPCODE.JMP_ABSOLUTE)
                .Ref("LoopStart")
                .Write(OPCODE.BRK, "GotCharacter")

                // ISR
                // Save the registers (P and SP are already saved)
                .Write(OPCODE.PHA, "ISR")
                .Write(OPCODE.TXA)
                .Write(OPCODE.PHA)
                .Write(OPCODE.TYA)
                .Write(OPCODE.PHA)

                // Check for keyboard data
                .Write(OPCODE.LDA_ZERO_PAGE)
                .ZeroPageRef("KEYBOARD_STATUS_REGISTER")
                .Write(OPCODE.AND_IMMEDIATE)
                .Write((byte)MemoryMappedKeyboard.StatusBits.KeyUp) // Only interested in KeyUp events
                .Write(OPCODE.BEQ)
                .RelativeRef("NoKeyboardCharacter")

                // Copy the new character into the buffer
                .Write(OPCODE.LDA_ZERO_PAGE)
                .ZeroPageRef("KEYBOARD_DATA_REGISTER")
                .Write(OPCODE.STA_ABSOLUTE)
                .Ref("CharacterBuffer")

                // Clear the keyboard status bits
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.STA_ZERO_PAGE)
                .ZeroPageRef("KEYBOARD_STATUS_REGISTER")

                // Done
                .Write(OPCODE.PLA, "NoKeyboardCharacter")
                .Write(OPCODE.TAY)
                .Write(OPCODE.PLA)
                .Write(OPCODE.TAX)
                .Write(OPCODE.PLA)
                .Write(OPCODE.RTI)

                // Data
                .Write(0xE00, 0x00, "CharacterBuffer")

                // Wire up ISR
                .Ref(_cpu.IRQ_VECTOR, "ISR");
            }

            // We use a timer to generate a KeyUp event, 3 seconds after the CPU
            // has started running the main code.
            var timer = new Timer(3000) // Wait 3 seconds, then trigger KeyUp
            {
                AutoReset = false
            };

            timer.Elapsed += async (s,e) => {await FeedKeyUpOnTimerExpiry(s,e);};

            _cpu.OnStarted += (s,e) => {timer.Start();};

            _cpu.Reset(TimeSpan.FromMinutes(20)); // Run for a maximum of one minute

            var buf = mem.Labels.Resolve("CharacterBuffer");

            Assert.AreEqual('a', mem.Read(buf));

            mem.Labels.Pop();

            await Task.Delay(0);
        }

        private async Task FeedKeyUpOnTimerExpiry(object sender, ElapsedEventArgs e)
        {
            await _signalr.KeyUp("a");
        }
    }
}