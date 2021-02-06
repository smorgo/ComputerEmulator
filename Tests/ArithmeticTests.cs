using NUnit.Framework;
using _6502;
using HardwareCore;
using System;
using RemoteDisplayConnector;
using System.Threading.Tasks;

namespace Tests
{
    public class ArithmeticTests
    {
        private CPU6502 _cpu;
        private AddressMap mem;
        const ushort PROG_START = 0x8000;
        const ushort MULTIPLY_RESULT = 0x0052;
        const ushort MULTIPLY_FACTOR_2 = 0x0054;
        const byte NoCarryNoOverflow = 0x00;
        const byte CarryNoOverflow = 0x01;
        const byte NoCarryOverflow = 0x40;
        const byte CarryOverflow = 0x41;

        [SetUp]
        public async Task Setup()
        {
            mem = new AddressMap();
            mem.Install(new Ram(0x0000, 0x10000));

            await mem.Initialise();

            _cpu = new CPU6502(mem);
            _cpu.DebugLevel = DebugLevel.Verbose;
            mem.WriteWord(_cpu.RESET_VECTOR, PROG_START);

            mem.Labels = new LabelTable();
            mem.Labels.Add("RESET_VECTOR", _cpu.RESET_VECTOR);
            mem.Labels.Add("IRQ_VECTOR", _cpu.IRQ_VECTOR);
            mem.Labels.Add("NMI_VECTOR", _cpu.NMI_VECTOR);
            mem.Labels.Add("MULTIPLY_RESULT", MULTIPLY_RESULT);
            mem.Labels.Add("MULTIPLY_FACTOR_2", MULTIPLY_FACTOR_2);
        }

        [Test]
        public void CanMultiplyEverything()
        {
            _cpu.DebugLevel = DebugLevel.Warnings;

            var failed = false;
            for(var x = 0; x < 256; x++)
            {
                for(var y = 0; y < 256; y++)
                {
        
                    mem.Labels.Push();

                    var result = Multiply((byte)x, (byte)y);

                    mem.Labels.Pop();

                    var expected = (ushort)(x * y);

                    if(result != expected)
                    {
                        failed = true;
                        Console.WriteLine($"{x} * {y} returned {result}. Should be {expected}");
                    }
                }
            }

            Assert.IsFalse(failed);
        }

        [TestCase((byte)1,(byte)2,(ushort)2)]
        [TestCase((byte)2,(byte)7,(ushort)14)]
        public void CanMultiply(byte x, byte y, ushort expected)
        {
            Assert.AreEqual(expected, Multiply(x,y));
        }

        private ushort Multiply(byte x, byte y)
        {
            using (var _ = mem.Load(PROG_START))
            {
                _
                    .LDX_IMMEDIATE(x)
                    .LDY_IMMEDIATE(y)
                    .JSR("MultiplyXbyY")
                    .BRK()

                    .Label("MultiplyXbyY")
                        .PHA()
                        .TXA()
                        .PHA()
                        .TYA()
                        .PHA()
                        .PHP()
                        .TXA()
                        .LSR_ACCUMULATOR()
                        .STA_ABSOLUTE("MULTIPLY_RESULT")
                        .STY_ABSOLUTE("MULTIPLY_FACTOR_2")
                        .LDA_IMMEDIATE(0x00)
                        .LDY_IMMEDIATE(0x08)
                    .Label("MultiplyLoop")
                        .BCC("MultiplyNoAdd")
                        .CLC()
                        .ADC_ABSOLUTE("MULTIPLY_FACTOR_2")
                    .Label("MultiplyNoAdd")
                        .ROR_ACCUMULATOR()
                        .ROR_ABSOLUTE("MULTIPLY_RESULT")
                        .DEY()
                        .BNE("MultiplyLoop")
                        .STA_ABSOLUTE("MULTIPLY_RESULT+1")
                        .PLP()
                        .PLA()
                        .TAY()
                        .PLA()
                        .TAX()
                        .PLA()
                        .RTS();
            }
            _cpu.Reset();
            
            return mem.ReadWord(MULTIPLY_RESULT);
        }

    }
}