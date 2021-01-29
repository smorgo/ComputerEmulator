using NUnit.Framework;
using _6502;
using System;

namespace Tests
{
    public class CpuTests
    {
        private  CPU6502 _cpu;
        private MemoryMappedDisplay _display;
        private AddressMap _map;
        const ushort DISPLAY_BASE_ADDR = 0xF000;
        const ushort DISPLAY_SIZE = 40;

        [SetUp]
        public void Setup()
        {
            _map = new AddressMap();
            _map.Install(new Ram(0x0000, 0x10000));
            _display = new MemoryMappedDisplay(DISPLAY_BASE_ADDR, DISPLAY_SIZE);
            _map.Install(_display);
            _display.Clear();
            _cpu = new CPU6502(_map);
            _cpu.DebugLevel = DebugLevel.Verbose;
            _map.WriteWord(0xFFFC, 0x8000); 
        }

        [Test]
        public void CanReset()
        {
            _cpu.Reset();
        }

        [Test]
        public void CanLoadAccumulatorImmediate()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, 0x34);
            _cpu.Reset();
            Assert.AreEqual(0x34, _cpu.A);
        }

        [Test]
        public void CanLoadAccumulatorZeroPage()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_ZERO_PAGE);
            _map.Write(0x8001, 0x34);
            _map.Write(0x34, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.A);
        }

        [Test]
        public void CanLoadAccumulatorZeroPageX()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDX_IMMEDIATE);
            _map.Write(0x8001, 0x1);
            _map.Write(0x8002, (byte)CPU6502.OPCODE.LDA_ZERO_PAGE_X);
            _map.Write(0x8003, 0x34);
            _map.Write(0x35, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.A);
        }

        [Test]
        public void CanLoadAccumulatorAbsolute()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_ABSOLUTE);
            _map.WriteWord(0x8001, 0x2021);
            _map.Write(0x2021, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.A);
        }

        [Test]
        public void CanLoadAccumulatorAbsoluteX()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDX_IMMEDIATE);
            _map.Write(0x8001, 0x1);
            _map.Write(0x8002, (byte)CPU6502.OPCODE.LDA_ABSOLUTE_X);
            _map.WriteWord(0x8003, 0x2021);
            _map.Write(0x2022, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.A);
        }

        [Test]
        public void CanLoadAccumulatorIndirectX()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDX_IMMEDIATE);
            _map.Write(0x8001, 0x2);
            _map.Write(0x8002, (byte)CPU6502.OPCODE.LDA_INDIRECT_X);
            _map.Write(0x8003, 0x80);
            _map.WriteWord(0x82, 0x2021);
            _map.Write(0x2021, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.A);
        }

        [Test]
        public void CanLoadAccumulatorIndirectY()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDY_IMMEDIATE);
            _map.Write(0x8001, 0x2);
            _map.Write(0x8002, (byte)CPU6502.OPCODE.LDA_INDIRECT_Y);
            _map.Write(0x8003, 0x80);
            _map.WriteWord(0x82, 0x2021);
            _map.Write(0x2021, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.A);
        }

        [Test]
        public void CanLoadXImmediate()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDX_IMMEDIATE);
            _map.Write(0x8001, 0x34);
            _cpu.Reset();
            Assert.AreEqual(0x34, _cpu.X);
        }

        [Test]
        public void CanLoadXZeroPage()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDX_ZERO_PAGE);
            _map.Write(0x8001, 0x34);
            _map.Write(0x34, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.X);
        }

        [Test]
        public void CanLoadXZeroPageY()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDY_IMMEDIATE);
            _map.Write(0x8001, 0x1);
            _map.Write(0x8002, (byte)CPU6502.OPCODE.LDX_ZERO_PAGE_Y);
            _map.Write(0x8003, 0x34);
            _map.Write(0x35, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.X);
        }

        [Test]
        public void CanLoadXAbsolute()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDX_ABSOLUTE);
            _map.WriteWord(0x8001, 0x2021);
            _map.Write(0x2021, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.X);
        }

        [Test]
        public void CanLoadXAbsoluteY()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDY_IMMEDIATE);
            _map.Write(0x8001, 0x1);
            _map.Write(0x8002, (byte)CPU6502.OPCODE.LDX_ABSOLUTE_Y);
            _map.WriteWord(0x8003, 0x2021);
            _map.Write(0x2022, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.X);
        }

        [Test]
        public void CanLoadYImmediate()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDY_IMMEDIATE);
            _map.Write(0x8001, 0x34);
            _cpu.Reset();
            Assert.AreEqual(0x34, _cpu.Y);
        }

        [Test]
        public void CanLoadYZeroPage()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDY_ZERO_PAGE);
            _map.Write(0x8001, 0x34);
            _map.Write(0x34, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.Y);
        }

        [Test]
        public void CanLoadYZeroPageX()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDX_IMMEDIATE);
            _map.Write(0x8001, 0x1);
            _map.Write(0x8002, (byte)CPU6502.OPCODE.LDY_ZERO_PAGE_X);
            _map.Write(0x8003, 0x34);
            _map.Write(0x35, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.Y);
        }

        [Test]
        public void CanLoadYAbsolute()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDY_ABSOLUTE);
            _map.WriteWord(0x8001, 0x2021);
            _map.Write(0x2021, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.Y);
        }

        [Test]
        public void CanLoadYAbsoluteX()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDX_IMMEDIATE);
            _map.Write(0x8001, 0x1);
            _map.Write(0x8002, (byte)CPU6502.OPCODE.LDY_ABSOLUTE_X);
            _map.WriteWord(0x8003, 0x2021);
            _map.Write(0x2022, 0x56);
            _cpu.Reset();
            Assert.AreEqual(0x56, _cpu.Y);
        }

        [Test]
        public void CanNoOperation()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.NOP);
            _map.Write(0x8001, (byte)CPU6502.OPCODE.NOP);
            _cpu.Reset();
            Assert.AreEqual(0x8003, _cpu.PC); // Allow for the unhandled OpCode after the NOPs
        }

        [Test]
        public void CanBreak()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.NOP);
            _map.Write(0x8001, (byte)CPU6502.OPCODE.BRK);
            _map.Write(0x8002, (byte)CPU6502.OPCODE.NOP);
            _cpu.Reset();
            Assert.AreEqual(0x8002, _cpu.PC); 
        }

        [Test]
        public void CanStoreAccumulatorAbsolute()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.STA_ABSOLUTE);
            _map.WriteWord(0x8003, DISPLAY_BASE_ADDR);
            _map.Write(0x8005, (byte)CPU6502.OPCODE.LDY_ABSOLUTE);
            _map.WriteWord(0x8006, DISPLAY_BASE_ADDR);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.Y);
        }

        [Test]
        public void CanStoreAccumulatorAbsoluteX()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.LDX_IMMEDIATE);
            _map.Write(0x8003, 1);
            _map.Write(0x8004, (byte)CPU6502.OPCODE.STA_ABSOLUTE_X);
            _map.WriteWord(0x8005, DISPLAY_BASE_ADDR);
            _map.Write(0x8007, (byte)CPU6502.OPCODE.LDY_ABSOLUTE);
            _map.WriteWord(0x8008, DISPLAY_BASE_ADDR+1);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.Y);
        }
        [Test]
        public void CanStoreAccumulatorAbsoluteY()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.LDY_IMMEDIATE);
            _map.Write(0x8003, 1);
            _map.Write(0x8004, (byte)CPU6502.OPCODE.STA_ABSOLUTE_Y);
            _map.WriteWord(0x8005, DISPLAY_BASE_ADDR);
            _map.Write(0x8007, (byte)CPU6502.OPCODE.LDX_ABSOLUTE);
            _map.WriteWord(0x8008, DISPLAY_BASE_ADDR+1);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.X);
        }
        [Test]
        public void CanStoreAccumulatorIndirectX()
        {
            _map.WriteWord(0x2, DISPLAY_BASE_ADDR);
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.LDX_IMMEDIATE);
            _map.Write(0x8003, 2);
            _map.Write(0x8004, (byte)CPU6502.OPCODE.STA_INDIRECT_X);
            _map.Write(0x8005, 0x00);
            _map.Write(0x8006, (byte)CPU6502.OPCODE.LDY_ABSOLUTE);
            _map.WriteWord(0x8007, DISPLAY_BASE_ADDR);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.Y);
        }
        [Test]
        public void CanStoreAccumulatorIndirectY()
        {
            _map.WriteWord(0x2, DISPLAY_BASE_ADDR);
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.LDY_IMMEDIATE);
            _map.Write(0x8003, 2);
            _map.Write(0x8004, (byte)CPU6502.OPCODE.STA_INDIRECT_Y);
            _map.Write(0x8005, 0x00);
            _map.Write(0x8006, (byte)CPU6502.OPCODE.LDX_ABSOLUTE);
            _map.WriteWord(0x8007, DISPLAY_BASE_ADDR);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.X);
        }
        [Test]
        public void CanStoreXZeroPage()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDX_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.STX_ZERO_PAGE);
            _map.Write(0x8003, 0x20);
            _map.Write(0x8004, (byte)CPU6502.OPCODE.LDA_ZERO_PAGE);
            _map.Write(0x8005, 0x20);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.A);
        }
        [Test]
        public void CanStoreXZeroPageY()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDX_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.LDY_IMMEDIATE);
            _map.Write(0x8003, 0x10);
            _map.Write(0x8004, (byte)CPU6502.OPCODE.STX_ZERO_PAGE_Y);
            _map.Write(0x8005, 0x10);
            _map.Write(0x8006, (byte)CPU6502.OPCODE.LDA_ZERO_PAGE);
            _map.Write(0x8007, 0x20);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.A);
        }

        [Test]
        public void CanStoreXAbsolute()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDX_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.STX_ABSOLUTE);
            _map.WriteWord(0x8003, DISPLAY_BASE_ADDR);
            _map.Write(0x8005, (byte)CPU6502.OPCODE.LDA_ABSOLUTE);
            _map.WriteWord(0x8006, DISPLAY_BASE_ADDR);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.A);
        }
        [Test]
        public void CanStoreYZeroPage()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDY_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.STY_ZERO_PAGE);
            _map.Write(0x8003, 0x20);
            _map.Write(0x8004, (byte)CPU6502.OPCODE.LDA_ZERO_PAGE);
            _map.Write(0x8005, 0x20);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.A);
        }
        [Test]
        public void CanStoreYZeroPageX()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDY_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.LDX_IMMEDIATE);
            _map.Write(0x8003, 0x10);
            _map.Write(0x8004, (byte)CPU6502.OPCODE.STY_ZERO_PAGE_X);
            _map.Write(0x8005, 0x10);
            _map.Write(0x8006, (byte)CPU6502.OPCODE.LDA_ZERO_PAGE);
            _map.Write(0x8007, 0x20);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.A);
        }

        [Test]
        public void CanStoreYAbsolute()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDY_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.STY_ABSOLUTE);
            _map.WriteWord(0x8003, DISPLAY_BASE_ADDR);
            _map.Write(0x8005, (byte)CPU6502.OPCODE.LDA_ABSOLUTE);
            _map.WriteWord(0x8006, DISPLAY_BASE_ADDR);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.A);
        }

        [Test]
        public void CanTransferAccumulatorToX()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.TAX);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.X);
        }
        [Test]
        public void CanTransferAccumulatorToY()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.TAY);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.Y);
        }
        [Test]
        public void CanPushAccumulator()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.PHA);
            _map.Write(0x8003, (byte)CPU6502.OPCODE.LDY_ABSOLUTE);
            _map.WriteWord(0x8004, 0x1FF);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.Y);
        }

        [Test]
        public void CanPushProcessorStatus()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, 0x00);
            _map.Write(0x8002, (byte)CPU6502.OPCODE.PHP);
            _map.Write(0x8003, (byte)CPU6502.OPCODE.LDY_ABSOLUTE);
            _map.WriteWord(0x8004, 0x1FF);
            _cpu.Reset();
            Assert.AreEqual(0x02, _cpu.Y);
        }
        [Test]
        public void CanPullAccumulator()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.PHA);
            _map.Write(0x8003, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8004, 0x00);
            _map.Write(0x8005, (byte)CPU6502.OPCODE.PLA);
            _cpu.Reset();
            Assert.AreEqual((byte)'H', _cpu.A);
        }
        // [Test]
        // public void CanPullProcessorStatus()
        // {
        //     _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
        //     _map.Write(0x8001, 0x02);
        //     _map.Write(0x8002, (byte)CPU6502.OPCODE.PHA);
        //     _map.Write(0x8003, (byte)CPU6502.OPCODE.PLP);
        //     // Need logic to act of the processor status - not yet
        //     _map.Write(0x8004, 0x00);
        //     _map.Write(0x8005, (byte)CPU6502.OPCODE.PLA);
        //     _cpu.Reset();
        //     Assert.AreEqual((byte)'H', _cpu.A);
        // }
        
        [Test]
        public void CanJumpAbsolute()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.JMP_ABSOLUTE);
            _map.WriteWord(0x8003, 0x8006);
            _map.WriteWord(0x8005, (byte)CPU6502.OPCODE.BRK);
            _map.Write(0x8006, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8007, (byte)'e');
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }
        [Test]
        public void CanJumpIndirect()
        {
            _map.WriteWord(0x9000, 0x8006);
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.JMP_INDIRECT);
            _map.WriteWord(0x8003, 0x9000);
            _map.WriteWord(0x8005, (byte)CPU6502.OPCODE.BRK);
            _map.Write(0x8006, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8007, (byte)'e');
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }

        [Test]
        public void CanJumpToSubroutine()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.JSR);
            _map.WriteWord(0x8003, 0x8006);
            _map.Write(0x8005, (byte)CPU6502.OPCODE.BRK);
            _map.Write(0x8006, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8007, (byte)'e');
            _map.WriteWord(0x8008, (byte)CPU6502.OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
            Assert.AreEqual(0xFD, _cpu.SP);
        }

        [Test]
        public void CanReturnSubroutine()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.JSR);
            _map.WriteWord(0x8003, 0x8008);
            _map.Write(0x8005, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8006, (byte)'l');
            _map.Write(0x8007, (byte)CPU6502.OPCODE.BRK);
            _map.Write(0x8008, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8009, (byte)'e');
            _map.WriteWord(0x800A, (byte)CPU6502.OPCODE.RTS);
            _cpu.Reset();
            Assert.AreEqual((byte)'l', _cpu.A);
            Assert.AreEqual(0xFF, _cpu.SP);
        }

        [Test]
        public void CanSetCarry()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.SEC);
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C);
        }

        [Test]
        public void CanClearCarry()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.SEC);
            _map.Write(0x8001, (byte)CPU6502.OPCODE.CLC);
            _cpu.Reset();
            Assert.IsFalse(_cpu.P.C);
        }

        [Test]
        public void CanBranchForwardsOnCarryClear()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.CLC);
            _map.Write(0x8001, (byte)CPU6502.OPCODE.BCC);
            _map.Write(0x8002, 0x3);
            _map.Write(0x8003, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8004, (byte)'H');
            _map.Write(0x8005, (byte)CPU6502.OPCODE.BRK);
            _map.Write(0x8006, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8007, (byte)'e');
            _map.Write(0x8008, (byte)CPU6502.OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }

        [Test]
        public void CanBranchBackwardsOnCarryClear()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.JMP_ABSOLUTE);
            _map.WriteWord(0x8001, 0x8006);
            _map.Write(0x8003, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8004, (byte)'e');
            _map.Write(0x8005, (byte)CPU6502.OPCODE.BRK);
            _map.Write(0x8006, (byte)CPU6502.OPCODE.CLC);
            _map.Write(0x8007, (byte)CPU6502.OPCODE.BCC);
            _map.Write(0x8008, (byte)Convert.ToSByte(-6));
            _map.Write(0x8009, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x800A, (byte)'H');
            _map.Write(0x800B, (byte)CPU6502.OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }

        [Test]
        public void CanBranchForwardsOnCarrySet()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.SEC);
            _map.Write(0x8001, (byte)CPU6502.OPCODE.BCS);
            _map.Write(0x8002, 0x3);
            _map.Write(0x8003, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8004, (byte)'H');
            _map.Write(0x8005, (byte)CPU6502.OPCODE.BRK);
            _map.Write(0x8006, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8007, (byte)'e');
            _map.Write(0x8008, (byte)CPU6502.OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }

        [Test]
        public void CanBranchBackwardsOnCarrySet()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.JMP_ABSOLUTE);
            _map.WriteWord(0x8001, 0x8006);
            _map.Write(0x8003, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8004, (byte)'e');
            _map.Write(0x8005, (byte)CPU6502.OPCODE.BRK);
            _map.Write(0x8006, (byte)CPU6502.OPCODE.SEC);
            _map.Write(0x8007, (byte)CPU6502.OPCODE.BCS);
            _map.Write(0x8008, (byte)Convert.ToSByte(-6));
            _map.Write(0x8009, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x800A, (byte)'H');
            _map.Write(0x800B, (byte)CPU6502.OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual((byte)'e', _cpu.A);
        }

        [Test]
        public void CanCompareImmediateEqual()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.LDY_IMMEDIATE);
            _map.Write(0x8003, 0x00);
            _map.Write(0x8004, (byte)CPU6502.OPCODE.CMP_IMMEDIATE);
            _map.Write(0x8005, (byte)'H');
            _map.Write(0x8006, (byte)CPU6502.OPCODE.BRK);
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }
        [Test]
        public void CanCompareImmediateLess()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, 0x90);
            _map.Write(0x8002, (byte)CPU6502.OPCODE.LDY_IMMEDIATE);
            _map.Write(0x8003, 0x00);
            _map.Write(0x8004, (byte)CPU6502.OPCODE.CMP_IMMEDIATE);
            _map.Write(0x8005, 0xFF);
            _map.Write(0x8006, (byte)CPU6502.OPCODE.BRK);
            _cpu.Reset();
            Assert.IsTrue(!_cpu.P.C && !_cpu.P.Z && _cpu.P.N);
        }
        [Test]
        public void CanCompareImmediateGreater()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, 0x90);
            _map.Write(0x8002, (byte)CPU6502.OPCODE.LDY_IMMEDIATE);
            _map.Write(0x8003, 0x00);
            _map.Write(0x8004, (byte)CPU6502.OPCODE.CMP_IMMEDIATE);
            _map.Write(0x8005, 0x00);
            _map.Write(0x8006, (byte)CPU6502.OPCODE.BRK);
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && !_cpu.P.Z && _cpu.P.N);
        }
        [Test]
        public void CanCompareZeroPage()
        {
            _map.Write(0x10, (byte)'H');
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.LDY_IMMEDIATE);
            _map.Write(0x8003, 0x00);
            _map.Write(0x8004, (byte)CPU6502.OPCODE.CMP_ZERO_PAGE);
            _map.Write(0x8005, 0x10);
            _map.Write(0x8006, (byte)CPU6502.OPCODE.BRK);
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }
        [Test]
        public void CanCompareZeroPageX()
        {
            _map.Write(0x12, (byte)'H');
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.LDX_IMMEDIATE);
            _map.Write(0x8003, 0x02);
            _map.Write(0x8004, (byte)CPU6502.OPCODE.CMP_ZERO_PAGE_X);
            _map.Write(0x8005, 0x10);
            _map.Write(0x8006, (byte)CPU6502.OPCODE.BRK);
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }

        [Test]
        public void CanCompareAbsolute()
        {
            _map.Write(0x1010, (byte)'H');
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.LDY_IMMEDIATE);
            _map.Write(0x8003, 0x00);
            _map.Write(0x8004, (byte)CPU6502.OPCODE.CMP_ABSOLUTE);
            _map.WriteWord(0x8005, 0x1010);
            _map.Write(0x8007, (byte)CPU6502.OPCODE.BRK);
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }

        [Test]
        public void CanCompareAbsoluteX()
        {
            _map.Write(0x1010, (byte)'H');
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.LDX_IMMEDIATE);
            _map.Write(0x8003, 0x10);
            _map.Write(0x8004, (byte)CPU6502.OPCODE.CMP_ABSOLUTE_X);
            _map.WriteWord(0x8005, 0x1000);
            _map.Write(0x8007, (byte)CPU6502.OPCODE.BRK);
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }
        [Test]
        public void CanCompareIndirectX()
        {
            _map.Write(0x1010, (byte)'H');
            _map.WriteWord(0x0010, 0x1010);
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.LDX_IMMEDIATE);
            _map.Write(0x8003, 0x02);
            _map.Write(0x8004, (byte)CPU6502.OPCODE.CMP_INDIRECT_X);
            _map.Write(0x8005, 0x0E);
            _map.Write(0x8006, (byte)CPU6502.OPCODE.BRK);
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }
        [Test]
        public void CanCompareIndirectY()
        {
            _map.Write(0x1010, (byte)'H');
            _map.WriteWord(0x0010, 0x1010);
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.LDY_IMMEDIATE);
            _map.Write(0x8003, 0x02);
            _map.Write(0x8004, (byte)CPU6502.OPCODE.CMP_INDIRECT_Y);
            _map.Write(0x8005, 0x0E);
            _map.Write(0x8006, (byte)CPU6502.OPCODE.BRK);
            _cpu.Reset();
            Assert.IsTrue(_cpu.P.C && _cpu.P.Z && !_cpu.P.N);
        }

        [Test]
        public void CanBranchEquals()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.CMP_IMMEDIATE);
            _map.Write(0x8003, (byte)'H');
            _map.Write(0x8004, (byte)CPU6502.OPCODE.BEQ);
            _map.Write(0x8005, 0x03);
            _map.Write(0x8006, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8007, (byte)'N');
            _map.Write(0x8008, (byte)CPU6502.OPCODE.BRK);
            _map.Write(0x8009, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x800A, (byte)'Y');
            _map.Write(0x800B, (byte)CPU6502.OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual((byte)'Y', _cpu.A);
        }
        [Test]
        public void CanBranchNotEquals()
        {
            _map.Write(0x8000, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8001, (byte)'H');
            _map.Write(0x8002, (byte)CPU6502.OPCODE.CMP_IMMEDIATE);
            _map.Write(0x8003, (byte)'e');
            _map.Write(0x8004, (byte)CPU6502.OPCODE.BNE);
            _map.Write(0x8005, 0x03);
            _map.Write(0x8006, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x8007, (byte)'N');
            _map.Write(0x8008, (byte)CPU6502.OPCODE.BRK);
            _map.Write(0x8009, (byte)CPU6502.OPCODE.LDA_IMMEDIATE);
            _map.Write(0x800A, (byte)'Y');
            _map.Write(0x800B, (byte)CPU6502.OPCODE.BRK);
            _cpu.Reset();
            Assert.AreEqual((byte)'Y', _cpu.A);
        }
    }
}