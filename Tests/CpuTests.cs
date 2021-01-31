using NUnit.Framework;
using _6502;
using HardwareCore;
using System;
using RemoteDisplayConnector;
using System.Threading.Tasks;

namespace Tests
{
    public class CpuTests
    {
        private  CPU6502 _cpu;
        private MemoryMappedDisplay _display;
        private AddressMap mem;
        const ushort DISPLAY_BASE_ADDR = 0xF000;
        const ushort PROG_START = 0x8000;
        const ushort DISPLAY_SIZE = 0x400;  // 1kB
        const byte NoCarryNoOverflow = 0x00;
        const byte CarryNoOverflow = 0x01;
        const byte NoCarryOverflow = 0x40;
        const byte CarryOverflow = 0x41;
        long _tickCount;
        long _interruptTickInterval;

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
        }

        [Test]
        public void CanReset()
        {
            _cpu.Reset();
            Assert.Pass();
        }

        [Test]
        public void EmulatorReportsInvalidOpcodes()
        {
            mem.Load(PROG_START)
                .Write(0xFF)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x02)
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual(1, _cpu.EmulationErrorsCount);
            Assert.AreEqual(0x02, _cpu.A);
        }

        [Test]
        public void CanLoadAccumulatorImmediate()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x34);
            _cpu.Reset();
            Assert.AreEqual(0x34, _cpu.A);
        }

        [Test]
        public void CanLoadAccumulatorZeroPage()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_ZERO_PAGE)
                .Write(0x34)
                .Write(0x34, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.A);
        }

        [Test]
        public void CanLoadAccumulatorZeroPageX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x1)
                .Write(OPCODE.LDA_ZERO_PAGE_X)
                .Write(0x34)
                .Write(0x35, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.A);
        }

        [Test]
        public void CanLoadAccumulatorAbsolute()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_ABSOLUTE)
                .WriteWord(0x2021)
                .Write(0x2021, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.A);
        }

        [Test]
        public void CanLoadAccumulatorAbsoluteX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x1)
                .Write(OPCODE.LDA_ABSOLUTE_X)
                .WriteWord(0x2021)
                .Write(0x2022, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.A);
        }

        [Test]
        public void CanLoadAccumulatorIndirectX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x2)
                .Write(OPCODE.LDA_INDIRECT_X)
                .Write(0x80)
                .WriteWord(0x82, 0x2021)
                .Write(0x2021, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.A);
        }

        [Test]
        public void CanLoadAccumulatorIndirectY()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x2)
                .Write(OPCODE.LDA_INDIRECT_Y)
                .Write(0x80)
                .WriteWord(0x80, 0x2021)
                .Write(0x2023, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.A);
        }

        [Test]
        public void CanLoadXImmediate()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x34);
            _cpu.Reset();
            Assert.AreEqual(0x34, _cpu.X);
        }

        [Test]
        public void CanLoadXZeroPage()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_ZERO_PAGE)
                .Write(0x34)
                .Write(0x34, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.X);
        }

        [Test]
        public void CanLoadXZeroPageY()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x1)
                .Write(OPCODE.LDX_ZERO_PAGE_Y)
                .Write(0x34)
                .Write(0x35, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.X);
        }

        [Test]
        public void CanLoadXAbsolute()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_ABSOLUTE)
                .WriteWord(0x2021)
                .Write(0x2021, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.X);
        }

        [Test]
        public void CanLoadXAbsoluteY()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x1)
                .Write(OPCODE.LDX_ABSOLUTE_Y)
                .WriteWord(0x2021)
                .Write(0x2022, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.X);
        }

        [Test]
        public void CanLoadYImmediate()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x34);
            _cpu.Reset();
            Assert.AreEqual(0x34, _cpu.Y);
        }

        [Test]
        public void CanLoadYZeroPage()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDY_ZERO_PAGE)
                .Write(0x34)
                .Write(0x34, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.Y);
        }

        [Test]
        public void CanLoadYZeroPageX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x1)
                .Write(OPCODE.LDY_ZERO_PAGE_X)
                .Write(0x34)
                .Write(0x35, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.Y);
        }

        [Test]
        public void CanLoadYAbsolute()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDY_ABSOLUTE)
                .WriteWord(0x2021)
                .Write(0x2021, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.Y);
        }

        [Test]
        public void CanLoadYAbsoluteX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x1)
                .Write(OPCODE.LDY_ABSOLUTE_X)
                .WriteWord(0x2021)
                .Write(0x2022, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.Y);
        }

        [Test]
        public void CanNoOperation()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.NOP)
                .Write(OPCODE.NOP)
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual(PROG_START + 3, _cpu.PC);
        }

        [Test]
        public void CanBreak()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.NOP)
                .Write(OPCODE.BRK)
                .Write(OPCODE.NOP);
            _cpu.Reset();
            Assert.AreEqual(PROG_START + 2, _cpu.PC); 
        }

        [Test]
        public void CanStoreAccumulatorZeroPage()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.STA_ZERO_PAGE)
                .Write(0x10)
                .Write(OPCODE.LDY_ZERO_PAGE)
                .Write(0x10);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.Y);
        }

        [Test]
        public void CanStoreAccumulatorZeroPageX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x02)
                .Write(OPCODE.STA_ZERO_PAGE_X)
                .Write(0x10)
                .Write(OPCODE.LDY_ZERO_PAGE)
                .Write(0x12);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.Y);
        }

        [Test]
        public void CanStoreAccumulatorAbsolute()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.STA_ABSOLUTE)
                .WriteWord(DISPLAY_BASE_ADDR)
                .Write(OPCODE.LDY_ABSOLUTE)
                .WriteWord(DISPLAY_BASE_ADDR);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.Y);
        }

        [Test]
        public void CanStoreAccumulatorAbsoluteX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(1)
                .Write(OPCODE.STA_ABSOLUTE_X)
                .WriteWord(DISPLAY_BASE_ADDR)
                .Write(OPCODE.LDY_ABSOLUTE)
                .WriteWord(DISPLAY_BASE_ADDR+1);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.Y);
        }
        [Test]
        public void CanStoreAccumulatorAbsoluteY()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(1)
                .Write(OPCODE.STA_ABSOLUTE_Y)
                .WriteWord(DISPLAY_BASE_ADDR)
                .Write(OPCODE.LDX_ABSOLUTE)
                .WriteWord(DISPLAY_BASE_ADDR+1);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.X);
        }
        [Test]
        public void CanStoreAccumulatorIndirectX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(2)
                .Write(OPCODE.STA_INDIRECT_X)
                .Write(0x00)
                .Write(OPCODE.LDY_ABSOLUTE)
                .WriteWord(DISPLAY_BASE_ADDR)
                .WriteWord(0x2, DISPLAY_BASE_ADDR);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.Y);
        }
        [Test]
        public void CanStoreAccumulatorIndirectY()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(2)
                .Write(OPCODE.STA_INDIRECT_Y)
                .Write(0x00)
                .Write(OPCODE.LDX_ABSOLUTE)
                .WriteWord(DISPLAY_BASE_ADDR + 2)
                .WriteWord(0x00, DISPLAY_BASE_ADDR);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.X);
        }
        [Test]
        public void CanStoreXZeroPage()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.STX_ZERO_PAGE)
                .Write(0x20)
                .Write(OPCODE.LDA_ZERO_PAGE)
                .Write(0x20);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.A);
        }
        [Test]
        public void CanStoreXZeroPageY()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x10)
                .Write(OPCODE.STX_ZERO_PAGE_Y)
                .Write(0x10)
                .Write(OPCODE.LDA_ZERO_PAGE)
                .Write(0x20);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.A);
        }

        [Test]
        public void CanStoreXAbsolute()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.STX_ABSOLUTE)
                .WriteWord(DISPLAY_BASE_ADDR)
                .Write(OPCODE.LDA_ABSOLUTE)
                .WriteWord(DISPLAY_BASE_ADDR);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.A);
        }
        [Test]
        public void CanStoreYZeroPage()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.STY_ZERO_PAGE)
                .Write(0x20)
                .Write(OPCODE.LDA_ZERO_PAGE)
                .Write(0x20);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.A);
        }
        [Test]
        public void CanStoreYZeroPageX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x10)
                .Write(OPCODE.STY_ZERO_PAGE_X)
                .Write(0x10)
                .Write(OPCODE.LDA_ZERO_PAGE)
                .Write(0x20);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.A);
        }

        [Test]
        public void CanStoreYAbsolute()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.STY_ABSOLUTE)
                .WriteWord(DISPLAY_BASE_ADDR)
                .Write(OPCODE.LDA_ABSOLUTE)
                .WriteWord(DISPLAY_BASE_ADDR);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.A);
        }

        [Test]
        public void CanTransferAccumulatorToX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.TAX);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.X);
        }
        [Test]
        public void CanTransferAccumulatorToY()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.TAY);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.Y);
        }
        [Test]
        public void CanTransferXToAccumulator()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.TXA);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.A);
        }
        [Test]
        public void CanTransferYToAccumulator()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.TYA);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.A);
        }

        [Test]
        public void CanTransferStackPointerToX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x7F)
                .Write(OPCODE.PHA)
                .Write(OPCODE.PHA)
                .Write(OPCODE.TSX)
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual(0xFD, _cpu.X);
        }

        [Test]
        public void CanTransferXToStackPointer()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x7F)
                .Write(OPCODE.PHA)
                .Write(OPCODE.PHA)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x80)
                .Write(OPCODE.TXS)
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual(0x80, _cpu.SP);
        }

        [Test]
        public void CanPushAccumulator()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.PHA)
                .Write(OPCODE.LDY_ABSOLUTE)
                .WriteWord(0x1FF);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.Y);
        }

        [Test]
        public void CanPushProcessorStatus()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.PHP)
                .Write(OPCODE.LDY_ABSOLUTE)
                .WriteWord(0x1FF);
            _cpu.Reset();
            Assert.AreEqual(0x02, _cpu.Y);
        }
        [Test]
        public void CanPullAccumulator()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.PHA)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.PLA);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.A);
        }

        [Test]
        public void CanPullProcessorStatus()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0xFF)
                .Write(OPCODE.PHA)
                .Write(OPCODE.PLP)
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual(0xFF, _cpu.P.AsByte());
        }
        
        [Test]
        public void CanJumpAbsolute()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.JMP_ABSOLUTE)
                .Ref("Continue")
                .Write(OPCODE.BRK)
                .Write(OPCODE.LDA_IMMEDIATE, "Continue")
                .Write('e')
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }
        [Test]
        public void CanJumpIndirect()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.JMP_INDIRECT)
                .Ref("Data")
                .Write(OPCODE.BRK)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('e')
                .WriteWord(0x9000, PROG_START + 6, "Data")
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }

        [Test]
        public void CanJumpToSubroutine()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.JSR)
                .Ref("Sub")
                .Write(OPCODE.BRK)
                .Write(OPCODE.LDA_IMMEDIATE, "Sub")
                .Write('e')
                .Write(OPCODE.BRK)
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
            Assert.AreEqual(0xFD, _cpu.SP);
        }

        [Test]
        public void CanReturnSubroutine()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.JSR)
                .Ref("Sub")
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('l')
                .Write(OPCODE.BRK)
                .Write(OPCODE.LDA_IMMEDIATE, "Sub")
                .Write('e')
                .Write(OPCODE.RTS)
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual((byte)'l', _cpu.A);
            Assert.AreEqual(0xFF, _cpu.SP);
        }

        [Test]
        public void CanSetCarry()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.SEC);
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanClearCarry()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.SEC)
                .Write(OPCODE.CLC);
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C);
        }

        [Test]
        public void CanClearOverflow()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x50)
                .Write(OPCODE.ADC_IMMEDIATE)
                .Write(0x50)
                .Write(OPCODE.CLV);
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.V);
        }

        [Test]
        public void CanSetInterruptDisable()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.SEI);
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.I);
        }

        [Test]
        public void CanClearInterruptDisable()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.SEI)
                .Write(OPCODE.CLI);
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.I);
        }

        [Test]
        public void CanSetDecimal()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.SED);
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.D);
        }

        [Test]
        public void CanClearDecimal()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.SED)
                .Write(OPCODE.CLD);
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.D);
        }

        [Test]
        public void CanBranchForwardsOnCarryClear()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.CLC)
                .Write(OPCODE.BCC)
                .RelativeRef("Cont")
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.BRK)
                .Write(OPCODE.LDA_IMMEDIATE, "Cont")
                .Write('e')
                .Write(OPCODE.BRK)
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }

        [Test]
        public void CanBranchBackwardsOnCarryClear()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.JMP_ABSOLUTE)
                .Ref("Continue")
                .Write(OPCODE.LDA_IMMEDIATE, "Back")
                .Write('e')
                .Write(OPCODE.BRK)
                .Write(OPCODE.CLC, "Continue")
                .Write(OPCODE.BCC)
                .RelativeRef("Back")
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.BRK)
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }

        [Test]
        public void CanBranchForwardsOnCarrySet()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.SEC)
                .Write(OPCODE.BCS)
                .RelativeRef("Cont")
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.BRK)
                .Write(OPCODE.LDA_IMMEDIATE, "Cont")
                .Write('e')
                .Write(OPCODE.BRK)
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }

        [Test]
        public void CanBranchBackwardsOnCarrySet()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.JMP_ABSOLUTE)
                .Ref("Continue")
                .Write(OPCODE.LDA_IMMEDIATE, "Back")
                .Write('e')
                .Write(OPCODE.BRK)
                .Write(OPCODE.SEC, "Continue")
                .Write(OPCODE.BCS)
                .RelativeRef("Back")
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.BRK)
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }

        [Test]
        public void CanBranchOnOverflowClear()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x22)
                .Write(OPCODE.ADC_IMMEDIATE)
                .Write(0x23)
                .Write(OPCODE.ASL_ACCUMULATOR)
                .Write(OPCODE.BVC)
                .RelativeRef("Cont")
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.BRK)
                .Write(OPCODE.LDA_IMMEDIATE, "Cont")
                .Write('e')
                .Write(OPCODE.BRK)
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }

        [Test]
        public void CanBranchOnOverflowSet()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x42)
                .Write(OPCODE.ADC_IMMEDIATE)
                .Write(0x43)
                .Write(OPCODE.ASL_ACCUMULATOR)
                .Write(OPCODE.BVS)
                .RelativeRef("Cont")
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.BRK)
                .Write(OPCODE.LDA_IMMEDIATE, "Cont")
                .Write('e')
                .Write(OPCODE.BRK)
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }


        [Test]
        public void CanBranchForwardsOnMinus()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x88)
                .Write(OPCODE.BMI)
                .RelativeRef("Cont")
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.BRK)
                .Write(OPCODE.LDA_IMMEDIATE, "Cont")
                .Write('e')
                .Write(OPCODE.BRK)
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }

        [Test]
        public void CanBranchBackwardsOnMinus()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.JMP_ABSOLUTE)
                .Ref("Continue")
                .Write(OPCODE.LDA_IMMEDIATE, "Back")
                .Write('e')
                .Write(OPCODE.BRK)
                .Write(OPCODE.LDA_IMMEDIATE, "Continue")
                .Write(0x88)
                .Write(OPCODE.BMI)
                .RelativeRef("Back")
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.BRK)
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }

        [Test]
        public void CanBranchForwardsOnPositive()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x08)
                .Write(OPCODE.BPL)
                .RelativeRef("Cont")
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.BRK)
                .Write(OPCODE.LDA_IMMEDIATE, "Cont")
                .Write('e')
                .Write(OPCODE.BRK)
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }

        [Test]
        public void CanBranchBackwardsOnPositive()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.JMP_ABSOLUTE)
                .Ref("Start")
                .Write(OPCODE.LDA_IMMEDIATE, "Back")
                .Write('e')
                .Write(OPCODE.BRK)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x08)
                .Write(OPCODE.BPL, "Start")
                .RelativeRef("Back")
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.BRK)
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }

        [Test]
        public void CanCompareImmediateEqual()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.CMP_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }
        [Test]
        public void CanCompareImmediateLess()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x90)
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.CMP_IMMEDIATE)
                .Write(0xFF)
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.IsTrue(!_cpu.P.C && !_cpu.P.Z && _cpu.P.N);
        }
        [Test]
        public void CanCompareImmediateGreater()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x90)
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.CMP_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && !_cpu.P.Z && _cpu.P.N);
        }
        [Test]
        public void CanCompareZeroPage()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.CMP_ZERO_PAGE)
                .Write(0x10)
                .Write(OPCODE.BRK)
                .Write(0x10, 'H');
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }
        [Test]
        public void CanCompareZeroPageX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x02)
                .Write(OPCODE.CMP_ZERO_PAGE_X)
                .Write(0x10)
                .Write(OPCODE.BRK)
                .Write(0x12, 'H');
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }

        [Test]
        public void CanCompareAbsolute()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.CMP_ABSOLUTE)
                .Ref("Data")
                .Write(OPCODE.BRK)
                .Write(0x1010, 'H', "Data")
                .Fixup();
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }

        [Test]
        public void CanCompareAbsoluteX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x10)
                .Write(OPCODE.CMP_ABSOLUTE_X)
                .WriteWord(0x1000)
                .Write(OPCODE.BRK)
                .Write(0x1010, 'H');
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }
        [Test]
        public void CanCompareIndirectX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x02)
                .Write(OPCODE.CMP_INDIRECT_X)
                .Write(0x0E)
                .Write(OPCODE.BRK)
                .Write(0x1010, 'H')
                .WriteWord(0x0010, 0x1010);
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }
        [Test]
        public void CanCompareIndirectY()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x02)
                .Write(OPCODE.CMP_INDIRECT_Y)
                .Write(0x10)
                .Write(OPCODE.BRK)
                .Write(0x1012,'H')
                .WriteWord(0x0010, 0x1010);
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }

        [Test]
        public void CanCompareXImmediateEqual()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.CPX_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }
        [Test]
        public void CanCompareXImmediateLess()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x90)
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.CPX_IMMEDIATE)
                .Write(0xFF)
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.IsTrue(!_cpu.P.C && !_cpu.P.Z && _cpu.P.N);
        }
        [Test]
        public void CanCompareXImmediateGreater()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x90)
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.CPX_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && !_cpu.P.Z && _cpu.P.N);
        }
        [Test]
        public void CanCompareXZeroPage()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.CPX_ZERO_PAGE)
                .Write(0x10)
                .Write(OPCODE.BRK)
                .Write(0x10, 'H');
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }

        [Test]
        public void CanCompareXAbsolute()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.CPX_ABSOLUTE)
                .Ref("Data")
                .Write(OPCODE.BRK)
                .Write(0x1010, 'H', "Data")
                .Fixup();
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }

        [Test]
        public void CanCompareYImmediateEqual()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.CPY_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }
        [Test]
        public void CanCompareYImmediateLess()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x90)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.CPY_IMMEDIATE)
                .Write(0xFF)
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.IsTrue(!_cpu.P.C && !_cpu.P.Z && _cpu.P.N);
        }
        [Test]
        public void CanCompareYImmediateGreater()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x90)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.CPY_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && !_cpu.P.Z && _cpu.P.N);
        }
        [Test]
        public void CanCompareYZeroPage()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.CPY_ZERO_PAGE)
                .Write(0x10)
                .Write(OPCODE.BRK)
                .Write(0x10, 'H');
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }

        [Test]
        public void CanCompareYAbsolute()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.CPY_ABSOLUTE)
                .Ref("Data")
                .Write(OPCODE.BRK)
                .Write(0x1010, 'H', "Data")
                .Fixup();
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }

        [Test]
        public void CanBranchEquals()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.CMP_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.BEQ)
                .Write(0x03)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('N')
                .Write(OPCODE.BRK)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('Y')
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual((byte)'Y', _cpu.A);
        }
        [Test]
        public void CanBranchNotEquals()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('H')
                .Write(OPCODE.CMP_IMMEDIATE)
                .Write('e')
                .Write(OPCODE.BNE)
                .Write(0x03)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('N')
                .Write(OPCODE.BRK)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write('Y')
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual((byte)'Y', _cpu.A);
        }

        [Test]
        public void CanBitTestAbsoluteAllMasked()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x0A)
                .Write(OPCODE.BIT_ABSOLUTE)
                .WriteWord(0x1000)
                .Write(OPCODE.BRK)
                .Write(0x1000, 0xF0);
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.Z);
        }

        [Test]
        public void CanBitTestAbsoluteNotAllMasked()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x1A)
                .Write(OPCODE.BIT_ABSOLUTE)
                .WriteWord(0x1000)
                .Write(OPCODE.BRK)
                .Write(0x1000, 0xF0);
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.Z);
        }

        [Test]
        public void CanAddWithCarryImmediate()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x01)
                .Write(OPCODE.ADC_IMMEDIATE)
                .Write(0x02)
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x03, _cpu.A);
        }

        [Test]
        public void CanAddWithCarryImmediateMany()
        {
            const byte NoCarryNoOverflow = 0x00;
            const byte CarryNoOverflow = 0x01;
            const byte NoCarryOverflow = 0x40;
            const byte CarryOverflow = 0x41;

            InnerAddWithCarry(0x50, 0x10, 0x60, NoCarryNoOverflow);
            InnerAddWithCarry(0x50, 0x50, 0xA0, NoCarryOverflow);
            InnerAddWithCarry(0x50, 0x90, 0xE0, NoCarryNoOverflow);
            InnerAddWithCarry(0x50, 0xD0, 0x20, CarryNoOverflow);
            InnerAddWithCarry(0xD0, 0x10, 0xE0, NoCarryNoOverflow);
            InnerAddWithCarry(0xD0, 0x50, 0x20, CarryNoOverflow);
            InnerAddWithCarry(0xD0, 0x90, 0x60, CarryOverflow);
            InnerAddWithCarry(0xD0, 0xD0, 0xA0, CarryNoOverflow);

        }

        private void InnerAddWithCarry(byte v1, byte v2, byte expected, byte statusMask)
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(v1)
                .Write(OPCODE.ADC_IMMEDIATE)
                .Write(v2)
                .Write(OPCODE.BRK);

            _cpu.Reset();

            Assert.AreEqual(expected, _cpu.A);
            Assert.AreEqual(statusMask, _cpu.P.AsByte() & statusMask);
        }

        [TestCase(200,100,300)]
        [TestCase(0,1,1)]
        [TestCase(65535,2,1)]
        public void CanAddMultipleBytes(int v1, int v2, int expectedResult)
        {
            mem.Load(PROG_START)
                .Write(OPCODE.CLC)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x01)
                .Write(OPCODE.LDA_ABSOLUTE_X)
                .Ref("V1")
                .Write(OPCODE.ADC_ABSOLUTE_X)
                .Ref("V2")
                .Write(OPCODE.STA_ABSOLUTE_X)
                .Ref("Result")
                .Write(OPCODE.INX)
                .Write(OPCODE.DEY)
                .Write(OPCODE.BPL)
                .Write(-13)
                .Write(OPCODE.BRK)
                .WriteWord(0x9000, (ushort)v1, "V1" )
                .WriteWord((ushort)v2, "V2")
                .WriteWord(0x0000, "Result")
                .Fixup();
            _cpu.Reset();

            var result = mem.ReadWord(0x9004);

            Assert.AreEqual(expectedResult, result);
        }
        [Test]
        public void CanAddWithCarryZeroPage()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x01)
                .Write(OPCODE.ADC_ZERO_PAGE)
                .Write(0x01)
                .Write(OPCODE.BRK)
                .Write(0x01, 0x02);
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x03, _cpu.A);
        }

        [Test]
        public void CanAddWithCarryZeroPageX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x01)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x02)
                .Write(OPCODE.ADC_ZERO_PAGE_X)
                .Write(0x03)
                .Write(OPCODE.BRK)
                .Write(0x05, 0x02);
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x03, _cpu.A);
        }

        [Test]
        public void CanAddWithCarryAbsolute()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x01)
                .Write(OPCODE.ADC_ABSOLUTE)
                .Ref("Data")
                .Write(OPCODE.BRK)
                .Write(0x1005, 0x02, "Data")
                .Fixup();
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x03, _cpu.A);
        }

        [Test]
        public void CanAddWithCarryAbsoluteX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x01)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x01)
                .Write(OPCODE.ADC_ABSOLUTE_X)
                .WriteWord(0x1004)
                .Write(OPCODE.BRK)
                .Write(0x1005, 0x02);
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x03, _cpu.A);
        }

        [Test]
        public void CanSubtractWithCarryImmediate()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x03)
                .Write(OPCODE.SBC_IMMEDIATE)
                .Write(0x02)
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x01, _cpu.A);
        }

        [TestCase(0x50, 0x10, 0x40, NoCarryNoOverflow)]
        [TestCase(0x50, 0x60, 0xF0, NoCarryOverflow)]
        [TestCase(0xE0, 0x90, 0x50, NoCarryNoOverflow)]
        [TestCase(0x9C, 0xE2, 0xBA, NoCarryNoOverflow)]
        [TestCase(0xE2, 0x9C, 0x46, NoCarryOverflow)]
        public void InnerSubtractWithCarry(byte v1, byte v2, byte expected, byte statusMask)
        {
            Console.WriteLine($"${v1:X2} - ${v2:X2} = ${expected:X2}? {(sbyte)v1}-{(sbyte)v2}={(sbyte)expected}");
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(v1)
                .Write(OPCODE.SBC_IMMEDIATE)
                .Write(v2)
                .Write(OPCODE.BRK);

            _cpu.Reset();

            Assert.AreEqual(expected, _cpu.A);
            Assert.AreEqual(statusMask, _cpu.P.AsByte() & statusMask);
        }

        [TestCase(300,200,100)]
        [TestCase(1,1,0)]
        [TestCase(2,65535,3)]
        public void CanSubtractMultipleBytes(int v1, int v2, int expectedResult)
        {
            mem.Load(PROG_START)
                .Write(OPCODE.CLC)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x01)
                .Write(OPCODE.LDA_ABSOLUTE_X)
                .Ref("V1")
                .Write(OPCODE.SBC_ABSOLUTE_X)
                .Ref("V2")
                .Write(OPCODE.STA_ABSOLUTE_X)
                .Ref("Result")
                .Write(OPCODE.INX)
                .Write(OPCODE.DEY)
                .Write(OPCODE.BPL)
                .Write(-13)
                .Write(OPCODE.BRK)
                .WriteWord(0x9000, (ushort)v1, "V1" )
                .WriteWord((ushort)v2, "V2")
                .WriteWord(0x0000, "Result")
                .Fixup();
            _cpu.Reset();

            var result = mem.ReadWord(0x9004);

            Assert.AreEqual(expectedResult, result);
        }
        [Test]
        public void CanSubtractWithCarryZeroPage()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x05)
                .Write(OPCODE.SBC_ZERO_PAGE)
                .Write(0x01)
                .Write(OPCODE.BRK)
                .Write(0x01, 0x02);
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x03, _cpu.A);
        }

        [Test]
        public void CanSubtractWithCarryZeroPageX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x05)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x02)
                .Write(OPCODE.SBC_ZERO_PAGE_X)
                .Write(0x03)
                .Write(OPCODE.BRK)
                .Write(0x05, 0x02);
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x03, _cpu.A);
        }

        [Test]
        public void CanSubtractWithCarryAbsolute()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x05)
                .Write(OPCODE.SBC_ABSOLUTE)
                .Ref("Data")
                .Write(OPCODE.BRK)
                .Write(0x1005, 0x02, "Data")
                .Fixup();
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x03, _cpu.A);
        }

        [Test]
        public void CanSubtractWithCarryAbsoluteX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x05)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x01)
                .Write(OPCODE.SBC_ABSOLUTE_X)
                .WriteWord(0x1004)
                .Write(OPCODE.BRK)
                .Write(0x1005, 0x02);
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x03, _cpu.A);
        }

        [Test]
        public void CanAddWithCarryAbsoluteY()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x01)
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x01)
                .Write(OPCODE.ADC_ABSOLUTE_Y)
                .WriteWord(0x1004)
                .Write(OPCODE.BRK)
                .Write(0x1005, 0x02);
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x03, _cpu.A);
        }
 
        [Test]
        public void CanAddWithCarryIndirectX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x01)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x02)
                .Write(OPCODE.ADC_INDIRECT_X)
                .Write(0x00)
                .Write(OPCODE.BRK)
                .WriteWord(0x02, 0x1234)
                .Write(0x1234, 0x02);
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x03, _cpu.A);
        }
        [Test]
        public void CanAddWithCarryIndirectY()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x01)
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x02)
                .Write(OPCODE.ADC_INDIRECT_Y)
                .Write(0x08)
                .Write(OPCODE.BRK)
                .WriteWord(0x08, 0x1234)
                .Write(0x1236, 0x02);
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x03, _cpu.A);
        }

        [Test]
        public void CanAndImmediate()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0xFF)
                .Write(OPCODE.AND_IMMEDIATE)
                .Write(0x80)
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual(0x80, _cpu.A);
        }

        [Test]
        public void CanAndZeroPage()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0xFF)
                .Write(OPCODE.AND_ZERO_PAGE)
                .Write(0x10)
                .Write(OPCODE.BRK)
                .Write(0x10, 0x80);
            _cpu.Reset();
            Assert.AreEqual(0x80, _cpu.A);
        }
        [Test]
        public void CanAndZeroPageX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0xFF)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x10)
                .Write(OPCODE.AND_ZERO_PAGE_X)
                .Write(0x00)
                .Write(OPCODE.BRK)
                .Write(0x10, 0x80);
            _cpu.Reset();
            Assert.AreEqual(0x80, _cpu.A);
        }
        [Test]
        public void CanAndAbsolute()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0xFF)
                .Write(OPCODE.AND_ABSOLUTE)
                .WriteWord(0x1010)
                .Write(OPCODE.BRK)
                .Write(0x1010, 0x80);
            _cpu.Reset();
            Assert.AreEqual(0x80, _cpu.A);
        }
        [Test]
        public void CanAndAbsoluteX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0xFF)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x10)
                .Write(OPCODE.AND_ABSOLUTE_X)
                .WriteWord(0x1000)
                .Write(OPCODE.BRK)
                .Write(0x1010, 0x80);
            _cpu.Reset();
            Assert.AreEqual(0x80, _cpu.A);
        }
        [Test]
        public void CanAndAbsoluteY()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0xFF)
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x10)
                .Write(OPCODE.AND_ABSOLUTE_Y)
                .WriteWord(0x1000)
                .Write(OPCODE.BRK)
                .Write(0x1010, 0x80);
            _cpu.Reset();
            Assert.AreEqual(0x80, _cpu.A);
        }

        [Test]
        public void CanOrImmediate()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x0F)
                .Write(OPCODE.ORA_IMMEDIATE)
                .Write(0x80)
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual(0x8F, _cpu.A);
        }

        [Test]
        public void CanOrZeroPage()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x0F)
                .Write(OPCODE.ORA_ZERO_PAGE)
                .Write(0x10)
                .Write(OPCODE.BRK)
                .Write(0x10, 0x80);
            _cpu.Reset();
            Assert.AreEqual(0x8F, _cpu.A);
        }
        [Test]
        public void CanOrZeroPageX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x0F)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x10)
                .Write(OPCODE.ORA_ZERO_PAGE_X)
                .Write(0x00)
                .Write(OPCODE.BRK)
                .Write(0x10, 0x80);
            _cpu.Reset();
            Assert.AreEqual(0x8F, _cpu.A);
        }
        [Test]
        public void CanOrAbsolute()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x0F)
                .Write(OPCODE.ORA_ABSOLUTE)
                .WriteWord(0x1010)
                .Write(OPCODE.BRK)
                .Write(0x1010, 0x80);
            _cpu.Reset();
            Assert.AreEqual(0x8F, _cpu.A);
        }
        [Test]
        public void CanOrAbsoluteX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x0F)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x10)
                .Write(OPCODE.ORA_ABSOLUTE_X)
                .WriteWord(0x1000)
                .Write(OPCODE.BRK)
                .Write(0x1010, 0x80);
            _cpu.Reset();
            Assert.AreEqual(0x8F, _cpu.A);
        }
        [Test]
        public void CanOrAbsoluteY()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x0F)
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x10)
                .Write(OPCODE.ORA_ABSOLUTE_Y)
                .WriteWord(0x1000)
                .Write(OPCODE.BRK)
                .Write(0x1010, 0x80);
            _cpu.Reset();
            Assert.AreEqual(0x8F, _cpu.A);
        }

        [Test]
        public void CanAslAccumulator()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x88)
                .Write(OPCODE.ASL_ACCUMULATOR)
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual(0x10, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanAslZeroPage()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.ASL_ZERO_PAGE)
                .Write(0x20)
                .Write(OPCODE.BRK)
                .Write(0x20, 0x88);
            _cpu.Reset();
            Assert.AreEqual(0x10, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanAslZeroPageX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x20)
                .Write(OPCODE.ASL_ZERO_PAGE_X)
                .Write(0x00)
                .Write(OPCODE.BRK)
                .Write(0x20, 0x88);
            _cpu.Reset();
            Assert.AreEqual(0x10, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanAslAbsolute()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.ASL_ABSOLUTE)
                .Ref("Data")
                .Write(OPCODE.BRK)
                .Write(0x1020, 0x88, "Data")
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual(0x10, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanAslAbsoluteX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x02)
                .Write(OPCODE.ASL_ABSOLUTE_X)
                .Ref("Data")
                .Write(OPCODE.BRK)
                .WriteWord(0x1020, 0x00, "Data")
                .Write(0x88)
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual(0x10, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanLsrAccumulator()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x11)
                .Write(OPCODE.LSR_ACCUMULATOR)
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual(0x08, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanLsrZeroPage()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LSR_ZERO_PAGE)
                .Write(0x20)
                .Write(OPCODE.BRK)
                .Write(0x20, 0x11);
            _cpu.Reset();
            Assert.AreEqual(0x08, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanLsrZeroPageX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x20)
                .Write(OPCODE.LSR_ZERO_PAGE_X)
                .Write(0x00)
                .Write(OPCODE.BRK)
                .Write(0x20, 0x11);
            _cpu.Reset();
            Assert.AreEqual(0x08, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanLsrAbsolute()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LSR_ABSOLUTE)
                .Ref("Data")
                .Write(OPCODE.BRK)
                .Write(0x1020, 0x11, "Data")
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual(0x08, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanLsrAbsoluteX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x02)
                .Write(OPCODE.LSR_ABSOLUTE_X)
                .Ref("Data")
                .Write(OPCODE.BRK)
                .WriteWord(0x1020, 0x00, "Data")
                .Write(0x11)
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual(0x08, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRolAccumulator()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x88)
                .Write(OPCODE.SEC)
                .Write(OPCODE.ROL_ACCUMULATOR)
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual(0x11, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRolZeroPage()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.SEC)
                .Write(OPCODE.ROL_ZERO_PAGE)
                .Write(0x20)
                .Write(OPCODE.BRK)
                .Write(0x20, 0x88);
            _cpu.Reset();
            Assert.AreEqual(0x11, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRolZeroPageX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x20)
                .Write(OPCODE.SEC)
                .Write(OPCODE.ROL_ZERO_PAGE_X)
                .Write(0x00)
                .Write(OPCODE.BRK)
                .Write(0x20, 0x88);
            _cpu.Reset();
            Assert.AreEqual(0x11, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRolAbsolute()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.SEC)
                .Write(OPCODE.ROL_ABSOLUTE)
                .Ref("Data")
                .Write(OPCODE.BRK)
                .Write(0x1020, 0x88, "Data")
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual(0x11, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRolAbsoluteX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x02)
                .Write(OPCODE.SEC)
                .Write(OPCODE.ROL_ABSOLUTE_X)
                .Ref("Data")
                .Write(OPCODE.BRK)
                .WriteWord(0x1020, 0x00, "Data")
                .Write(0x88)
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual(0x11, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRorAccumulatorNoCarry()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x11)
                .Write(OPCODE.ROR_ACCUMULATOR)
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual(0x08, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRorZeroPageNoCarry()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.ROR_ZERO_PAGE)
                .Write(0x20)
                .Write(OPCODE.BRK)
                .Write(0x20, 0x11);
            _cpu.Reset();
            Assert.AreEqual(0x08, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRorZeroPageXNoCarry()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x20)
                .Write(OPCODE.ROR_ZERO_PAGE_X)
                .Write(0x00)
                .Write(OPCODE.BRK)
                .Write(0x20, 0x11);
            _cpu.Reset();
            Assert.AreEqual(0x08, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRorAbsoluteNoCarry()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.ROR_ABSOLUTE)
                .Ref("Data")
                .Write(OPCODE.BRK)
                .Write(0x1020, 0x11, "Data")
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual(0x08, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRorAbsoluteXNoCarry()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x02)
                .Write(OPCODE.ROR_ABSOLUTE_X)
                .Ref("Data")
                .Write(OPCODE.BRK)
                .WriteWord(0x1020, 0x00, "Data")
                .Write(0x11)
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual(0x08, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRorAccumulatorCarry()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x11)
                .Write(OPCODE.SEC)
                .Write(OPCODE.ROR_ACCUMULATOR)
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual(0x88, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRorZeroPageCarry()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.SEC)
                .Write(OPCODE.ROR_ZERO_PAGE)
                .Write(0x20)
                .Write(OPCODE.BRK)
                .Write(0x20, 0x11);
            _cpu.Reset();
            Assert.AreEqual(0x88, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRorZeroPageXCarry()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x20)
                .Write(OPCODE.SEC)
                .Write(OPCODE.ROR_ZERO_PAGE_X)
                .Write(0x00)
                .Write(OPCODE.BRK)
                .Write(0x20, 0x11);
            _cpu.Reset();
            Assert.AreEqual(0x88, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRorAbsoluteCarry()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.SEC)
                .Write(OPCODE.ROR_ABSOLUTE)
                .Ref("Data")
                .Write(OPCODE.BRK)
                .Write(0x1020, 0x11, "Data")
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual(0x88, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRorAbsoluteXCarry()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x02)
                .Write(OPCODE.SEC)
                .Write(OPCODE.ROR_ABSOLUTE_X)
                .Ref("Data")
                .Write(OPCODE.BRK)
                .WriteWord(0x1020, 0x00, "Data")
                .Write(0x11)
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual(0x88, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRolAccumulatorNoCarry()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x88)
                .Write(OPCODE.ROL_ACCUMULATOR)
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual(0x10, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRolZeroPageNoCarry()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.ROL_ZERO_PAGE)
                .Write(0x20)
                .Write(OPCODE.BRK)
                .Write(0x20, 0x88);
            _cpu.Reset();
            Assert.AreEqual(0x10, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRolZeroPageXNoCarry()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x20)
                .Write(OPCODE.ROL_ZERO_PAGE_X)
                .Write(0x00)
                .Write(OPCODE.BRK)
                .Write(0x20, 0x88);
            _cpu.Reset();
            Assert.AreEqual(0x10, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRolAbsoluteNoCarry()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.ROL_ABSOLUTE)
                .Ref("Data")
                .Write(OPCODE.BRK)
                .Write(0x1020, 0x88, "Data")
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual(0x10, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRolAbsoluteXNoCarry()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x02)
                .Write(OPCODE.ROL_ABSOLUTE_X)
                .Ref("Data")
                .Write(OPCODE.BRK)
                .WriteWord(0x1020, 0x00, "Data")
                .Write(0x88)
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual(0x10, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanDecrementZeroPage()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.DEC_ZERO_PAGE)
                .Write(0x10)
                .Write(OPCODE.LDA_ZERO_PAGE)
                .Write(0x10)
                .Write(OPCODE.BRK)
                .Write(0x10, 0x80);
            _cpu.Reset();
            Assert.AreEqual(0x7F, _cpu.A);
        }

        [Test]
        public void CanDecrementZeroPageFromZero()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.DEC_ZERO_PAGE)
                .Write(0x10)
                .Write(OPCODE.LDA_ZERO_PAGE)
                .Write(0x10)
                .Write(OPCODE.BRK)
                .Write(0x10, 0x00);
            _cpu.Reset();
            Assert.AreEqual(0xFF, _cpu.A);
            Assert.IsTrue(_cpu.P.N);
        }

        [Test]
        public void CanDecrementZeroPageX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x10)
                .Write(OPCODE.DEC_ZERO_PAGE_X)
                .Write(0x00)
                .Write(OPCODE.LDA_ZERO_PAGE)
                .Write(0x10)
                .Write(OPCODE.BRK)
                .Write(0x10, 0x80);
            _cpu.Reset();
            Assert.AreEqual(0x7F, _cpu.A);
        }

        [Test]
        public void CanDecrementAbsolute()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.DEC_ABSOLUTE)
                .Ref("Data")
                .Write(OPCODE.LDA_ABSOLUTE)
                .Ref("Data")
                .Write(OPCODE.BRK)
                .Write(0x1010, 0x80, "Data")
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual(0x7F, _cpu.A);
        }

        [Test]
        public void CanDecrementAbsoluteX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x02)
                .Write(OPCODE.DEC_ABSOLUTE_X)
                .Ref("Data")
                .Write(OPCODE.LDA_ABSOLUTE_X)
                .Ref("Data")
                .Write(OPCODE.BRK)
                .WriteWord(0x1010, 0x5555, "Data")
                .Write(0x80)
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual(0x7F, _cpu.A);
        }

        [Test]
        public void CanDecrementX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x80)
                .Write(OPCODE.DEX)
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual(0x7F, _cpu.X);
            Assert.IsFalse(_cpu.P.N);
        }

        [Test]
        public void CanDecrementY()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x80)
                .Write(OPCODE.DEY)
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual(0x7F, _cpu.Y);
            Assert.IsFalse(_cpu.P.N);
        }

        [Test]
        public void CanExclusiveOrImmediate()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0xFF)
                .Write(OPCODE.EOR_IMMEDIATE)
                .Write(0x55)
                .Write(OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual(0xAA, _cpu.A);
        }

        [Test]
        public void CanExclusiveOrZeroPage()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0xFF)
                .Write(OPCODE.EOR_ZERO_PAGE)
                .Write(0x10)
                .Write(OPCODE.BRK)
                .Write(0x10, 0x55);
            _cpu.Reset();
            Assert.AreEqual(0xAA, _cpu.A);
        }

        [Test]
        public void CanExclusiveOrZeroPageX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0xFF)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x02)
                .Write(OPCODE.EOR_ZERO_PAGE_X)
                .Write(0x10)
                .Write(OPCODE.BRK)
                .Write(0x12, 0x55);
            _cpu.Reset();
            Assert.AreEqual(0xAA, _cpu.A);
        }

        [Test]
        public void CanExclusiveOrAbsolute()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0xFF)
                .Write(OPCODE.EOR_ABSOLUTE)
                .Ref("Data")
                .Write(OPCODE.BRK)
                .Write(0x1010, 0x55, "Data")
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual(0xAA, _cpu.A);
        }

        [Test]
        public void CanExclusiveOrAbsoluteX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0xFF)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x02)
                .Write(OPCODE.EOR_ABSOLUTE_X)
                .Ref("Data")
                .Write(OPCODE.BRK)
                .WriteWord(0x1010, 0xFFFF, "Data")
                .Write(0x55)
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual(0xAA, _cpu.A);
        }

        [Test]
        public void CanExclusiveOrIndirectX()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0xFF)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x02)
                .Write(OPCODE.EOR_INDIRECT_X)
                .Write(0x10)
                .Write(OPCODE.BRK)
                .Write(0x9000, 0x55, "Data")
                .Ref(0x12, "Data")
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual(0xAA, _cpu.A);
        }

        [Test]
        public void CanExclusiveOrIndirectY()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0xFF)
                .Write(OPCODE.LDY_IMMEDIATE)
                .Write(0x02)
                .Write(OPCODE.EOR_INDIRECT_Y)
                .Write(0x10)
                .Write(OPCODE.BRK)
                .WriteWord(0x9000, 0xFFFF, "Data")
                .Write(0x55)
                .Ref(0x10, "Data")
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual(0xAA, _cpu.A);
        }

        [Test]
        public void CanCauseStackOverflow()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.JSR, "InfiniteLoop")
                .Ref("InfiniteLoop")
                .Fixup();
            _cpu.Reset();
            Assert.IsTrue(_cpu.HaltReason == HaltReason.StackOverflow);
        }

        [Test]
        public void CanCauseStackUnderflow()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.RTS);
            _cpu.Reset();
            Assert.IsTrue(_cpu.HaltReason == HaltReason.StackUnderflow);
        }

        [Test]
        public void CanReturnFromInterrupt()
        {
            // Manually push a return address and processor flags to the stack
            // to simulate an interrupt
            mem.Load(PROG_START)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x90)
                .Write(OPCODE.PHA)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.PHA)
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0x55)
                .Write(OPCODE.PHA)
                .Write(OPCODE.RTI)
                .Write(0x9000, OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual(0x9000, _cpu.PC);
            Assert.AreEqual(0x51, _cpu.P.AsByte());
        }

        [Test]
        public void CanServiceInterrupt()
        {
            _cpu.OnTick += TriggerInterrupt;
            _tickCount = 0;
            _interruptTickInterval = 600;

            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE)
                .Write(0x00)
                .Write(OPCODE.LDY_IMMEDIATE, "LoopX")
                .Write(0x20)
                .Write(OPCODE.TXA, "LoopY")
                .Write(OPCODE.STA_INDIRECT_Y)
                .Write(0x10)    // Holds vector for data block to write
                .Write(OPCODE.DEY)
                .Write(OPCODE.BNE)
                .Write(-5)
                .Write(OPCODE.INX)
                .Write(OPCODE.BNE)
                .Write(-11)
                .Write(OPCODE.BRK)
                .Write(0xFF00, OPCODE.INC_ABSOLUTE)
                .WriteWord(0x7800)
                .Write(OPCODE.RTI)
                .Write(0x7000, 0x00, "Block")
                .Write(0x7800, 0x00) // ISR will increment this
                .Ref(0x10, "Block")
                .WriteWord(_cpu.IRQ_VECTOR, 0xFF00)
                .Fixup();
            _cpu.Reset();
            _cpu.OnTick -= TriggerInterrupt;
            var result = mem.Read(0x7800);
            Assert.AreEqual(42, result);
        }

        [Test]
        public void LetsDoThis()
        {
            mem.Load(PROG_START)
                .Write(OPCODE.LDX_IMMEDIATE) // X will be our index into the data and the screen
                .Write(0x00)
                .Write(OPCODE.LDA_ABSOLUTE_X, "Loop")
                .Ref("Data")
                .Write(OPCODE.BEQ)
                .RelativeRef("Finished")
                .Write(OPCODE.STA_ABSOLUTE_X)
                .WriteWord(DISPLAY_BASE_ADDR)
                .Write(OPCODE.INX)
                .Write(OPCODE.BNE)
                .RelativeRef("Loop")
                .Write(OPCODE.BRK, "Finished")
                .WriteString("Hello, World!", "Data")
                .Write(0x00)
                .Fixup();
            _cpu.Reset();
            Assert.AreEqual('H', mem.Read(DISPLAY_BASE_ADDR));
        }

        [TestCase(1,127,128)]
        [TestCase(1,255,256)]
        public void CanAdd16Bit(int v1, int v2, int expected)
        {
            mem.Load(PROG_START)
                .Write(OPCODE.CLC)
                .Write(OPCODE.LDA_ABSOLUTE)
                .Ref("v1")
                .Write(OPCODE.ADC_ABSOLUTE)
                .Ref("v2")
                .Write(OPCODE.STA_ABSOLUTE)
                .Ref("Result")
                .Write(OPCODE.LDA_ABSOLUTE)
                .Ref("v1",1)
                .Write(OPCODE.ADC_ABSOLUTE)
                .Ref("v2",1)
                .Write(OPCODE.STA_ABSOLUTE)
                .Ref("Result",1)
                .Write(OPCODE.BRK)
                .WriteWord((ushort)v1, "v1")
                .WriteWord((ushort)v2, "v2")
                .WriteWord(0x8200, 0x0000, "Result")
                .Fixup();
            _cpu.Reset();
            var result = mem.ReadWord(0x8200);
            Assert.AreEqual(expected,result);
        }

        [Test]
        public void CanFillScreen()
        {
            var w = _display.Mode.Width;
            var h = _display.Mode.Height;

            mem.Load(PROG_START)
                // Write column header
                .Write(OPCODE.LDX_IMMEDIATE)
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
                .Write(w)
                .Write(OPCODE.ADC_ZERO_PAGE)
                .ZeroPageRef("DisplayVector")
                .Write(OPCODE.STA_ZERO_PAGE)
                .ZeroPageRef("DisplayVector")
                .Write(OPCODE.LDA_IMMEDIATE)
                .Write(0)
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

                .Write(0x10, '0', "CurrentDigit")
                .WriteWord(0x12, DISPLAY_BASE_ADDR, "DisplayVector")
                .Fixup();

            _cpu.Reset();
            Assert.AreEqual('0', mem.Read(DISPLAY_BASE_ADDR));
            var expected = (h + 9) % 10 + '0';
            Assert.AreEqual(expected, mem.Read((ushort)(DISPLAY_BASE_ADDR + (h-1) * w)));
        }

        private void TriggerInterrupt(object sender, EventArgs e)
        {
            _tickCount++;
            if(_tickCount >= _interruptTickInterval)
            {
                _tickCount = 0;
                _cpu.Interrupt();
            }
        }
    }
}