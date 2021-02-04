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
        private CPU6502 _cpu;
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
            await mem.Initialise();
            _display.Clear();
            _cpu = new CPU6502(mem);
            _cpu.DebugLevel = DebugLevel.Verbose;
            mem.WriteWord(_cpu.RESET_VECTOR, PROG_START);

            mem.Labels = new LabelTable();
            mem.Labels.Add("DISPLAY_CONTROL_ADDR", MemoryMappedDisplay.DISPLAY_CONTROL_BLOCK_ADDR);
            mem.Labels.Add("DISPLAY_BASE_ADDR", DISPLAY_BASE_ADDR);
            mem.Labels.Add("DISPLAY_SIZE", DISPLAY_SIZE);
            mem.Labels.Add("RESET_VECTOR", _cpu.RESET_VECTOR);
            mem.Labels.Add("IRQ_VECTOR", _cpu.IRQ_VECTOR);
            mem.Labels.Add("NMI_VECTOR", _cpu.NMI_VECTOR);
        }

        /*

           I apologise in advance for using _ as a variable name.
           
           The loader instance just gets in the way, so assigning it
           the variable name _ makes it much less intrusive.
           But it might bother some people!
        */

        [Test]
        public void CanReset()
        {
            _cpu.Reset();
            Assert.Pass();
        }

        [Test]
        public void EmulatorReportsInvalidOpcodes()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                .Write(0xFF)
                .LDA_IMMEDIATE(0x02)
                .BRK();
            }
            _cpu.Reset();
            Assert.AreEqual(1, _cpu.EmulationErrorsCount);
            Assert.AreEqual(0x02, _cpu.A);
        }

        [Test]
        public void CanLoadAccumulatorImmediate()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                .LDA_IMMEDIATE(0x34);
            }
            _cpu.Reset();
            Assert.AreEqual(0x34, _cpu.A);
        }

        [Test]
        public void CanLoadAccumulatorZeroPage()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                .LDA_ZERO_PAGE(0x34)
                .BRK()
                .Write(0x34, 0x56);
            }
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.A);
        }

        [Test]
        public void CanLoadAccumulatorZeroPageX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                .LDX_IMMEDIATE(0x1)
                .LDA_ZERO_PAGE_X(0x34)
                .BRK()
                .Write(0x35, 0x56);
            }
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.A);
        }

        [Test]
        public void CanLoadAccumulatorAbsolute()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                .LDA_ABSOLUTE(0x2021)
                .BRK()
                .Write(0x2021, 0x56);
            }
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.A);
        }

        [Test]
        public void CanLoadAccumulatorAbsoluteX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                .LDX_IMMEDIATE(0x1)
                .LDA_ABSOLUTE_X(0x2021)
                .BRK()
                .Write(0x2022, 0x56);
            }
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.A);
        }

        [Test]
        public void CanLoadAccumulatorIndirectX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                .LDX_IMMEDIATE(0x2)
                .LDA_INDIRECT_X(0x80)
                .BRK()
                .WriteWord(0x82, 0x2021)
                .Write(0x2021, 0x56);
            }
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.A);
        }

        [Test]
        public void CanLoadAccumulatorIndirectY()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                .LDY_IMMEDIATE(0x2)
                .LDA_INDIRECT_Y(0x80)
                .BRK()
                .WriteWord(0x80, 0x2021)
                .Write(0x2023, 0x56);
            }
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.A);
        }

        [Test]
        public void CanLoadXImmediate()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                .LDX_IMMEDIATE(0x34);
            }
            _cpu.Reset();
            Assert.AreEqual(0x34, _cpu.X);
        }

        [Test]
        public void CanLoadXZeroPage()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                .LDX_ZERO_PAGE(0x34)
                .BRK()
                .Write(0x34, 0x56);
            }
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.X);
        }

        [Test]
        public void CanLoadXZeroPageY()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                .LDY_IMMEDIATE(0x1)
                .LDX_ZERO_PAGE_Y(0x34)
                .BRK()
                .Write(0x35, 0x56);
            }
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.X);
        }

        [Test]
        public void CanLoadXAbsolute()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                .LDX_ABSOLUTE(0x2021)
                .BRK()
                .Write(0x2021, 0x56);
            }
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.X);
        }

        [Test]
        public void CanLoadXAbsoluteY()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                .LDY_IMMEDIATE(0x1)
                .LDX_ABSOLUTE_Y(0x2021)
                .BRK()
                .Write(0x2022, 0x56);
            }
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.X);
        }

        [Test]
        public void CanLoadYImmediate()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                .LDY_IMMEDIATE(0x34);
            }
            _cpu.Reset();
            Assert.AreEqual(0x34, _cpu.Y);
        }

        [Test]
        public void CanLoadYZeroPage()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                .LDY_ZERO_PAGE(0x34)
                .BRK()
                .Write(0x34, 0x56);
            }
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.Y);
        }

        [Test]
        public void CanLoadYZeroPageX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                .LDX_IMMEDIATE(0x1)
                .LDY_ZERO_PAGE_X(0x34)
                .BRK()
                .Write(0x35, 0x56);
            }
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.Y);
        }

        [Test]
        public void CanLoadYAbsolute()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                .LDY_ABSOLUTE(0x2021)
                .BRK()
                .Write(0x2021, 0x56);
            }
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.Y);
        }

        [Test]
        public void CanLoadYAbsoluteX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                .LDX_IMMEDIATE(0x1)
                .LDY_ABSOLUTE_X(0x2021)
                .BRK()
                .Write(0x2022, 0x56);
            }
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.Y);
        }

        [Test]
        public void CanNoOperation()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                .NOP()
                .NOP()
                .BRK();
            }
            _cpu.Reset();
            Assert.AreEqual(PROG_START + 3, _cpu.PC);
        }

        [Test]
        public void CanBreak()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                .NOP()
                .BRK()
                .NOP();
            }
            _cpu.Reset();
            Assert.AreEqual(PROG_START + 2, _cpu.PC);
        }

        [Test]
        public void CanStoreAccumulatorZeroPage()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                .LDA_IMMEDIATE((byte)'H')
                .STA_ZERO_PAGE(0x10)
                .LDY_ZERO_PAGE(0x10);
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.Y);
        }

        [Test]
        public void CanStoreAccumulatorZeroPageX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                .LDA_IMMEDIATE((byte)'H')
                .LDX_IMMEDIATE(0x02)
                .STA_ZERO_PAGE_X(0x10)
                .LDY_ZERO_PAGE(0x12);
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.Y);
        }

        [Test]
        public void CanStoreAccumulatorAbsolute()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                .LDA_IMMEDIATE((byte)'H')
                .STA_ABSOLUTE("DISPLAY_BASE_ADDR")
                .LDY_ABSOLUTE("DISPLAY_BASE_ADDR")
                .BRK();
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.Y);
        }

        [Test]
        public void CanStoreAccumulatorAbsoluteX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(1)
                              .Write(OPCODE.STA_ABSOLUTE_X)
                              .WriteWord(DISPLAY_BASE_ADDR)
                              .Write(OPCODE.LDY_ABSOLUTE)
                              .WriteWord(DISPLAY_BASE_ADDR + 1);
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.Y);
        }
        [Test]
        public void CanStoreAccumulatorAbsoluteY()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write(1)
                              .Write(OPCODE.STA_ABSOLUTE_Y)
                              .WriteWord(DISPLAY_BASE_ADDR)
                              .Write(OPCODE.LDX_ABSOLUTE)
                              .WriteWord(DISPLAY_BASE_ADDR + 1);
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.X);
        }
        [Test]
        public void CanStoreAccumulatorIndirectX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(2)
                              .Write(OPCODE.STA_INDIRECT_X)
                              .Write(0x00)
                              .Write(OPCODE.LDY_ABSOLUTE)
                              .WriteWord(DISPLAY_BASE_ADDR)
                              .WriteWord(0x2, DISPLAY_BASE_ADDR);
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.Y);
        }
        [Test]
        public void CanStoreAccumulatorIndirectY()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write(2)
                              .Write(OPCODE.STA_INDIRECT_Y)
                              .Write(0x00)
                              .Write(OPCODE.LDX_ABSOLUTE)
                              .WriteWord(DISPLAY_BASE_ADDR + 2)
                              .WriteWord(0x00, DISPLAY_BASE_ADDR);
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.X);
        }
        [Test]
        public void CanStoreXZeroPage()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.STX_ZERO_PAGE)
                              .Write(0x20)
                              .Write(OPCODE.LDA_ZERO_PAGE)
                              .Write(0x20);
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.A);
        }
        [Test]
        public void CanStoreXZeroPageY()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write(0x10)
                              .Write(OPCODE.STX_ZERO_PAGE_Y)
                              .Write(0x10)
                              .Write(OPCODE.LDA_ZERO_PAGE)
                              .Write(0x20);
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.A);
        }

        [Test]
        public void CanStoreXAbsolute()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.STX_ABSOLUTE)
                              .WriteWord(DISPLAY_BASE_ADDR)
                              .Write(OPCODE.LDA_ABSOLUTE)
                              .WriteWord(DISPLAY_BASE_ADDR);
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.A);
        }
        [Test]
        public void CanStoreYZeroPage()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.STY_ZERO_PAGE)
                              .Write(0x20)
                              .Write(OPCODE.LDA_ZERO_PAGE)
                              .Write(0x20);
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.A);
        }
        [Test]
        public void CanStoreYZeroPageX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x10)
                              .Write(OPCODE.STY_ZERO_PAGE_X)
                              .Write(0x10)
                              .Write(OPCODE.LDA_ZERO_PAGE)
                              .Write(0x20);
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.A);
        }

        [Test]
        public void CanStoreYAbsolute()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.STY_ABSOLUTE)
                              .WriteWord(DISPLAY_BASE_ADDR)
                              .Write(OPCODE.LDA_ABSOLUTE)
                              .WriteWord(DISPLAY_BASE_ADDR);
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.A);
        }

        [Test]
        public void CanTransferAccumulatorToX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.TAX);
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.X);
        }
        [Test]
        public void CanTransferAccumulatorToY()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.TAY);
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.Y);
        }
        [Test]
        public void CanTransferXToAccumulator()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.TXA);
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.A);
        }
        [Test]
        public void CanTransferYToAccumulator()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.TYA);
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.A);
        }

        [Test]
        public void CanTransferStackPointerToX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x7F)
                              .Write(OPCODE.PHA)
                              .Write(OPCODE.PHA)
                              .Write(OPCODE.TSX)
                              .Write(OPCODE.BRK);
            }
            _cpu.Reset();
            Assert.AreEqual(0xFD, _cpu.X);
        }

        [Test]
        public void CanTransferXToStackPointer()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x7F)
                              .Write(OPCODE.PHA)
                              .Write(OPCODE.PHA)
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x80)
                              .Write(OPCODE.TXS)
                              .Write(OPCODE.BRK);
            }
            _cpu.Reset();
            Assert.AreEqual(0x80, _cpu.SP);
        }

        [Test]
        public void CanPushAccumulator()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.PHA)
                              .Write(OPCODE.LDY_ABSOLUTE)
                              .WriteWord(0x1FF);
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.Y);
        }

        [Test]
        public void CanPushProcessorStatus()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x00)
                              .Write(OPCODE.PHP)
                              .Write(OPCODE.LDY_ABSOLUTE)
                              .WriteWord(0x1FF);
            }
            _cpu.Reset();
            Assert.AreEqual(0x02, _cpu.Y);
        }
        [Test]
        public void CanPullAccumulator()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.PHA)
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x00)
                              .Write(OPCODE.PLA);
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.A);
        }

        [Test]
        public void CanPullProcessorStatus()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0xFF)
                              .Write(OPCODE.PHA)
                              .Write(OPCODE.PLP)
                              .Write(OPCODE.BRK);
            }
            _cpu.Reset();
            Assert.AreEqual(0xFF, _cpu.P.AsByte());
        }

        [Test]
        public void CanJumpAbsolute()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.JMP_ABSOLUTE)
                              .Ref("Continue")
                              .Write(OPCODE.BRK)
                              .Write(OPCODE.LDA_IMMEDIATE, "Continue")
                              .Write('e')
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }
        [Test]
        public void CanJumpIndirect()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.JMP_INDIRECT)
                              .Ref("Data")
                              .Write(OPCODE.BRK)
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write('e')
                              .WriteWord(0x9000, PROG_START + 6, "Data")
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }

        [Test]
        public void CanJumpToSubroutine()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.JSR)
                              .Ref("Sub")
                              .Write(OPCODE.BRK)
                              .Write(OPCODE.LDA_IMMEDIATE, "Sub")
                              .Write('e')
                              .Write(OPCODE.BRK)
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
            Assert.AreEqual(0xFD, _cpu.SP);
        }

        [Test]
        public void CanReturnSubroutine()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
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
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'l', _cpu.A);
            Assert.AreEqual(0xFF, _cpu.SP);
        }

        [Test]
        public void CanSetCarry()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.SEC);
            }
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanClearCarry()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.SEC)
                              .Write(OPCODE.CLC);
            }
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C);
        }

        [Test]
        public void CanClearOverflow()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x50)
                              .Write(OPCODE.ADC_IMMEDIATE)
                              .Write(0x50)
                              .Write(OPCODE.CLV);
            }
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.V);
        }

        [Test]
        public void CanSetInterruptDisable()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.SEI);
            }
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.I);
        }

        [Test]
        public void CanClearInterruptDisable()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.SEI)
                              .Write(OPCODE.CLI);
            }
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.I);
        }

        [Test]
        public void CanSetDecimal()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.SED);
            }
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.D);
        }

        [Test]
        public void CanClearDecimal()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.SED)
                              .Write(OPCODE.CLD);
            }
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.D);
        }

        [Test]
        public void CanBranchForwardsOnCarryClear()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.CLC)
                              .Write(OPCODE.BCC)
                              .RelativeRef("Cont")
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.BRK)
                              .Write(OPCODE.LDA_IMMEDIATE, "Cont")
                              .Write('e')
                              .Write(OPCODE.BRK)
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }

        [Test]
        public void CanBranchBackwardsOnCarryClear()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
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
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }

        [Test]
        public void CanBranchForwardsOnCarrySet()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.SEC)
                              .Write(OPCODE.BCS)
                              .RelativeRef("Cont")
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.BRK)
                              .Write(OPCODE.LDA_IMMEDIATE, "Cont")
                              .Write('e')
                              .Write(OPCODE.BRK)
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }

        [Test]
        public void CanBranchBackwardsOnCarrySet()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
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
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }

        [Test]
        public void CanBranchOnOverflowClear()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
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
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }

        [Test]
        public void CanBranchOnOverflowSet()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
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
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }


        [Test]
        public void CanBranchForwardsOnMinus()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
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
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }

        [Test]
        public void CanBranchBackwardsOnMinus()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
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
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }

        [Test]
        public void CanBranchForwardsOnPositive()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
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
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }

        [Test]
        public void CanBranchBackwardsOnPositive()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
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
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }

        [Test]
        public void CanCompareImmediateEqual()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write(0x00)
                              .Write(OPCODE.CMP_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.BRK);
            }
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }
        [Test]
        public void CanCompareImmediateLess()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x90)
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write(0x00)
                              .Write(OPCODE.CMP_IMMEDIATE)
                              .Write(0xFF)
                              .Write(OPCODE.BRK);
            }
            _cpu.Reset();
            Assert.IsTrue(!_cpu.P.C && !_cpu.P.Z && _cpu.P.N);
        }
        [Test]
        public void CanCompareImmediateGreater()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x90)
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write(0x00)
                              .Write(OPCODE.CMP_IMMEDIATE)
                              .Write(0x00)
                              .Write(OPCODE.BRK);
            }
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && !_cpu.P.Z && _cpu.P.N);
        }
        [Test]
        public void CanCompareZeroPage()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write(0x00)
                              .Write(OPCODE.CMP_ZERO_PAGE)
                              .Write(0x10)
                              .Write(OPCODE.BRK)
                              .Write(0x10, 'H');
            }
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }
        [Test]
        public void CanCompareZeroPageX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x02)
                              .Write(OPCODE.CMP_ZERO_PAGE_X)
                              .Write(0x10)
                              .Write(OPCODE.BRK)
                              .Write(0x12, 'H');
            }
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }

        [Test]
        public void CanCompareAbsolute()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write(0x00)
                              .Write(OPCODE.CMP_ABSOLUTE)
                              .Ref("Data")
                              .Write(OPCODE.BRK)
                              .Write(0x1010, 'H', "Data")
                              ;
            }
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }

        [Test]
        public void CanCompareAbsoluteX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x10)
                              .Write(OPCODE.CMP_ABSOLUTE_X)
                              .WriteWord(0x1000)
                              .Write(OPCODE.BRK)
                              .Write(0x1010, 'H');
            }
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }
        [Test]
        public void CanCompareIndirectX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x02)
                              .Write(OPCODE.CMP_INDIRECT_X)
                              .Write(0x0E)
                              .Write(OPCODE.BRK)
                              .Write(0x1010, 'H')
                              .WriteWord(0x0010, 0x1010);
            }
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }
        [Test]
        public void CanCompareIndirectY()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write(0x02)
                              .Write(OPCODE.CMP_INDIRECT_Y)
                              .Write(0x10)
                              .Write(OPCODE.BRK)
                              .Write(0x1012, 'H')
                              .WriteWord(0x0010, 0x1010);
            }
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }

        [Test]
        public void CanCompareXImmediateEqual()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write(0x00)
                              .Write(OPCODE.CPX_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.BRK);
            }
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }
        [Test]
        public void CanCompareXImmediateLess()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x90)
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write(0x00)
                              .Write(OPCODE.CPX_IMMEDIATE)
                              .Write(0xFF)
                              .Write(OPCODE.BRK);
            }
            _cpu.Reset();
            Assert.IsTrue(!_cpu.P.C && !_cpu.P.Z && _cpu.P.N);
        }
        [Test]
        public void CanCompareXImmediateGreater()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x90)
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write(0x00)
                              .Write(OPCODE.CPX_IMMEDIATE)
                              .Write(0x00)
                              .Write(OPCODE.BRK);
            }
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && !_cpu.P.Z && _cpu.P.N);
        }
        [Test]
        public void CanCompareXZeroPage()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write(0x00)
                              .Write(OPCODE.CPX_ZERO_PAGE)
                              .Write(0x10)
                              .Write(OPCODE.BRK)
                              .Write(0x10, 'H');
            }
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }

        [Test]
        public void CanCompareXAbsolute()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write(0x00)
                              .Write(OPCODE.CPX_ABSOLUTE)
                              .Ref("Data")
                              .Write(OPCODE.BRK)
                              .Write(0x1010, 'H', "Data")
                              ;
            }
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }

        [Test]
        public void CanCompareYImmediateEqual()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x00)
                              .Write(OPCODE.CPY_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.BRK);
            }
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }
        [Test]
        public void CanCompareYImmediateLess()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write(0x90)
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x00)
                              .Write(OPCODE.CPY_IMMEDIATE)
                              .Write(0xFF)
                              .Write(OPCODE.BRK);
            }
            _cpu.Reset();
            Assert.IsTrue(!_cpu.P.C && !_cpu.P.Z && _cpu.P.N);
        }
        [Test]
        public void CanCompareYImmediateGreater()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write(0x90)
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x00)
                              .Write(OPCODE.CPY_IMMEDIATE)
                              .Write(0x00)
                              .Write(OPCODE.BRK);
            }
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && !_cpu.P.Z && _cpu.P.N);
        }
        [Test]
        public void CanCompareYZeroPage()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x00)
                              .Write(OPCODE.CPY_ZERO_PAGE)
                              .Write(0x10)
                              .Write(OPCODE.BRK)
                              .Write(0x10, 'H');
            }
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }

        [Test]
        public void CanCompareYAbsolute()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write('H')
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x00)
                              .Write(OPCODE.CPY_ABSOLUTE)
                              .Ref("Data")
                              .Write(OPCODE.BRK)
                              .Write(0x1010, 'H', "Data")
                              ;
            }
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }

        [Test]
        public void CanBranchEquals()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
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
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'Y', _cpu.A);
        }
        [Test]
        public void CanBranchNotEquals()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
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
            }
            _cpu.Reset();
            Assert.AreEqual((byte)'Y', _cpu.A);
        }

        [Test]
        public void CanBitTestAbsoluteAllMasked()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x0A)
                              .Write(OPCODE.BIT_ABSOLUTE)
                              .WriteWord(0x1000)
                              .Write(OPCODE.BRK)
                              .Write(0x1000, 0xF0);
            }
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.Z);
        }

        [Test]
        public void CanBitTestAbsoluteNotAllMasked()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x1A)
                              .Write(OPCODE.BIT_ABSOLUTE)
                              .WriteWord(0x1000)
                              .Write(OPCODE.BRK)
                              .Write(0x1000, 0xF0);
            }
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.Z);
        }

        [Test]
        public void CanAddWithCarryImmediate()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x01)
                              .Write(OPCODE.ADC_IMMEDIATE)
                              .Write(0x02)
                              .Write(OPCODE.BRK);
            }
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
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(v1)
                              .Write(OPCODE.ADC_IMMEDIATE)
                              .Write(v2)
                              .Write(OPCODE.BRK);
            }

            _cpu.Reset();

            Assert.AreEqual(expected, _cpu.A);
            Assert.AreEqual(statusMask, _cpu.P.AsByte() & statusMask);
        }

        [TestCase(200, 100, 300)]
        [TestCase(0, 1, 1)]
        [TestCase(65535, 2, 1)]
        public void CanAddMultipleBytes(int v1, int v2, int expectedResult)
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
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
                              .WriteWord(0x9000, (ushort)v1, "V1")
                              .WriteWord((ushort)v2, "V2")
                              .WriteWord(0x0000, "Result")
                              ;
            }

            _cpu.Reset();

            var result = mem.ReadWord(0x9004);

            Assert.AreEqual(expectedResult, result);
        }
        [Test]
        public void CanAddWithCarryZeroPage()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x01)
                              .Write(OPCODE.ADC_ZERO_PAGE)
                              .Write(0x01)
                              .Write(OPCODE.BRK)
                              .Write(0x01, 0x02);
            }

            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x03, _cpu.A);
        }

        [Test]
        public void CanAddWithCarryZeroPageX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x01)
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x02)
                              .Write(OPCODE.ADC_ZERO_PAGE_X)
                              .Write(0x03)
                              .Write(OPCODE.BRK)
                              .Write(0x05, 0x02);
            }
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x03, _cpu.A);
        }

        [Test]
        public void CanAddWithCarryAbsolute()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x01)
                              .Write(OPCODE.ADC_ABSOLUTE)
                              .Ref("Data")
                              .Write(OPCODE.BRK)
                              .Write(0x1005, 0x02, "Data")
                              ;
            }
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x03, _cpu.A);
        }

        [Test]
        public void CanAddWithCarryAbsoluteX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x01)
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x01)
                              .Write(OPCODE.ADC_ABSOLUTE_X)
                              .WriteWord(0x1004)
                              .Write(OPCODE.BRK)
                              .Write(0x1005, 0x02);
            }
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x03, _cpu.A);
        }

        [Test]
        public void CanSubtractWithCarryImmediate()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x03)
                              .Write(OPCODE.SBC_IMMEDIATE)
                              .Write(0x02)
                              .Write(OPCODE.BRK);
            }
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
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(v1)
                              .Write(OPCODE.SBC_IMMEDIATE)
                              .Write(v2)
                              .Write(OPCODE.BRK);

            }
            _cpu.Reset();

            Assert.AreEqual(expected, _cpu.A);
            Assert.AreEqual(statusMask, _cpu.P.AsByte() & statusMask);
        }

        [TestCase(300, 200, 100)]
        [TestCase(1, 1, 0)]
        [TestCase(2, 65535, 3)]
        public void CanSubtractMultipleBytes(int v1, int v2, int expectedResult)
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
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
                              .WriteWord(0x9000, (ushort)v1, "V1")
                              .WriteWord((ushort)v2, "V2")
                              .WriteWord(0x0000, "Result")
                              ;
            }
            _cpu.Reset();

            var result = mem.ReadWord(0x9004);

            Assert.AreEqual(expectedResult, result);
        }
        [Test]
        public void CanSubtractWithCarryZeroPage()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x05)
                              .Write(OPCODE.SBC_ZERO_PAGE)
                              .Write(0x01)
                              .Write(OPCODE.BRK)
                              .Write(0x01, 0x02);
            }
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x03, _cpu.A);
        }

        [Test]
        public void CanSubtractWithCarryZeroPageX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x05)
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x02)
                              .Write(OPCODE.SBC_ZERO_PAGE_X)
                              .Write(0x03)
                              .Write(OPCODE.BRK)
                              .Write(0x05, 0x02);
            }
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x03, _cpu.A);
        }

        [Test]
        public void CanSubtractWithCarryAbsolute()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x05)
                              .Write(OPCODE.SBC_ABSOLUTE)
                              .Ref("Data")
                              .Write(OPCODE.BRK)
                              .Write(0x1005, 0x02, "Data")
                              ;
            }
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x03, _cpu.A);
        }

        [Test]
        public void CanSubtractWithCarryAbsoluteX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x05)
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x01)
                              .Write(OPCODE.SBC_ABSOLUTE_X)
                              .WriteWord(0x1004)
                              .Write(OPCODE.BRK)
                              .Write(0x1005, 0x02);
            }
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x03, _cpu.A);
        }

        [Test]
        public void CanAddWithCarryAbsoluteY()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x01)
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write(0x01)
                              .Write(OPCODE.ADC_ABSOLUTE_Y)
                              .WriteWord(0x1004)
                              .Write(OPCODE.BRK)
                              .Write(0x1005, 0x02);
            }
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x03, _cpu.A);
        }

        [Test]
        public void CanAddWithCarryIndirectX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x01)
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x02)
                              .Write(OPCODE.ADC_INDIRECT_X)
                              .Write(0x00)
                              .Write(OPCODE.BRK)
                              .WriteWord(0x02, 0x1234)
                              .Write(0x1234, 0x02);
            }
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x03, _cpu.A);
        }
        [Test]
        public void CanAddWithCarryIndirectY()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x01)
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write(0x02)
                              .Write(OPCODE.ADC_INDIRECT_Y)
                              .Write(0x08)
                              .Write(OPCODE.BRK)
                              .WriteWord(0x08, 0x1234)
                              .Write(0x1236, 0x02);
            }
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x03, _cpu.A);
        }

        [Test]
        public void CanAndImmediate()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0xFF)
                              .Write(OPCODE.AND_IMMEDIATE)
                              .Write(0x80)
                              .Write(OPCODE.BRK);
            }
            _cpu.Reset();
            Assert.AreEqual(0x80, _cpu.A);
        }

        [Test]
        public void CanAndZeroPage()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0xFF)
                              .Write(OPCODE.AND_ZERO_PAGE)
                              .Write(0x10)
                              .Write(OPCODE.BRK)
                              .Write(0x10, 0x80);
            }
            _cpu.Reset();
            Assert.AreEqual(0x80, _cpu.A);
        }
        [Test]
        public void CanAndZeroPageX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0xFF)
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x10)
                              .Write(OPCODE.AND_ZERO_PAGE_X)
                              .Write(0x00)
                              .Write(OPCODE.BRK)
                              .Write(0x10, 0x80);
            }
            _cpu.Reset();
            Assert.AreEqual(0x80, _cpu.A);
        }
        [Test]
        public void CanAndAbsolute()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0xFF)
                              .Write(OPCODE.AND_ABSOLUTE)
                              .WriteWord(0x1010)
                              .Write(OPCODE.BRK)
                              .Write(0x1010, 0x80);
            }
            _cpu.Reset();
            Assert.AreEqual(0x80, _cpu.A);
        }
        [Test]
        public void CanAndAbsoluteX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0xFF)
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x10)
                              .Write(OPCODE.AND_ABSOLUTE_X)
                              .WriteWord(0x1000)
                              .Write(OPCODE.BRK)
                              .Write(0x1010, 0x80);
            }
            _cpu.Reset();
            Assert.AreEqual(0x80, _cpu.A);
        }
        [Test]
        public void CanAndAbsoluteY()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0xFF)
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write(0x10)
                              .Write(OPCODE.AND_ABSOLUTE_Y)
                              .WriteWord(0x1000)
                              .Write(OPCODE.BRK)
                              .Write(0x1010, 0x80);
            }
            _cpu.Reset();
            Assert.AreEqual(0x80, _cpu.A);
        }

        [Test]
        public void CanOrImmediate()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x0F)
                              .Write(OPCODE.ORA_IMMEDIATE)
                              .Write(0x80)
                              .Write(OPCODE.BRK);
            }
            _cpu.Reset();
            Assert.AreEqual(0x8F, _cpu.A);
        }

        [Test]
        public void CanOrZeroPage()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x0F)
                              .Write(OPCODE.ORA_ZERO_PAGE)
                              .Write(0x10)
                              .Write(OPCODE.BRK)
                              .Write(0x10, 0x80);
            }
            _cpu.Reset();
            Assert.AreEqual(0x8F, _cpu.A);
        }
        [Test]
        public void CanOrZeroPageX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x0F)
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x10)
                              .Write(OPCODE.ORA_ZERO_PAGE_X)
                              .Write(0x00)
                              .Write(OPCODE.BRK)
                              .Write(0x10, 0x80);
            }
            _cpu.Reset();
            Assert.AreEqual(0x8F, _cpu.A);
        }
        [Test]
        public void CanOrAbsolute()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x0F)
                              .Write(OPCODE.ORA_ABSOLUTE)
                              .WriteWord(0x1010)
                              .Write(OPCODE.BRK)
                              .Write(0x1010, 0x80);
            }
            _cpu.Reset();
            Assert.AreEqual(0x8F, _cpu.A);
        }
        [Test]
        public void CanOrAbsoluteX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x0F)
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x10)
                              .Write(OPCODE.ORA_ABSOLUTE_X)
                              .WriteWord(0x1000)
                              .Write(OPCODE.BRK)
                              .Write(0x1010, 0x80);
            }
            _cpu.Reset();
            Assert.AreEqual(0x8F, _cpu.A);
        }
        [Test]
        public void CanOrAbsoluteY()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x0F)
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write(0x10)
                              .Write(OPCODE.ORA_ABSOLUTE_Y)
                              .WriteWord(0x1000)
                              .Write(OPCODE.BRK)
                              .Write(0x1010, 0x80);
            }
            _cpu.Reset();
            Assert.AreEqual(0x8F, _cpu.A);
        }

        [Test]
        public void CanAslAccumulator()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x88)
                              .Write(OPCODE.ASL_ACCUMULATOR)
                              .Write(OPCODE.BRK);
            }
            _cpu.Reset();
            Assert.AreEqual(0x10, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanAslZeroPage()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.ASL_ZERO_PAGE)
                              .Write(0x20)
                              .Write(OPCODE.BRK)
                              .Write(0x20, 0x88);
            }
            _cpu.Reset();
            Assert.AreEqual(0x10, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanAslZeroPageX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x20)
                              .Write(OPCODE.ASL_ZERO_PAGE_X)
                              .Write(0x00)
                              .Write(OPCODE.BRK)
                              .Write(0x20, 0x88);
            }
            _cpu.Reset();
            Assert.AreEqual(0x10, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanAslAbsolute()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.ASL_ABSOLUTE)
                              .Ref("Data")
                              .Write(OPCODE.BRK)
                              .Write(0x1020, 0x88, "Data")
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual(0x10, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanAslAbsoluteX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x02)
                              .Write(OPCODE.ASL_ABSOLUTE_X)
                              .Ref("Data")
                              .Write(OPCODE.BRK)
                              .WriteWord(0x1020, 0x00, "Data")
                              .Write(0x88)
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual(0x10, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanLsrAccumulator()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x11)
                              .Write(OPCODE.LSR_ACCUMULATOR)
                              .Write(OPCODE.BRK);
            }
            _cpu.Reset();
            Assert.AreEqual(0x08, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanLsrZeroPage()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LSR_ZERO_PAGE)
                              .Write(0x20)
                              .Write(OPCODE.BRK)
                              .Write(0x20, 0x11);
            }
            _cpu.Reset();
            Assert.AreEqual(0x08, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanLsrZeroPageX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x20)
                              .Write(OPCODE.LSR_ZERO_PAGE_X)
                              .Write(0x00)
                              .Write(OPCODE.BRK)
                              .Write(0x20, 0x11);
            }
            _cpu.Reset();
            Assert.AreEqual(0x08, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanLsrAbsolute()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LSR_ABSOLUTE)
                              .Ref("Data")
                              .Write(OPCODE.BRK)
                              .Write(0x1020, 0x11, "Data")
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual(0x08, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanLsrAbsoluteX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x02)
                              .Write(OPCODE.LSR_ABSOLUTE_X)
                              .Ref("Data")
                              .Write(OPCODE.BRK)
                              .WriteWord(0x1020, 0x00, "Data")
                              .Write(0x11)
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual(0x08, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRolAccumulator()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x88)
                              .Write(OPCODE.SEC)
                              .Write(OPCODE.ROL_ACCUMULATOR)
                              .Write(OPCODE.BRK);
            }
            _cpu.Reset();
            Assert.AreEqual(0x11, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRolZeroPage()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.SEC)
                              .Write(OPCODE.ROL_ZERO_PAGE)
                              .Write(0x20)
                              .Write(OPCODE.BRK)
                              .Write(0x20, 0x88);
            }
            _cpu.Reset();
            Assert.AreEqual(0x11, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRolZeroPageX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x20)
                              .Write(OPCODE.SEC)
                              .Write(OPCODE.ROL_ZERO_PAGE_X)
                              .Write(0x00)
                              .Write(OPCODE.BRK)
                              .Write(0x20, 0x88);
            }
            _cpu.Reset();
            Assert.AreEqual(0x11, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRolAbsolute()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.SEC)
                              .Write(OPCODE.ROL_ABSOLUTE)
                              .Ref("Data")
                              .Write(OPCODE.BRK)
                              .Write(0x1020, 0x88, "Data")
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual(0x11, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRolAbsoluteX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x02)
                              .Write(OPCODE.SEC)
                              .Write(OPCODE.ROL_ABSOLUTE_X)
                              .Ref("Data")
                              .Write(OPCODE.BRK)
                              .WriteWord(0x1020, 0x00, "Data")
                              .Write(0x88)
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual(0x11, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRorAccumulatorNoCarry()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x11)
                              .Write(OPCODE.ROR_ACCUMULATOR)
                              .Write(OPCODE.BRK);
            }
            _cpu.Reset();
            Assert.AreEqual(0x08, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRorZeroPageNoCarry()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.ROR_ZERO_PAGE)
                              .Write(0x20)
                              .Write(OPCODE.BRK)
                              .Write(0x20, 0x11);
            }
            _cpu.Reset();
            Assert.AreEqual(0x08, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRorZeroPageXNoCarry()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x20)
                              .Write(OPCODE.ROR_ZERO_PAGE_X)
                              .Write(0x00)
                              .Write(OPCODE.BRK)
                              .Write(0x20, 0x11);
            }
            _cpu.Reset();
            Assert.AreEqual(0x08, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRorAbsoluteNoCarry()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.ROR_ABSOLUTE)
                              .Ref("Data")
                              .Write(OPCODE.BRK)
                              .Write(0x1020, 0x11, "Data")
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual(0x08, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRorAbsoluteXNoCarry()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x02)
                              .Write(OPCODE.ROR_ABSOLUTE_X)
                              .Ref("Data")
                              .Write(OPCODE.BRK)
                              .WriteWord(0x1020, 0x00, "Data")
                              .Write(0x11)
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual(0x08, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRorAccumulatorCarry()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x11)
                              .Write(OPCODE.SEC)
                              .Write(OPCODE.ROR_ACCUMULATOR)
                              .Write(OPCODE.BRK);
            }
            _cpu.Reset();
            Assert.AreEqual(0x88, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRorZeroPageCarry()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.SEC)
                              .Write(OPCODE.ROR_ZERO_PAGE)
                              .Write(0x20)
                              .Write(OPCODE.BRK)
                              .Write(0x20, 0x11);
            }
            _cpu.Reset();
            Assert.AreEqual(0x88, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRorZeroPageXCarry()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x20)
                              .Write(OPCODE.SEC)
                              .Write(OPCODE.ROR_ZERO_PAGE_X)
                              .Write(0x00)
                              .Write(OPCODE.BRK)
                              .Write(0x20, 0x11);
            }
            _cpu.Reset();
            Assert.AreEqual(0x88, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRorAbsoluteCarry()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.SEC)
                              .Write(OPCODE.ROR_ABSOLUTE)
                              .Ref("Data")
                              .Write(OPCODE.BRK)
                              .Write(0x1020, 0x11, "Data")
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual(0x88, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRorAbsoluteXCarry()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x02)
                              .Write(OPCODE.SEC)
                              .Write(OPCODE.ROR_ABSOLUTE_X)
                              .Ref("Data")
                              .Write(OPCODE.BRK)
                              .WriteWord(0x1020, 0x00, "Data")
                              .Write(0x11)
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual(0x88, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRolAccumulatorNoCarry()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0x88)
                              .Write(OPCODE.ROL_ACCUMULATOR)
                              .Write(OPCODE.BRK);
            }
            _cpu.Reset();
            Assert.AreEqual(0x10, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRolZeroPageNoCarry()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.ROL_ZERO_PAGE)
                              .Write(0x20)
                              .Write(OPCODE.BRK)
                              .Write(0x20, 0x88);
            }
            _cpu.Reset();
            Assert.AreEqual(0x10, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRolZeroPageXNoCarry()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x20)
                              .Write(OPCODE.ROL_ZERO_PAGE_X)
                              .Write(0x00)
                              .Write(OPCODE.BRK)
                              .Write(0x20, 0x88);
            }
            _cpu.Reset();
            Assert.AreEqual(0x10, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRolAbsoluteNoCarry()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.ROL_ABSOLUTE)
                              .Ref("Data")
                              .Write(OPCODE.BRK)
                              .Write(0x1020, 0x88, "Data")
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual(0x10, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanRolAbsoluteXNoCarry()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x02)
                              .Write(OPCODE.ROL_ABSOLUTE_X)
                              .Ref("Data")
                              .Write(OPCODE.BRK)
                              .WriteWord(0x1020, 0x00, "Data")
                              .Write(0x88)
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual(0x10, _cpu.A);
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanDecrementZeroPage()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.DEC_ZERO_PAGE)
                              .Write(0x10)
                              .Write(OPCODE.LDA_ZERO_PAGE)
                              .Write(0x10)
                              .Write(OPCODE.BRK)
                              .Write(0x10, 0x80);
            }
            _cpu.Reset();
            Assert.AreEqual(0x7F, _cpu.A);
        }

        [Test]
        public void CanDecrementZeroPageFromZero()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.DEC_ZERO_PAGE)
                              .Write(0x10)
                              .Write(OPCODE.LDA_ZERO_PAGE)
                              .Write(0x10)
                              .Write(OPCODE.BRK)
                              .Write(0x10, 0x00);
            }
            _cpu.Reset();
            Assert.AreEqual(0xFF, _cpu.A);
            Assert.IsTrue(_cpu.P.N);
        }

        [Test]
        public void CanDecrementZeroPageX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x10)
                              .Write(OPCODE.DEC_ZERO_PAGE_X)
                              .Write(0x00)
                              .Write(OPCODE.LDA_ZERO_PAGE)
                              .Write(0x10)
                              .Write(OPCODE.BRK)
                              .Write(0x10, 0x80);
            }
            _cpu.Reset();
            Assert.AreEqual(0x7F, _cpu.A);
        }

        [Test]
        public void CanDecrementAbsolute()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.DEC_ABSOLUTE)
                              .Ref("Data")
                              .Write(OPCODE.LDA_ABSOLUTE)
                              .Ref("Data")
                              .Write(OPCODE.BRK)
                              .Write(0x1010, 0x80, "Data")
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual(0x7F, _cpu.A);
        }

        [Test]
        public void CanDecrementAbsoluteX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x02)
                              .Write(OPCODE.DEC_ABSOLUTE_X)
                              .Ref("Data")
                              .Write(OPCODE.LDA_ABSOLUTE_X)
                              .Ref("Data")
                              .Write(OPCODE.BRK)
                              .WriteWord(0x1010, 0x5555, "Data")
                              .Write(0x80)
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual(0x7F, _cpu.A);
        }

        [Test]
        public void CanDecrementX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x80)
                              .Write(OPCODE.DEX)
                              .Write(OPCODE.BRK);
            }
            _cpu.Reset();
            Assert.AreEqual(0x7F, _cpu.X);
            Assert.IsFalse(_cpu.P.N);
        }

        [Test]
        public void CanDecrementY()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDY_IMMEDIATE)
                              .Write(0x80)
                              .Write(OPCODE.DEY)
                              .Write(OPCODE.BRK);
            }
            _cpu.Reset();
            Assert.AreEqual(0x7F, _cpu.Y);
            Assert.IsFalse(_cpu.P.N);
        }

        [Test]
        public void CanExclusiveOrImmediate()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0xFF)
                              .Write(OPCODE.EOR_IMMEDIATE)
                              .Write(0x55)
                              .Write(OPCODE.BRK);
            }
            _cpu.Reset();
            Assert.AreEqual(0xAA, _cpu.A);
        }

        [Test]
        public void CanExclusiveOrZeroPage()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0xFF)
                              .Write(OPCODE.EOR_ZERO_PAGE)
                              .Write(0x10)
                              .Write(OPCODE.BRK)
                              .Write(0x10, 0x55);
            }
            _cpu.Reset();
            Assert.AreEqual(0xAA, _cpu.A);
        }

        [Test]
        public void CanExclusiveOrZeroPageX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0xFF)
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x02)
                              .Write(OPCODE.EOR_ZERO_PAGE_X)
                              .Write(0x10)
                              .Write(OPCODE.BRK)
                              .Write(0x12, 0x55);
            }
            _cpu.Reset();
            Assert.AreEqual(0xAA, _cpu.A);
        }

        [Test]
        public void CanExclusiveOrAbsolute()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0xFF)
                              .Write(OPCODE.EOR_ABSOLUTE)
                              .Ref("Data")
                              .Write(OPCODE.BRK)
                              .Write(0x1010, 0x55, "Data")
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual(0xAA, _cpu.A);
        }

        [Test]
        public void CanExclusiveOrAbsoluteX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0xFF)
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x02)
                              .Write(OPCODE.EOR_ABSOLUTE_X)
                              .Ref("Data")
                              .Write(OPCODE.BRK)
                              .WriteWord(0x1010, 0xFFFF, "Data")
                              .Write(0x55)
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual(0xAA, _cpu.A);
        }

        [Test]
        public void CanExclusiveOrIndirectX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.LDA_IMMEDIATE)
                              .Write(0xFF)
                              .Write(OPCODE.LDX_IMMEDIATE)
                              .Write(0x02)
                              .Write(OPCODE.EOR_INDIRECT_X)
                              .Write(0x10)
                              .Write(OPCODE.BRK)
                              .Write(0x9000, 0x55, "Data")
                              .Ref(0x12, "Data")
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual(0xAA, _cpu.A);
        }

        [Test]
        public void CanExclusiveOrIndirectY()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
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
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual(0xAA, _cpu.A);
        }

        [Test]
        public void CanCauseStackOverflow()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.JSR, "InfiniteLoop")
                              .Ref("InfiniteLoop")
                              ;
            }
            _cpu.Reset();
            Assert.IsTrue(_cpu.HaltReason == HaltReason.StackOverflow);
        }

        [Test]
        public void CanCauseStackUnderflow()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                              .Write(OPCODE.RTS);
            }
            _cpu.Reset();
            Assert.IsTrue(_cpu.HaltReason == HaltReason.StackUnderflow);
        }

        [Test]
        public void CanReturnFromInterrupt()
        {
            // Manually push a return address and processor flags to the stack
            // to simulate an interrupt
            using (var _ = mem.Load(PROG_START))
            {
                _
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
            }
            _cpu.Reset();
            Assert.AreEqual(0x9000, _cpu.PC);
            Assert.AreEqual(0x51, _cpu.P.AsByte());
        }

        [Test]
        public void CanServiceInterrupt()
        {
            _cpu.OnTick += async (s,e) => {await TriggerInterrupt(s,e);};

            _tickCount = 0;
            _interruptTickInterval = 600;

            using (var _ = mem.Load(PROG_START))
            {
                _
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
                              ;
            }
            _cpu.Reset();
            var result = mem.Read(0x7800);
            Assert.AreEqual(42, result);
        }

        [Test]
        public void LetsDoThis()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
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
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual('H', mem.Read(DISPLAY_BASE_ADDR));
        }

        [TestCase(1, 127, 128)]
        [TestCase(1, 255, 256)]
        public void CanAdd16Bit(int v1, int v2, int expected)
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                    .CLC()
                    .LDA_ABSOLUTE("v1")
                    .ADC_ABSOLUTE("v2")
                    .STA_ABSOLUTE("Result")
                    .LDA_ABSOLUTE("v1+1")
                    .ADC_ABSOLUTE("v2+1")
                    .STA_ABSOLUTE("Result+1")
                    .BRK()
                    .WriteWord((ushort)v1, "v1")
                    .WriteWord((ushort)v2, "v2")
                    .WriteWord(0x8200, 0x0000, "Result");
            }
            _cpu.Reset();
            var result = mem.ReadWord(0x8200);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void CanDrawColumnAndRowHeadersOnScreen()
        {
            Assert.IsTrue(_display.Mode.Type == DisplayMode.RenderType.Text);

            var w = _display.Mode.Width;
            var h = _display.Mode.Height;
            var bpr = _display.Mode.BytesPerRow;

            /*
             * I'm using a different coding style here, with labels
             * explicitly created using the .Label() method.
             * The alternative, used elsewhere, is to add the label
             * as the final parameter of the instruction that starts
             * at the label address (so, the instruction that otherwise
             * comes after the .Label() call).
             */
             
            using (var _ = mem.Load(PROG_START))
            {
                _
                    // Write column header
                    .LDX_IMMEDIATE(0x00)
                    .JSR("ResetDigit")

                .Label("ColumnLoop")
                    .LDA_ZERO_PAGE("CurrentDigit")
                    .STA_ABSOLUTE_X(DISPLAY_BASE_ADDR)
                    .INX()
                    .CPX_IMMEDIATE((byte)w)
                    .BCS("DoneColumns")

                    .JSR("IncrementDigit")
                    .JMP_ABSOLUTE("ColumnLoop")

                    // Write row labels
                .Label("DoneColumns")
                    .JSR("ResetDigit")
                    .LDY_IMMEDIATE((byte)(h - 1))

                .Label("IncrementRowAddress")
                    .CLC()
                    .LDA_IMMEDIATE(bpr.Lsb())
                    .ADC_ZERO_PAGE("DisplayVector")
                    .STA_ZERO_PAGE("DisplayVector")
                    .LDA_IMMEDIATE(bpr.Msb())
                    .ADC_ZERO_PAGE("DisplayVector+1")
                    .STA_ZERO_PAGE("DisplayVector+1")

                    .JSR("IncrementDigit")

                    .LDA_ZERO_PAGE("CurrentDigit")
                    .LDX_IMMEDIATE(0x00)
                    .STA_INDIRECT_X("DisplayVector")

                    .DEY()
                    .BNE("IncrementRowAddress")
                .Label("Finished")
                    .BRK()

                .Label("ResetDigit")
                    .LDA_IMMEDIATE((byte)'0')
                    .STA_ZERO_PAGE("CurrentDigit")
                    .RTS()

                .Label("IncrementDigit")
                    .LDA_ZERO_PAGE("CurrentDigit")
                    .CMP_IMMEDIATE((byte)'9')
                    .BCS("ResetDigit") // Sneakily jump to the reset routine
                    .INC_ZERO_PAGE("CurrentDigit")
                    .RTS()

                    .Write(0x10, '0', "CurrentDigit")
                    .WriteWord(0x12, DISPLAY_BASE_ADDR, "DisplayVector");
            }

            _cpu.Reset();
            Assert.AreEqual('0', mem.Read(DISPLAY_BASE_ADDR));
            var expected = (h + 9) % 10 + '0';
            Assert.AreEqual(expected, mem.Read((ushort)(DISPLAY_BASE_ADDR + (h - 1) * w)));
        }

        private async Task TriggerInterrupt(object sender, EventArgs e)
        {
            _tickCount++;
            if (_tickCount >= _interruptTickInterval)
            {
                _tickCount = 0;
                await _cpu.Interrupt();
            }
        }
    }
}