using NUnit.Framework;
using _6502;
using HardwareCore;
using System;
using RemoteDisplayConnector;
using System.Threading.Tasks;
using FilePersistence;
using System.Collections.Generic;

namespace Tests
{
    public class FilePersistenceTests
    {
        private  CPU6502 _cpu;
        private MemoryMappedDisplay _display;
        private AddressMap mem;
        private ILoaderPersistence _persistence;
        const ushort DISPLAY_BASE_ADDR = 0xF000;
        const ushort PROG_START = 0x8000;
        const ushort DISPLAY_SIZE = 0x400;  // 1kB

        [SetUp]
        public async Task Setup()
        {
            mem = new AddressMap();
            mem.Install(new Ram(0x0000, 0x10000));
            _display = new MemoryMappedDisplay(DISPLAY_BASE_ADDR, DISPLAY_SIZE);
            mem.Install(_display);
            await _display.Initialise();
            _display.Clear();
            _cpu = new CPU6502(mem);
            _cpu.DebugLevel = DebugLevel.Verbose;
            mem.WriteWord(_cpu.RESET_VECTOR, PROG_START);
            _persistence = new MemoryFilePersistence
            {
                WorkingDirectory = "~/6502Programs"
            };
        }


        [Test]
        public void CanSaveTestProgram()
        {
            Assert.IsTrue(_display.Mode.Type == DisplayMode.RenderType.Text);

            var w = _display.Mode.Width;
            var h = _display.Mode.Height;
            var bpr = _display.Mode.BytesPerRow;
            Dictionary<string, ushort> labels;
            
            mem.Load(PROG_START)
                // Write column header
                .Write(OPCODE.LDX_IMMEDIATE, "StartOfProgram")
                .Write(0x00)
                .Write(OPCODE.JSR)
                .Ref("ResetDigit")

                .Write(OPCODE.LDA_ZERO_PAGE, "ColumnLoop")
                .ZeroPageRef("CurrentDigit")
                .Write(OPCODE.STA_ABSOLUTE_X)
                .WriteWord(DISPLAY_BASE_ADDR)
                .Write(OPCODE.INX)
                .Write(OPCODE.CPX_IMMEDIATE)
                .Write((byte)w)
                .Write(OPCODE.BCS)
                .RelativeRef("DoneColumns")

                .Write(OPCODE.JSR)
                .Ref("IncrementDigit")
                .Write(OPCODE.JMP_ABSOLUTE)
                .Ref("ColumnLoop")

                // Write row labels
                .Write(OPCODE.JSR, "DoneColumns")
                .Ref("ResetDigit")
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(h-1)

                .Write(OPCODE.CLC, "IncrementRowAddress")
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(bpr.Lsb())
                .Write(OPCODE.ADC_ZERO_PAGE)
                .ZeroPageRef("DisplayVector")
                .Write(OPCODE.STA_ZERO_PAGE)
                .ZeroPageRef("DisplayVector")
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(bpr.Msb())
                .Write(OPCODE.ADC_ZERO_PAGE)
                .ZeroPageRef("DisplayVector",1)
                .Write(OPCODE.STA_ZERO_PAGE)
                .ZeroPageRef("DisplayVector",1)

                .Write(OPCODE.JSR)
                .Ref("IncrementDigit")

                .Write(OPCODE.LDA_ZERO_PAGE)
                .ZeroPageRef("CurrentDigit")
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.STA_INDIRECT_X)
                .ZeroPageRef("DisplayVector")
                
                .Write(OPCODE.DEY)
                .Write(OPCODE.BNE)
                .RelativeRef("IncrementRowAddress")
                .Write(OPCODE.BRK, "Finished")

                // Subroutine ResetDigit
                .Write(OPCODE.LDA_IMMEDIATE, "ResetDigit")
                .Write('0')
                .Write(OPCODE.STA_ZERO_PAGE)
                .ZeroPageRef("CurrentDigit")
                .Write(OPCODE.RTS)

                // Subroutine IncrementDigit
                .Write(OPCODE.LDA_ZERO_PAGE, "IncrementDigit")
                .ZeroPageRef("CurrentDigit")
                .Write(OPCODE.CMP_IMMEDIATE)
                .Write('9')
                .Write(OPCODE.BCS)
                .RelativeRef("ResetDigit") // Sneakily jump to the reset routine
                .Write(OPCODE.INC_ZERO_PAGE)
                .ZeroPageRef("CurrentDigit")
                .Write(OPCODE.RTS)
                .Write(OPCODE.BRK, "EndOfProgram")

                .Write(0x10, '0', "CurrentDigit")
                .WriteWord(0x12, DISPLAY_BASE_ADDR, "DisplayVector")
                .Fixup(out labels);

            var start = labels["StartOfProgram"];
            var end = labels["EndOfProgram"];
            var length = (ushort)(end - start);

            _persistence.Save("TestProgram.bin", start, length, mem);
        }

        [Test]
        public void CanLoadAndRunTestProgram()
        {
            Assert.IsTrue(_display.Mode.Type == DisplayMode.RenderType.Text);

            var w = _display.Mode.Width;
            var h = _display.Mode.Height;

            _persistence.Load("TestProgram.bin", mem);

            // Initialise Working Memory
            mem.Load()
                .Write(0x10, '0')
                .WriteWord(0x12, DISPLAY_BASE_ADDR);

            _cpu.Reset();
            Assert.AreEqual('0', mem.Read(DISPLAY_BASE_ADDR));
            var expected = (h + 9) % 10 + '0';
            Assert.AreEqual(expected, mem.Read((ushort)(DISPLAY_BASE_ADDR + (h-1) * w)));
        }
    }
}