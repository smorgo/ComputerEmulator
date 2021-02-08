using _6502;
using HardwareCore;
using System;
using KeyboardConnector;
using RemoteDisplayConnector;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Timers;
using Debugger;
using System.Threading;
using Memory;
using Microsoft.Extensions.Logging;

namespace Repl
{
    public class ReplHost : IEmulatorHost
    {
        private CPU6502 _cpu;
        public IDebuggableCpu Cpu => _cpu;
        private IMemoryMappedDisplay _display;
        private MemoryMappedKeyboard _keyboard;
        private IRemoteConnection _keyboardConnection;
        private IAddressMap mem;
        public IMemoryDebug Memory => (IMemoryDebug)mem;
        public ILabelMap Labels {get; private set;}
        const ushort DISPLAY_BASE_ADDR = 0xF000;
        const ushort KEYBOARD_BASE_ADDR = 0x84;
        const ushort PROG_START = 0x8000;
        const ushort DISPLAY_SIZE = 0x400;  // 1kB
        const byte NoCarryNoOverflow = 0x00;
        const byte CarryNoOverflow = 0x01;
        const byte NoCarryOverflow = 0x40;
        const byte CarryOverflow = 0x41;
        private readonly CancellationTokenWrapper _cancellationToken;
        private readonly CpuHoldEvent _debuggerSyncEvent;

        public bool Running {get; private set;}
        public ReplHost(
            ILabelMap labels, 
            CancellationTokenWrapper cancellationToken, 
            CpuHoldEvent debuggerSyncEvent, 
            IDebuggableCpu cpu,
            IAddressMap addressMap,
            IRemoteConnection remoteKeyboardConnection,
            IMemoryMappedDisplay memoryMappedDisplay)
        {
            Labels = labels;
            _cancellationToken = cancellationToken;
            _debuggerSyncEvent = debuggerSyncEvent;
            _cpu = (CPU6502)cpu;
            mem = addressMap;
            _keyboardConnection = remoteKeyboardConnection;
            _display = memoryMappedDisplay;
        }

        public void Start()
        {
            mem.Install(new Ram(0x0000, 0x10000));
            _display.Locate(DISPLAY_BASE_ADDR, DISPLAY_SIZE);
            mem.Install(_display);

            // This is interactive, so we want the RemoteKeyboardConnection
            _keyboard = new MemoryMappedKeyboard(KEYBOARD_BASE_ADDR, _keyboardConnection);
            mem.Install(_keyboard);
            AsyncUtil.RunSync(() => mem.Initialise());
            _display.Clear();
            _cpu.LogLevel = LogLevel.Trace;

            _keyboard.RequestInterrupt += async (s,e) => {await _cpu.Interrupt();};

            mem.WriteWord(_cpu.RESET_VECTOR, PROG_START);

            mem.Labels.Clear();
            mem.Labels.Add("DISPLAY_CONTROL_ADDR", MemoryMappedDisplay.DISPLAY_CONTROL_BLOCK_ADDR);
            mem.Labels.Add("DISPLAY_CONTROL_REGISTER", MemoryMappedDisplay.DISPLAY_CONTROL_BLOCK_ADDR + DisplayControlBlock.CONTROL_ADDR);
            mem.Labels.Add("CURSOR_X_REGISTER", MemoryMappedDisplay.DISPLAY_CONTROL_BLOCK_ADDR + DisplayControlBlock.CURSOR_X_ADDR);
            mem.Labels.Add("CURSOR_Y_REGISTER", MemoryMappedDisplay.DISPLAY_CONTROL_BLOCK_ADDR + DisplayControlBlock.CURSOR_X_ADDR);
            mem.Labels.Add("DISPLAY_BASE_ADDR", DISPLAY_BASE_ADDR);
            mem.Labels.Add("DISPLAY_SIZE", DISPLAY_SIZE);
            mem.Labels.Add("KEYBOARD_STATUS_REGISTER", MemoryMappedKeyboard.STATUS_REGISTER + KEYBOARD_BASE_ADDR);
            mem.Labels.Add("KEYBOARD_CONTROL_REGISTER", MemoryMappedKeyboard.CONTROL_REGISTER + KEYBOARD_BASE_ADDR);
            mem.Labels.Add("KEYBOARD_DATA_REGISTER", MemoryMappedKeyboard.DATA_REGISTER + KEYBOARD_BASE_ADDR);
            mem.Labels.Add("KEYBOARD_SCAN_CODE_REGISTER", MemoryMappedKeyboard.SCAN_CODE_REGISTER + KEYBOARD_BASE_ADDR);
            mem.Labels.Add("RESET_VECTOR", _cpu.RESET_VECTOR);
            mem.Labels.Add("IRQ_VECTOR", _cpu.IRQ_VECTOR);
            mem.Labels.Add("NMI_VECTOR", _cpu.NMI_VECTOR);

            Run();
        }

        public void Run()
        {
            Load();

            Running = true;

            _cpu.Reset(TimeSpan.FromHours(1)); // Run for a maximum of one hour
        }

        public void Load()
        {
            mem.Labels.Push();

            using(var _ = mem.Load(0x8000))
            {
                _

                // Main program
                .JSR("PrintIntro")
                .LDX_IMMEDIATE(0x00)
                .STA_ABSOLUTE("CURSOR_X_REGISTER")
                .INX()
                .STA_ABSOLUTE("CURSOR_Y_REGISTER")

                .LDA_ABSOLUTE("CharacterBuffer", "LoopStart")
                .BNE("CharacterInBuffer")
                .NOP()
                .JMP_ABSOLUTE("LoopStart")

                // There is a character in the input buffer (which is now in the accumulator)
                .JSR("PrintCharacter", "CharacterInBuffer")
                .LDA_IMMEDIATE(0x00)
                .STA_ABSOLUTE("CharacterBuffer")
                .JMP_ABSOLUTE("LoopStart")

                // For now, don't use the cursor position.
                // Just write to the character vector and increment it
                .LDX_IMMEDIATE(0x00, "PrintCharacter")
                .STA_INDIRECT_X("CharacterVector")
                .CLC()
                .LDA_IMMEDIATE(0x01)
                .ADC_ZERO_PAGE("CharacterVector")
                .STA_ZERO_PAGE("CharacterVector")
                .LDA_IMMEDIATE(0x00)
                .ADC_ZERO_PAGE("CharacterVector+1")
                .STA_ZERO_PAGE("CharacterVector+1")
                .RTS()

                // ISR
                // Save the registers (P and SP are already saved)
                .PHA("ISR")
                .TXA()
                .PHA()
                .TYA()
                .PHA()

                // Check for keyboard data
                .LDA_ZERO_PAGE("KEYBOARD_STATUS_REGISTER", "CheckKeyboard")
                .AND_IMMEDIATE((byte)MemoryMappedKeyboard.StatusBits.KeyUp) // Only interested in KeyUp events
                .BEQ("NoKeyboardCharacter")

                // Copy the new character into the buffer
                .LDA_ZERO_PAGE("KEYBOARD_DATA_REGISTER")
                .STA_ABSOLUTE("CharacterBuffer")

                // Clear the keyboard status bits
                .LDA_IMMEDIATE(0x00)
                .STA_ZERO_PAGE("KEYBOARD_STATUS_REGISTER")
                
                // Done
                .PLA("NoKeyboardCharacter")
                .TAY()
                .PLA()
                .TAX()
                .PLA()
                .RTI()

                .LDA_IMMEDIATE(DisplayControlBlock.ControlBits.CLEAR_SCREEN, "PrintIntro")
                .STA_ZERO_PAGE("DISPLAY_CONTROL_REGISTER")
                .LDX_IMMEDIATE(0x00)
                .LDA_ABSOLUTE_X("INTRO_TEXT", "Loop")
                .BEQ("Finished")
                .STA_ABSOLUTE_X(DISPLAY_BASE_ADDR)
                .INX()
                .BNE("Loop")
                .RTS("Finished")

                // Data
                .WriteString("Computer Emulator", "INTRO_TEXT")
                .Write(0x00)
                .WriteWord(0x10, (ushort)(DISPLAY_BASE_ADDR + _display.Mode.Width), "CharacterVector") // Set to the start of the second line
                .Write(0xE00, 0x00, "CharacterBuffer")
                // Wire up ISR
                .Ref(_cpu.IRQ_VECTOR, "ISR");
            }

            GenerateDebuggerLabels();
            mem.Labels.Pop();

        }

        private void GenerateDebuggerLabels()
        {
            foreach(var name in mem.Labels.Names)
            {
                Labels.Add(new Label(name, mem.Labels.Resolve(name)));
            }            
        }
    }
}