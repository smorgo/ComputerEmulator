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
                .LDA_IMMEDIATE('H')
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
                .LDA_IMMEDIATE('H')
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
                .LDA_IMMEDIATE('H')
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
                    .LDA_IMMEDIATE('H')
                    .LDX_IMMEDIATE(1)
                    .STA_ABSOLUTE_X(DISPLAY_BASE_ADDR)
                    .LDY_ABSOLUTE(DISPLAY_BASE_ADDR + 1);
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
                    .LDA_IMMEDIATE('H')
                    .LDY_IMMEDIATE(1)
                    .STA_ABSOLUTE_Y(DISPLAY_BASE_ADDR)
                    .LDX_ABSOLUTE(DISPLAY_BASE_ADDR + 1);
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
                    .LDA_IMMEDIATE('H')
                    .LDX_IMMEDIATE(2)
                    .STA_INDIRECT_X(0x00)
                    .LDY_ABSOLUTE(DISPLAY_BASE_ADDR)
                    
                    // Data
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
                    .LDA_IMMEDIATE('H')
                    .LDY_IMMEDIATE(2)
                    .STA_INDIRECT_Y(0x00)
                    .LDX_ABSOLUTE(DISPLAY_BASE_ADDR + 2)

                    // Data
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
                    .LDX_IMMEDIATE('H')
                    .STX_ZERO_PAGE(0x20)
                    .LDA_ZERO_PAGE(0x20);
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
                    .LDX_IMMEDIATE('H')
                    .LDY_IMMEDIATE(0x10)
                    .STX_ZERO_PAGE_Y(0x10)
                    .LDA_ZERO_PAGE(0x20);
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
                    .LDX_IMMEDIATE('H')
                    .STX_ABSOLUTE(DISPLAY_BASE_ADDR)
                    .LDA_ABSOLUTE(DISPLAY_BASE_ADDR);
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
                    .LDY_IMMEDIATE('H')
                    .STY_ZERO_PAGE(0x20)
                    .LDA_ZERO_PAGE(0x20);
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
                    .LDY_IMMEDIATE('H')
                    .LDX_IMMEDIATE(0x10)
                    .STY_ZERO_PAGE_X(0x10)
                    .LDA_ZERO_PAGE(0x20);
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
                    .LDY_IMMEDIATE('H')
                    .STY_ABSOLUTE(DISPLAY_BASE_ADDR)
                    .LDA_ABSOLUTE(DISPLAY_BASE_ADDR);
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
                    .LDA_IMMEDIATE('H')
                    .TAX();
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
                    .LDA_IMMEDIATE('H')
                    .TAY();
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
                    .LDX_IMMEDIATE('H')
                    .TXA();
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
                    .LDY_IMMEDIATE('H')
                    .TYA();
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
                    .LDA_IMMEDIATE(0x7F)
                    .PHA()
                    .PHA()
                    .TSX()
                    .BRK();
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
                    .LDA_IMMEDIATE(0x7F)
                    .PHA()
                    .PHA()
                    .LDX_IMMEDIATE(0x80)
                    .TXS()
                    .BRK();
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
                    .LDA_IMMEDIATE('H')
                    .PHA()
                    .LDY_ABSOLUTE(0x1FF);
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
                    .LDA_IMMEDIATE(0x00)
                    .PHP()
                    .LDY_ABSOLUTE(0x1FF);
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
                    .LDA_IMMEDIATE('H')
                    .PHA()
                    .LDA_IMMEDIATE(0x00)
                    .PLA();
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
                    .LDA_IMMEDIATE(0xFF)
                    .PHA()
                    .PLP()
                    .BRK();
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
                    .LDA_IMMEDIATE('H')
                    .JMP_ABSOLUTE("Continue")
                    .BRK()
                    .LDA_IMMEDIATE('e', "Continue");
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
                    .LDA_IMMEDIATE('H')
                    .JMP_INDIRECT("Vector")
                    .BRK()
                    .LDA_IMMEDIATE('e', "Continue")
                    .BRK()
                .Label("Vector")
                    .Ref("Continue");
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
                    .LDA_IMMEDIATE('H')
                    .JSR("Sub")
                    .BRK()
                .Label("Sub")
                    .LDA_IMMEDIATE('e')
                    .BRK();  // No RTS - we're only testing the jump
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
                    .LDA_IMMEDIATE('H')
                    .JSR("Sub")
                    .LDA_IMMEDIATE('l')
                    .BRK()
                .Label("Sub")
                    .LDA_IMMEDIATE('e')
                    .RTS();
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
                    .SEC();
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
                    .SEC()
                    .CLC();
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
                    .LDA_IMMEDIATE(0x50)
                    .ADC_IMMEDIATE(0x50)
                    .CLV();
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
                    .SEI();
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
                    .SEI()
                    .CLI();
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
                    .SED();
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
                    .SED()
                    .CLD();
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
                    .CLC()
                    .BCC("Cont")
                    .LDA_IMMEDIATE('H')
                    .BRK()
                    .LDA_IMMEDIATE('e', "Cont")
                    .BRK();
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
                    .JMP_ABSOLUTE("Continue")
                    .LDA_IMMEDIATE('e', "Back")
                    .BRK()
                    .CLC("Continue")
                    .BCC("Back")
                    .LDA_IMMEDIATE('H')
                    .BRK();
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
                    .SEC()
                    .BCS("Cont")
                    .LDA_IMMEDIATE('H')
                    .BRK()
                    .LDA_IMMEDIATE('e', "Cont")
                    .BRK();
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
                    .JMP_ABSOLUTE("Continue")
                    .LDA_IMMEDIATE('e', "Back")
                    .BRK()
                    .SEC("Continue")
                    .BCS("Back")
                    .LDA_IMMEDIATE('H')
                    .BRK();
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
                    .LDA_IMMEDIATE(0x22)
                    .ADC_IMMEDIATE(0x23)
                    .ASL_ACCUMULATOR()
                    .BVC("Cont")
                    .LDA_IMMEDIATE('H')
                    .BRK()
                    .LDA_IMMEDIATE('e', "Cont")
                    .BRK();
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
                    .LDA_IMMEDIATE(0x42)
                    .ADC_IMMEDIATE(0x43)
                    .ASL_ACCUMULATOR()
                    .BVS("Cont")
                    .LDA_IMMEDIATE('H')
                    .BRK()
                    .LDA_IMMEDIATE('e', "Cont")
                    .BRK();
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
                    .LDA_IMMEDIATE(0x88)
                    .BMI("Cont")
                    .LDA_IMMEDIATE('H')
                    .BRK()
                    .LDA_IMMEDIATE('e', "Cont")
                    .BRK();
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
                    .JMP_ABSOLUTE("Continue")
                    .LDA_IMMEDIATE('e', "Back")
                    .BRK()
                    .LDA_IMMEDIATE(0x88, "Continue")
                    .BMI("Back")
                    .LDA_IMMEDIATE('H')
                    .BRK();
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
                    .LDA_IMMEDIATE(0x08)
                    .BPL("Cont")
                    .LDA_IMMEDIATE('H')
                    .BRK()
                    .LDA_IMMEDIATE('e', "Cont")
                    .BRK();
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
                    .JMP_ABSOLUTE("Start")
                    .LDA_IMMEDIATE('e', "Back")
                    .BRK()
                    .LDA_IMMEDIATE(0x08)
                    .BPL("Back", "Start")
                    .LDA_IMMEDIATE('H')
                    .BRK();
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
                    .LDA_IMMEDIATE('H')
                    .LDY_IMMEDIATE(0x00)
                    .CMP_IMMEDIATE('H')
                    .BRK();
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
                    .LDA_IMMEDIATE(0x90)
                    .LDY_IMMEDIATE(0x00)
                    .CMP_IMMEDIATE(0xFF)
                    .BRK();
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
                    .LDA_IMMEDIATE(0x90)
                    .LDY_IMMEDIATE(0x00)
                    .CMP_IMMEDIATE(0x00)
                    .BRK();
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
                    .LDA_IMMEDIATE('H')
                    .LDY_IMMEDIATE(0x00)
                    .CMP_ZERO_PAGE(0x10)
                    .BRK()
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
                    .LDA_IMMEDIATE('H')
                    .LDX_IMMEDIATE(0x02)
                    .CMP_ZERO_PAGE_X(0x10)
                    .BRK()
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
                    .LDA_IMMEDIATE('H')
                    .LDY_IMMEDIATE(0x00)
                    .CMP_ABSOLUTE("Data")
                    .BRK()
                    .Write(0x1010, 'H', "Data");
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
                    .LDA_IMMEDIATE('H')
                    .LDX_IMMEDIATE(0x10)
                    .CMP_ABSOLUTE_X(0x1000)
                    .BRK()
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
                    .LDA_IMMEDIATE('H')
                    .LDX_IMMEDIATE(0x02)
                    .CMP_INDIRECT_X(0x0E)
                    .BRK()
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
                    .LDA_IMMEDIATE('H')
                    .LDY_IMMEDIATE(0x02)
                    .CMP_INDIRECT_Y(0x10)
                    .BRK()
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
                    .LDX_IMMEDIATE('H')
                    .LDY_IMMEDIATE(0x00)
                    .CPX_IMMEDIATE('H')
                    .BRK();
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
                    .LDX_IMMEDIATE(0x90)
                    .LDY_IMMEDIATE(0x00)
                    .CPX_IMMEDIATE(0xFF)
                    .BRK();
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
                    .LDX_IMMEDIATE(0x90)
                    .LDY_IMMEDIATE(0x00)
                    .CPX_IMMEDIATE(0x00)
                    .BRK();
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
                    .LDX_IMMEDIATE('H')
                    .LDY_IMMEDIATE(0x00)
                    .CPX_ZERO_PAGE(0x10)
                    .BRK()
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
                    .LDX_IMMEDIATE('H')
                    .LDY_IMMEDIATE(0x00)
                    .CPX_ABSOLUTE("Data")
                    .BRK()
                    .Write(0x1010, 'H', "Data");
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
                    .LDY_IMMEDIATE('H')
                    .LDX_IMMEDIATE(0x00)
                    .CPY_IMMEDIATE('H')
                    .BRK();
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
                    .LDY_IMMEDIATE(0x90)
                    .LDX_IMMEDIATE(0x00)
                    .CPY_IMMEDIATE(0xFF)
                    .BRK();
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
                    .LDY_IMMEDIATE(0x90)
                    .LDX_IMMEDIATE(0x00)
                    .CPY_IMMEDIATE(0x00)
                    .BRK();
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
                    .LDY_IMMEDIATE('H')
                    .LDX_IMMEDIATE(0x00)
                    .CPY_ZERO_PAGE(0x10)
                    .BRK()
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
                    .LDY_IMMEDIATE('H')
                    .LDX_IMMEDIATE(0x00)
                    .CPY_ABSOLUTE("Data")
                    .BRK()
                    .Write(0x1010, 'H', "Data");
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
                    .LDA_IMMEDIATE('H')
                    .CMP_IMMEDIATE('H')
                    .BEQ("Next")
                    .LDA_IMMEDIATE('N')
                    .BRK()
                    .LDA_IMMEDIATE('Y', "Next")
                    .BRK();
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
                    .LDA_IMMEDIATE('H')
                    .CMP_IMMEDIATE('e')
                    .BNE("Next")
                    .LDA_IMMEDIATE('N')
                    .BRK()
                    .LDA_IMMEDIATE('Y', "Next")
                    .BRK();
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
                    .LDA_IMMEDIATE(0x0A)
                    .BIT_ABSOLUTE(0x1000)
                    .BRK()
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
                    .LDA_IMMEDIATE(0x1A)
                    .BIT_ABSOLUTE(0x1000)
                    .BRK()
                    .Write(0x1000, 0xF0);
            }
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.Z);
        }

        [Test]
        public void CanBitTestZeroPageAllMasked()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                    .LDA_IMMEDIATE(0x0A)
                    .BIT_ZERO_PAGE(0x10)
                    .BRK()
                    .Write(0x10, 0xF0);
            }
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.Z);
        }

        [Test]
        public void CanBitTestZeroPageNotAllMasked()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                    .LDA_IMMEDIATE(0x1A)
                    .BIT_ZERO_PAGE(0x10)
                    .BRK()
                    .Write(0x10, 0xF0);
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
                    .LDA_IMMEDIATE(0x01)
                    .ADC_IMMEDIATE(0x02)
                    .BRK();
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
                    .LDA_IMMEDIATE(v1)
                    .ADC_IMMEDIATE(v2)
                    .BRK();
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
                    .CLC()
                    .LDX_IMMEDIATE(0x00)
                    .LDY_IMMEDIATE(0x01)
                    .LDA_ABSOLUTE_X("V1", "Loop")
                    .ADC_ABSOLUTE_X("V2")
                    .STA_ABSOLUTE_X("Result")
                    .INX()
                    .DEY()
                    .BPL("Loop")
                    .BRK()
                    .WriteWord(0x9000, (ushort)v1, "V1")
                    .WriteWord((ushort)v2, "V2")
                    .WriteWord(0x0000, "Result");
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
                    .LDA_IMMEDIATE(0x01)
                    .ADC_ZERO_PAGE(0x01)
                    .BRK()
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
                    .LDA_IMMEDIATE(0x01)
                    .LDX_IMMEDIATE(0x02)
                    .ADC_ZERO_PAGE_X(0x03)
                    .BRK()
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
                    .LDA_IMMEDIATE(0x01)
                    .ADC_ABSOLUTE("Data")
                    .BRK()
                    .Write(0x1005, 0x02, "Data");
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
                    .LDA_IMMEDIATE(0x01)
                    .LDX_IMMEDIATE(0x01)
                    .ADC_ABSOLUTE_X(0x1004)
                    .BRK()
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
                    .LDA_IMMEDIATE(0x03)
                    .SBC_IMMEDIATE(0x02)
                    .BRK();
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
                    .LDA_IMMEDIATE(v1)
                    .SBC_IMMEDIATE(v2)
                    .BRK();
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
                    .CLC()
                    .LDX_IMMEDIATE(0x00)
                    .LDY_IMMEDIATE(0x01)
                    .LDA_ABSOLUTE_X("V1", "Loop")
                    .SBC_ABSOLUTE_X("V2")
                    .STA_ABSOLUTE_X("Result")
                    .INX()
                    .DEY()
                    .BPL("Loop")
                    .BRK()
                    .WriteWord(0x9000, (ushort)v1, "V1")
                    .WriteWord((ushort)v2, "V2")
                    .WriteWord(0x0000, "Result");
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
                    .LDA_IMMEDIATE(0x05)
                    .SBC_ZERO_PAGE(0x01)
                    .BRK()
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
                    .LDA_IMMEDIATE(0x05)
                    .LDX_IMMEDIATE(0x02)
                    .SBC_ZERO_PAGE_X(0x03)
                    .BRK()
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
                    .LDA_IMMEDIATE(0x05)
                    .SBC_ABSOLUTE("Data")
                    .BRK()
                    .Write(0x1005, 0x02, "Data");
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
                    .LDA_IMMEDIATE(0x05)
                    .LDX_IMMEDIATE(0x01)
                    .SBC_ABSOLUTE_X(0x1004)
                    .BRK()
                    .Write(0x1005, 0x02);
            }
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x03, _cpu.A);
        }

        [Test]
        public void CanSubtractWithCarryAbsoluteY()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                    .LDA_IMMEDIATE(0x05)
                    .LDY_IMMEDIATE(0x01)
                    .SBC_ABSOLUTE_Y(0x1004)
                    .BRK()
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
                    .LDA_IMMEDIATE(0x01)
                    .LDY_IMMEDIATE(0x01)
                    .ADC_ABSOLUTE_Y(0x1004)
                    .BRK()
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
                    .LDA_IMMEDIATE(0x01)
                    .LDX_IMMEDIATE(0x02)
                    .ADC_INDIRECT_X(0x00)
                    .BRK()
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
                    .LDA_IMMEDIATE(0x01)
                    .LDY_IMMEDIATE(0x02)
                    .ADC_INDIRECT_Y(0x08)
                    .BRK()
                    .WriteWord(0x08, 0x1234)
                    .Write(0x1236, 0x02);
            }
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C | _cpu.P.Z | _cpu.P.V | _cpu.P.N);
            Assert.AreEqual(0x03, _cpu.A);
        }

        [Test]
        public void CanSubtractWithCarryIndirectX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                    .LDA_IMMEDIATE(0x01)
                    .LDX_IMMEDIATE(0x02)
                    .SBC_INDIRECT_X(0x00)
                    .BRK()
                    .WriteWord(0x02, 0x1234)
                    .Write(0x1234, 0x02);
            }
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.Z);
            Assert.IsTrue(_cpu.P.C & _cpu.P.V & _cpu.P.N);
            Assert.AreEqual(0xFF, _cpu.A);
        }
        [Test]
        public void CanSubtractWithCarryIndirectY()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                    .LDA_IMMEDIATE(0x01)
                    .LDY_IMMEDIATE(0x02)
                    .SBC_INDIRECT_Y(0x08)
                    .BRK()
                    .WriteWord(0x08, 0x1234)
                    .Write(0x1236, 0x02);
            }
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.Z);
            Assert.IsTrue(_cpu.P.C & _cpu.P.V & _cpu.P.N);
            Assert.AreEqual(0xFF, _cpu.A);
        }

        [Test]
        public void CanAndImmediate()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                    .LDA_IMMEDIATE(0xFF)
                    .AND_IMMEDIATE(0x80)
                    .BRK();
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
                    .LDA_IMMEDIATE(0xFF)
                    .AND_ZERO_PAGE(0x10)
                    .BRK()
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
                    .LDA_IMMEDIATE(0xFF)
                    .LDX_IMMEDIATE(0x10)
                    .AND_ZERO_PAGE_X(0x00)
                    .BRK()
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
                    .LDA_IMMEDIATE(0xFF)
                    .AND_ABSOLUTE(0x1010)
                    .BRK()
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
                    .LDA_IMMEDIATE(0xFF)
                    .LDX_IMMEDIATE(0x10)
                    .AND_ABSOLUTE_X(0x1000)
                    .BRK()
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
                    .LDA_IMMEDIATE(0xFF)
                    .LDY_IMMEDIATE(0x10)
                    .AND_ABSOLUTE_Y(0x1000)
                    .BRK()
                    .Write(0x1010, 0x80);
            }
            _cpu.Reset();
            Assert.AreEqual(0x80, _cpu.A);
        }

        [Test]
        public void CanAndIndirectX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                    .LDA_IMMEDIATE(0xFF)
                    .LDX_IMMEDIATE(0x02)
                    .AND_INDIRECT_X("Vector")
                    .BRK()
                    
                    // Data
                    .WriteWord(0x10, 0xFFFF, "Vector")
                    .WriteWord(0x1010)
                    .Write(0x1010, 0x80);
            }
            _cpu.Reset();
            Assert.AreEqual(0x80, _cpu.A);
        }

        [Test]
        public void CanAndIndirectY()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                    .LDA_IMMEDIATE(0xFF)
                    .LDY_IMMEDIATE(0x02)
                    .AND_INDIRECT_Y("Vector")
                    .BRK()
                    
                    // Data
                    .WriteWord(0x10, 0x100E, "Vector")
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
                    .LDA_IMMEDIATE(0x0F)
                    .ORA_IMMEDIATE(0x80)
                    .BRK();
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
                    .LDA_IMMEDIATE(0x0F)
                    .ORA_ZERO_PAGE(0x10)
                    .BRK()
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
                    .LDA_IMMEDIATE(0x0F)
                    .LDX_IMMEDIATE(0x10)
                    .ORA_ZERO_PAGE_X(0x00)
                    .BRK()
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
                    .LDA_IMMEDIATE(0x0F)
                    .ORA_ABSOLUTE(0x1010)
                    .BRK()
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
                    .LDA_IMMEDIATE(0x0F)
                    .LDX_IMMEDIATE(0x10)
                    .ORA_ABSOLUTE_X(0x1000)
                    .BRK()
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
                    .LDA_IMMEDIATE(0x0F)
                    .LDY_IMMEDIATE(0x10)
                    .ORA_ABSOLUTE_Y(0x1000)
                    .BRK()
                    .Write(0x1010, 0x80);
            }
            _cpu.Reset();
            Assert.AreEqual(0x8F, _cpu.A);
        }

        [Test]
        public void CanOrIndirectX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                    .LDA_IMMEDIATE(0x0F)
                    .LDX_IMMEDIATE(0x02)
                    .ORA_INDIRECT_X("Vector")
                    .BRK()
                    
                    // Data
                    .WriteWord(0x10, 0xFFFF, "Vector")
                    .WriteWord(0x1010)
                    .Write(0x1010, 0x80);
            }
            _cpu.Reset();
            Assert.AreEqual(0x8F, _cpu.A);
        }

        [Test]
        public void CanOrIndirectY()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                    .LDA_IMMEDIATE(0x0F)
                    .LDY_IMMEDIATE(0x02)
                    .ORA_INDIRECT_Y("Vector")
                    .BRK()
                    
                    // Data
                    .WriteWord(0x10, 0x100E, "Vector")
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
                    .LDA_IMMEDIATE(0x88)
                    .ASL_ACCUMULATOR()
                    .BRK();
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
                    .ASL_ZERO_PAGE(0x20)
                    .BRK()
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
                    .LDX_IMMEDIATE(0x20)
                    .ASL_ZERO_PAGE_X(0x00)
                    .BRK()
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
                    .ASL_ABSOLUTE("Data")
                    .BRK()
                    .Write(0x1020, 0x88, "Data");
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
                    .LDX_IMMEDIATE(0x02)
                    .ASL_ABSOLUTE_X("Data")
                    .BRK()
                    .WriteWord(0x1020, 0x00, "Data")
                    .Write(0x88);
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
                    .LDA_IMMEDIATE(0x11)
                    .LSR_ACCUMULATOR()
                    .BRK();
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
                    .LSR_ZERO_PAGE(0x20)
                    .BRK()
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
                    .LDX_IMMEDIATE(0x20)
                    .LSR_ZERO_PAGE_X(0x00)
                    .BRK()
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
                    .LSR_ABSOLUTE("Data")
                    .BRK()
                    .Write(0x1020, 0x11, "Data");
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
                    .LDX_IMMEDIATE(0x02)
                    .LSR_ABSOLUTE_X("Data")
                    .BRK()
                    .WriteWord(0x1020, 0x00, "Data")
                    .Write(0x11);
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
                    .LDA_IMMEDIATE(0x88)
                    .SEC()
                    .ROL_ACCUMULATOR()
                    .BRK();
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
                    .SEC()
                    .ROL_ZERO_PAGE(0x20)
                    .BRK()
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
                    .LDX_IMMEDIATE(0x20)
                    .SEC()
                    .ROL_ZERO_PAGE_X(0x00)
                    .BRK()
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
                    .SEC()
                    .ROL_ABSOLUTE("Data")
                    .BRK()
                    .Write(0x1020, 0x88, "Data");
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
                    .LDX_IMMEDIATE(0x02)
                    .SEC()
                    .ROL_ABSOLUTE_X("Data")
                    .BRK()
                    .WriteWord(0x1020, 0x00, "Data")
                    .Write(0x88);
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
                    .LDA_IMMEDIATE(0x11)
                    .ROR_ACCUMULATOR()
                    .BRK();
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
                    .ROR_ZERO_PAGE(0x20)
                    .BRK()
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
                    .LDX_IMMEDIATE(0x20)
                    .ROR_ZERO_PAGE_X(0x00)
                    .BRK()
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
                    .ROR_ABSOLUTE("Data")
                    .BRK()
                    .Write(0x1020, 0x11, "Data");
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
                    .LDX_IMMEDIATE(0x02)
                    .ROR_ABSOLUTE_X("Data")
                    .BRK()
                    .WriteWord(0x1020, 0x00, "Data")
                    .Write(0x11);
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
                    .LDA_IMMEDIATE(0x11)
                    .SEC()
                    .Write(OPCODE.ROR_ACCUMULATOR)
                    .BRK();
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
                    .SEC()
                    .ROR_ZERO_PAGE(0x20)
                    .BRK()
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
                    .LDX_IMMEDIATE(0x20)
                    .SEC()
                    .ROR_ZERO_PAGE_X(0x00)
                    .BRK()
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
                    .SEC()
                    .ROR_ABSOLUTE("Data")
                    .BRK()
                    .Write(0x1020, 0x11, "Data");
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
                    .LDX_IMMEDIATE(0x02)
                    .SEC()
                    .ROR_ABSOLUTE_X("Data")
                    .BRK()
                    .WriteWord(0x1020, 0x00, "Data")
                    .Write(0x11);
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
                    .LDA_IMMEDIATE(0x88)
                    .ROL_ACCUMULATOR()
                    .BRK();
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
                    .ROL_ZERO_PAGE(0x20)
                    .BRK()
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
                    .LDX_IMMEDIATE(0x20)
                    .ROL_ZERO_PAGE_X(0x00)
                    .BRK()
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
                    .ROL_ABSOLUTE("Data")
                    .BRK()
                    .Write(0x1020, 0x88, "Data");
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
                    .LDX_IMMEDIATE(0x02)
                    .ROL_ABSOLUTE_X("Data")
                    .BRK()
                    .WriteWord(0x1020, 0x00, "Data")
                    .Write(0x88);
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
                    .DEC_ZERO_PAGE(0x10)
                    .LDA_ZERO_PAGE(0x10)
                    .BRK()
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
                    .DEC_ZERO_PAGE(0x10)
                    .LDA_ZERO_PAGE(0x10)
                    .BRK()
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
                    .LDX_IMMEDIATE(0x10)
                    .DEC_ZERO_PAGE_X(0x00)
                    .LDA_ZERO_PAGE(0x10)
                    .BRK()
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
                    .DEC_ABSOLUTE("Data")
                    .LDA_ABSOLUTE("Data")
                    .BRK()
                    .Write(0x1010, 0x80, "Data");
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
                    .LDX_IMMEDIATE(0x02)
                    .DEC_ABSOLUTE_X("Data")
                    .LDA_ABSOLUTE_X("Data")
                    .BRK()
                    .WriteWord(0x1010, 0x5555, "Data")
                    .Write(0x80);
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
                    .LDX_IMMEDIATE(0x80)
                    .DEX()
                    .BRK();
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
                    .LDY_IMMEDIATE(0x80)
                    .DEY()
                    .BRK();
            }
            _cpu.Reset();
            Assert.AreEqual(0x7F, _cpu.Y);
            Assert.IsFalse(_cpu.P.N);
        }

        [Test]
        public void CanIncrementZeroPage()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                    .INC_ZERO_PAGE(0x10)
                    .LDA_ZERO_PAGE(0x10)
                    .BRK()
                    .Write(0x10, 0x80);
            }
            _cpu.Reset();
            Assert.AreEqual(0x81, _cpu.A);
        }

        [Test]
        public void CanIncrementZeroPageFromMinus1()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                    .INC_ZERO_PAGE(0x10)
                    .LDA_ZERO_PAGE(0x10)
                    .BRK()
                    .Write(0x10, 0xFF);
            }
            _cpu.Reset();
            Assert.AreEqual(0x0, _cpu.A);
            Assert.IsFalse(_cpu.P.N);
        }

        [Test]
        public void CanIncrementZeroPageX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                    .LDX_IMMEDIATE(0x10)
                    .INC_ZERO_PAGE_X(0x00)
                    .LDA_ZERO_PAGE(0x10)
                    .BRK()
                    .Write(0x10, 0x80);
            }
            _cpu.Reset();
            Assert.AreEqual(0x81, _cpu.A);
        }

        [Test]
        public void CanIncrementAbsolute()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                    .INC_ABSOLUTE("Data")
                    .LDA_ABSOLUTE("Data")
                    .BRK()
                    .Write(0x1010, 0x80, "Data");
            }
            _cpu.Reset();
            Assert.AreEqual(0x81, _cpu.A);
        }

        [Test]
        public void CanIncrementAbsoluteX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                    .LDX_IMMEDIATE(0x02)
                    .INC_ABSOLUTE_X("Data")
                    .LDA_ABSOLUTE_X("Data")
                    .BRK()
                    .WriteWord(0x1010, 0x5555, "Data")
                    .Write(0x80);
            }
            _cpu.Reset();
            Assert.AreEqual(0x81, _cpu.A);
        }

        [Test]
        public void CanIncrementX()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                    .LDX_IMMEDIATE(0x80)
                    .INX()
                    .BRK();
            }
            _cpu.Reset();
            Assert.AreEqual(0x81, _cpu.X);
            Assert.IsTrue(_cpu.P.N);
        }

        [Test]
        public void CanIncrementY()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                    .LDY_IMMEDIATE(0x80)
                    .INY()
                    .BRK();
            }
            _cpu.Reset();
            Assert.AreEqual(0x81, _cpu.Y);
            Assert.IsTrue(_cpu.P.N);
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
                              .BRK();
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
                              .BRK()
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
                              .BRK()
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
                              .BRK()
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
                              .BRK()
                              .WriteWord(0x1010, 0xFFFF, "Data")
                              .Write(0x55)
                              ;
            }
            _cpu.Reset();
            Assert.AreEqual(0xAA, _cpu.A);
        }

        [Test]
        public void CanExclusiveOrAbsoluteY()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                    .LDA_IMMEDIATE(0xFF)
                    .LDY_IMMEDIATE(0x02)
                    .EOR_ABSOLUTE_Y("Data")
                    .BRK()
                    .WriteWord(0x1010, 0xFFFF, "Data")
                    .Write(0x55);
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
                              .BRK()
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
                              .BRK()
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
                    .JSR("InfiniteLoop", "InfiniteLoop");
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
                    .RTS();
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
                    .LDA_IMMEDIATE(0x90)
                    .PHA()
                    .LDA_IMMEDIATE(0x00)
                    .PHA()
                    .LDA_IMMEDIATE(0x55)
                    .PHA()
                    .RTI()
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
                    .LDX_IMMEDIATE(0x00)
                    .LDY_IMMEDIATE(0x20, "LoopX")
                    .TXA("LoopY")
                    .STA_INDIRECT_Y(0x10)    // Holds vector for data block to write
                    .DEY()
                    .BNE("LoopY")
                    .INX()
                    .BNE("LoopX")
                    .BRK()
                    .INC_ABSOLUTE(0x7800, "ISR")
                    .RTI()
                    .Write(0x7000, 0x00, "Block")
                    .Write(0x7800, 0x00) // ISR will increment this
                    .Ref(0x10, "Block")
                    .Ref(_cpu.IRQ_VECTOR, "ISR"); // Write the address of the ISR into the Interrupt Vector
            }
            _cpu.Reset();
            var result = mem.Read(0x7800);
            Assert.AreEqual(56, result);
        }

        [Test]
        public void LetsDoThis()
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                    .LDX_IMMEDIATE(0x00) // X will be our index into the data and the screen
                    .LDA_ABSOLUTE_X("Data", "Loop")
                    .BEQ("Finished")
                    .STA_ABSOLUTE_X(DISPLAY_BASE_ADDR)
                    .INX()
                    .BNE("Loop")
                    .BRK("Finished")
                    .WriteString("Hello, World!", "Data")
                    .Write(0x00);
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

        [TestCase(127, 128, -1)]
        [TestCase(-1, -255, 254)]
        public void CanSubtract16Bit(int v1, int v2, int expected)
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                    .CLC()
                    .LDA_ABSOLUTE("v1")
                    .SBC_ABSOLUTE("v2")
                    .STA_ABSOLUTE("Result")
                    .LDA_ABSOLUTE("v1+1")
                    .SBC_ABSOLUTE("v2+1")
                    .STA_ABSOLUTE("Result+1")
                    .BRK()
                    .WriteWord((ushort)v1, "v1")
                    .WriteWord((ushort)v2, "v2")
                    .WriteWord(0x8200, 0x0000, "Result");
            }
            _cpu.Reset();
            var result = mem.ReadWord(0x8200);
            Assert.AreEqual((ushort)expected, (ushort)result);
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
                    .LDA_IMMEDIATE('0')
                    .STA_ZERO_PAGE("CurrentDigit")
                    .RTS()

                .Label("IncrementDigit")
                    .LDA_ZERO_PAGE("CurrentDigit")
                    .CMP_IMMEDIATE('9')
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