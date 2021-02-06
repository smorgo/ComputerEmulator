using _6502;
using HardwareCore;
using System;
using KeyboardConnector;
using RemoteDisplayConnector;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Timers;
using System.Collections.Generic;

namespace Repl
{
    public class ReplHost
    {
        private CPU6502 _cpu;
        private MemoryMappedDisplay _display;
        private MemoryMappedKeyboard _keyboard;
        private IRemoteConnection _keyboardConnection;
        private AddressMap mem;
        const ushort CHAR_TO_PRINT = 0x0050;
        const ushort ARITH_RESULT = 0x0052;
        const ushort ARITH_OPERAND_1 = 0x0054;        
        const ushort DISPLAY_BASE_ADDR = 0xF000;
        const ushort KEYBOARD_BASE_ADDR = 0x84;
        const ushort PROG_START = 0x8000;
        const ushort DISPLAY_SIZE = 0x400;  // 1kB
        const byte NoCarryNoOverflow = 0x00;
        const byte CarryNoOverflow = 0x01;
        const byte NoCarryOverflow = 0x40;
        const byte CarryOverflow = 0x41;
        private List<ushort> _breakpoints;
        public async Task Initialise()
        {
            mem = new AddressMap();
            mem.Install(new Ram(0x0000, 0x10000));
            _display = new MemoryMappedDisplay(DISPLAY_BASE_ADDR, DISPLAY_SIZE);
            mem.Install(_display);

            // This is interactive, so we want the RemoteKeyboardConnection
            _keyboardConnection = new RemoteKeyboardConnection();
            _keyboard = new MemoryMappedKeyboard(KEYBOARD_BASE_ADDR, _keyboardConnection);
            mem.Install(_keyboard);
            await mem.Initialise();
            await _display.Clear();
            _cpu = new CPU6502(mem);
            _cpu.DebugLevel = DebugLevel.Verbose;
            _keyboard.RequestInterrupt += async (s,e) => {await _cpu.Interrupt();};

            mem.WriteWord(_cpu.RESET_VECTOR, PROG_START);

            mem.Labels = new LabelTable();
            mem.Labels.Add("DISPLAY_CONTROL_ADDR", MemoryMappedDisplay.DISPLAY_CONTROL_BLOCK_ADDR);
            mem.Labels.Add("DISPLAY_CONTROL_REGISTER", MemoryMappedDisplay.DISPLAY_CONTROL_BLOCK_ADDR + DisplayControlBlock.CONTROL_ADDR);
            mem.Labels.Add("CURSOR_X_REGISTER", MemoryMappedDisplay.DISPLAY_CONTROL_BLOCK_ADDR + DisplayControlBlock.CURSOR_X_ADDR);
            mem.Labels.Add("CURSOR_Y_REGISTER", MemoryMappedDisplay.DISPLAY_CONTROL_BLOCK_ADDR + DisplayControlBlock.CURSOR_Y_ADDR);
            mem.Labels.Add("DISPLAY_BASE_ADDR", DISPLAY_BASE_ADDR);
            mem.Labels.Add("DISPLAY_SIZE", DISPLAY_SIZE);
            mem.Labels.Add("KEYBOARD_STATUS_REGISTER", MemoryMappedKeyboard.STATUS_REGISTER + KEYBOARD_BASE_ADDR);
            mem.Labels.Add("KEYBOARD_CONTROL_REGISTER", MemoryMappedKeyboard.CONTROL_REGISTER + KEYBOARD_BASE_ADDR);
            mem.Labels.Add("KEYBOARD_DATA_REGISTER", MemoryMappedKeyboard.DATA_REGISTER + KEYBOARD_BASE_ADDR);
            mem.Labels.Add("KEYBOARD_SCAN_CODE_REGISTER", MemoryMappedKeyboard.SCAN_CODE_REGISTER + KEYBOARD_BASE_ADDR);
            mem.Labels.Add("RESET_VECTOR", _cpu.RESET_VECTOR);
            mem.Labels.Add("IRQ_VECTOR", _cpu.IRQ_VECTOR);
            mem.Labels.Add("NMI_VECTOR", _cpu.NMI_VECTOR);
            mem.Labels.Add("ARITH_RESULT", ARITH_RESULT);
            mem.Labels.Add("ARITH_OPERAND_1", ARITH_OPERAND_1);
            mem.Labels.Add("CHAR_TO_PRINT", CHAR_TO_PRINT);

            _breakpoints = new List<ushort>();
        }

        public async Task Run()
        {
            Load();

            _cpu.Breakpoints = _breakpoints;
            _cpu.Reset(TimeSpan.FromHours(1)); // Run for a maximum of one hour
 
            await Task.Delay(0);
        }

        public void Load()
        {
            mem.Labels.Push();

            using(var _ = mem.Load(0x8000))
            {
                _

                // Main program
                .JSR("PrintIntro")

                .LDA_ABSOLUTE("CharacterBuffer", "LoopStart")
                .BNE("CharacterInBuffer")
                .NOP()
                .JMP_ABSOLUTE("LoopStart")

                // There is a character in the input buffer (which is now in the accumulator)
                .JSR("PrintCharacterAtCursor", "CharacterInBuffer")
                .JSR("AdvanceCursor")
                .LDA_IMMEDIATE(0x00)
                .STA_ABSOLUTE("CharacterBuffer")
                .JMP_ABSOLUTE("LoopStart")

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
            .Label("AddOperand")
                .CLC()
                .LDA_ABSOLUTE("ARITH_OPERAND_1")
                .ADC_ABSOLUTE("ARITH_RESULT")
                .STA_ABSOLUTE("ARITH_RESULT")
                .LDA_ABSOLUTE("ARITH_OPERAND_1+1")
                .ADC_ABSOLUTE("ARITH_RESULT+1")
                .STA_ABSOLUTE("ARITH_RESULT+1")
                .RTS()
            .Label("MultiplyXbyY")
                .PHA()
                .TXA()
                .PHA()
                .TYA()
                .PHA()
                .PHP()
                .TXA()
                .LSR_ACCUMULATOR()
                .STA_ABSOLUTE("ARITH_RESULT")
                .STY_ABSOLUTE("ARITH_OPERAND_1")
                .LDA_IMMEDIATE(0x00)
                .LDY_IMMEDIATE(0x08)
            .Label("MultiplyLoop")
                .BCC("MultiplyNoAdd")
                .CLC()
                .ADC_ABSOLUTE("ARITH_OPERAND_1")
            .Label("MultiplyNoAdd")
                .ROR_ACCUMULATOR()
                .ROR_ABSOLUTE("ARITH_RESULT")
                .DEY()
                .BNE("MultiplyLoop")
                .STA_ABSOLUTE("ARITH_OPERAND_1+1")
                .PLP()
                .PLA()
                .TAY()
                .PLA()
                .TAX()
                .PLA()
                .RTS()
            .Label("AdvanceCursor")
                .INC_ZERO_PAGE("CURSOR_X_REGISTER")
                .LDA_ZERO_PAGE("CURSOR_X_REGISTER")
                .CMP_IMMEDIATE((byte)_display.Mode.Width)
                .BNE("DontAdvanceRow")
            .Label("MoveToNextRow")
                .LDA_IMMEDIATE(0x00)
                .STA_ZERO_PAGE("CURSOR_X_REGISTER")
                .INC_ZERO_PAGE("CURSOR_Y_REGISTER")
                .CMP_IMMEDIATE((byte)_display.Mode.Height)
                .BNE("DontAdvanceRow")
            .Label("ScrollScreen")
                // Don't do anything for now. Just wrap around.
                .LDA_IMMEDIATE(0x00)
                .STA_ZERO_PAGE("CURSOR_Y_REGISTER")
            .Label("DontAdvanceRow")
                .RTS()
            .Label("PrintIntro")
                .Macro(ClearScreen)
                .LDX_IMMEDIATE(0x00)
                .LDA_ABSOLUTE_X("INTRO_TEXT", "Loop")
                .BEQ("Finished")
                .STA_ZERO_PAGE("CHAR_TO_PRINT")
                .TXA()
                .PHA("bp1")
                .LDA_ZERO_PAGE("CHAR_TO_PRINT")
                .JSR("PrintCharacterAtCursor")
                .JSR("AdvanceCursor")
                .PLA("bp2")
                .TAX()
                .INX()
                .BNE("Loop")
                .RTS("Finished")
            .Label("PrintCharacterAtCursor")
                // A contains the character to print
                // Cursor position is from the DisplayControlBlock
                .LDX_ZERO_PAGE("CURSOR_X_REGISTER")
                .LDY_ZERO_PAGE("CURSOR_Y_REGISTER")
            .Label("PrintCharacterAtXY")
                .PHA("bp3") // Push the character to print
                .TXA()
                .PHA() // Push X for now
                .LDX_IMMEDIATE((byte)_display.Mode.Width)
                .JSR("MultiplyXbyY") // Result is in ARITH_RESULT
                .LDA_IMMEDIATE("DISPLAY_BASE_ADDR:LO")
                .STA_ZERO_PAGE("ARITH_OPERAND_1")
                .LDA_IMMEDIATE("DISPLAY_BASE_ADDR:HI")
                .STA_ZERO_PAGE("ARITH_OPERAND_1+1")
                .JSR("AddOperand")
                .LDA_IMMEDIATE(0x00)
                .STA_ZERO_PAGE("ARITH_OPERAND_1")
                .PLA("bp4") // Pull X
                .STA_ZERO_PAGE("ARITH_OPERAND_1+1")
                .JSR("AddOperand") // Add result should now contain the address of the cursor position
                .PLA("bp5") // Pull the character to print
                .LDY_IMMEDIATE(0x00)
                .STA_INDIRECT_Y("ARITH_RESULT")
                .RTS()
                // Data
                .WriteString("Computer Emulator", "INTRO_TEXT")
                .Write(0x00)
                .WriteWord(0x10, (ushort)(DISPLAY_BASE_ADDR + _display.Mode.Width), "CharacterVector") // Set to the start of the second line
                .Write(0xE00, 0x00, "CharacterBuffer")
                // Wire up ISR
                .Ref(_cpu.IRQ_VECTOR, "ISR");
            }


            _breakpoints.Add(mem.Labels.Resolve("AdvanceCursor"));
/*
            _breakpoints.Add(mem.Labels.Resolve("bp2"));
            _breakpoints.Add(mem.Labels.Resolve("bp3"));
            _breakpoints.Add(mem.Labels.Resolve("bp4"));
            _breakpoints.Add(mem.Labels.Resolve("bp5"));
*/            
            mem.Labels.Pop();

        }

        private void ClearScreen(ushort address, Loader _)
        {
            _
                .LDA_IMMEDIATE(DisplayControlBlock.ControlBits.CLEAR_SCREEN)
                .STA_ZERO_PAGE("DISPLAY_CONTROL_REGISTER");
        }

    }
}