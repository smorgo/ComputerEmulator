using NUnit.Framework;
using _6502;
using HardwareCore;
using System;
using RemoteDisplayConnector;
using System.Threading.Tasks;
using FilePersistence;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    public class ComparisonContext
    {
        public ushort Cursor {get; set;}
    }

    public static class TestLoaderExtensions
    {
        public static bool IsConsistent(this IAddressMap memory, ComparisonContext context)
        {
            var half = (ushort)(context.Cursor / 2);
            var buffer1 = new StringBuilder();
            var buffer2 = new StringBuilder();
            
            for(var ix = 0; ix < half; ix++)
            {
                var b1 = memory.Read((ushort)ix);

                buffer1.Append($"{b1:X2} ");

                var b2 = memory.Read((ushort)(ix + half));

                buffer2.Append($"{b2:X2} ");
            }

            if(buffer1.ToString() == buffer2.ToString())
            {
                return true;
            }

            Console.WriteLine("Comparison:");
            Console.WriteLine(buffer1.ToString());
            Console.WriteLine(buffer2.ToString());

            return false;
        }

        public static void Stop(this Loader loader, out ComparisonContext context)
        {
            context = new ComparisonContext { Cursor = loader.Cursor };
        }
    }

    public class LoaderExtensionsTests
    {
        private AddressMap mem;
        private const char TEST_CHAR = 'H';
        private const byte TEST_BYTE = (byte)TEST_CHAR;
        private const ushort TEST_ADDR = 0x1234;
        private const byte ZP_ADDR = 0x34;

        [SetUp]
        public void Setup()
        {
            mem = new AddressMap();
            mem.Install(new Ram(0x0000, 0x4000));
            mem.Labels = new LabelTable();
            mem.Labels.Add("TEST_ADDR", TEST_ADDR);
            mem.Labels.Add("ZP_ADDR", ZP_ADDR);
            mem.Labels.Push();
        }

        [Test]
        public void ADC_IMMEDIATE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ADC_IMMEDIATE(TEST_BYTE)
                    .ADC_IMMEDIATE(TEST_CHAR)
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void ADC_IMMEDIATE_Label_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ADC_IMMEDIATE(0x20)
                    .ADC_IMMEDIATE("lbl:LO")
                .Label(0x1020, "lbl")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void ADC_ABSOLUTE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ADC_ABSOLUTE(TEST_ADDR)
                    .ADC_ABSOLUTE("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void ADC_ABSOLUTE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ADC_ABSOLUTE_X(TEST_ADDR)
                    .ADC_ABSOLUTE_X("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void ADC_ABSOLUTE_Y_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ADC_ABSOLUTE_Y(TEST_ADDR)
                    .ADC_ABSOLUTE_Y("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }


        [Test]
        public void ADC_ZERO_PAGE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ADC_ZERO_PAGE_X(ZP_ADDR)
                    .ADC_ZERO_PAGE_X("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void AND_ZERO_PAGE_Label_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .AND_ZERO_PAGE(ZP_ADDR)
                    .AND_ZERO_PAGE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void AND_ZERO_PAGE_X_Label_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .AND_ZERO_PAGE_X(ZP_ADDR)
                    .AND_ZERO_PAGE_X("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }


        [Test]
        public void AND_IMMEDIATE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .AND_IMMEDIATE(TEST_BYTE)
                    .AND_IMMEDIATE(TEST_CHAR)
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void AND_IMMEDIATE_Label_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .AND_IMMEDIATE(0x34)
                    .AND_IMMEDIATE("TEST_ADDR:LO")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void AND_INDIRECTX_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .AND_INDIRECT_X(ZP_ADDR)
                    .AND_INDIRECT_X("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void AND_ABSOLUTE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .AND_ABSOLUTE(TEST_ADDR)
                    .AND_ABSOLUTE("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void AND_ABSOLUTE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .AND_ABSOLUTE_X(TEST_ADDR)
                    .AND_ABSOLUTE_X("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void AND_ABSOLUTE_Y_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .AND_ABSOLUTE_Y(TEST_ADDR)
                    .AND_ABSOLUTE_Y("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }


        [Test]
        public void AND_INDIRECTY_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .AND_INDIRECT_Y(ZP_ADDR)
                    .AND_INDIRECT_Y("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void ADC_INDIRECT_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ADC_INDIRECT_X(ZP_ADDR)
                    .ADC_INDIRECT_X("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void ADC_INDIRECT_Y_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ADC_INDIRECT_Y(ZP_ADDR)
                    .ADC_INDIRECT_Y("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void ASL_ABSOLUTE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ASL_ABSOLUTE(TEST_ADDR)
                    .ASL_ABSOLUTE("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void ASL_ZERO_PAGE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ASL_ZERO_PAGE(ZP_ADDR)
                    .ASL_ZERO_PAGE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void ASL_ZERO_PAGE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ASL_ZERO_PAGE_X(ZP_ADDR)
                    .ASL_ZERO_PAGE_X("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void BIT_ZERO_PAGE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .BIT_ZERO_PAGE(ZP_ADDR)
                    .BIT_ZERO_PAGE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void CMP_ZERO_PAGE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .CMP_ZERO_PAGE(ZP_ADDR)
                    .CMP_ZERO_PAGE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void CMP_ZERO_PAGE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .CMP_ZERO_PAGE_X(ZP_ADDR)
                    .CMP_ZERO_PAGE_X("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void CMP_IMMEDIATE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .CMP_IMMEDIATE(ZP_ADDR)
                    .CMP_IMMEDIATE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void CMP_INDIRECT_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .CMP_INDIRECT_X(ZP_ADDR)
                    .CMP_INDIRECT_X("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void CMP_INDIRECT_Y_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .CMP_INDIRECT_Y(ZP_ADDR)
                    .CMP_INDIRECT_Y("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void CPX_ZERO_PAGE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .CPX_ZERO_PAGE(ZP_ADDR)
                    .CPX_ZERO_PAGE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void CPX_IMMEDIATE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .CPX_IMMEDIATE(ZP_ADDR)
                    .CPX_IMMEDIATE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void CPY_ZERO_PAGE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .CPY_ZERO_PAGE(ZP_ADDR)
                    .CPY_ZERO_PAGE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void CPY_IMMEDIATE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .CPY_IMMEDIATE(ZP_ADDR)
                    .CPY_IMMEDIATE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void BIT_ABSOLUTE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .BIT_ABSOLUTE(TEST_ADDR)
                    .BIT_ABSOLUTE("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }


        [Test]
        public void ASL_ABSOLUTE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ASL_ABSOLUTE_X(TEST_ADDR)
                    .ASL_ABSOLUTE_X("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void BCC_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .BCC(0xFC, "rel")
                    .BCC("rel")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void BCS_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .BCS(0xFC, "rel")
                    .BCS("rel")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void BEQ_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .BEQ(0xFC, "rel")
                    .BEQ("rel")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void BMI_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .BMI(0xFC, "rel")
                    .BMI("rel")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void BNE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .BNE(0xFC, "rel")
                    .BNE("rel")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void BPL_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .BPL(0xFC, "rel")
                    .BPL("rel")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void BVC_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .BVC(0xFC, "rel")
                    .BVC("rel")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void BVS_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .BVS(0xFC, "rel")
                    .BVS("rel")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void CMP_ABSOLUTE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .CMP_ABSOLUTE(TEST_ADDR)
                    .CMP_ABSOLUTE("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void CMP_ABSOLUTE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .CMP_ABSOLUTE_X(TEST_ADDR)
                    .CMP_ABSOLUTE_X("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void CMP_ABSOLUTE_Y_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .CMP_ABSOLUTE_Y(TEST_ADDR)
                    .CMP_ABSOLUTE_Y("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void CPX_ABSOLUTE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .CPX_ABSOLUTE(TEST_ADDR)
                    .CPX_ABSOLUTE("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void CPY_ABSOLUTE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .CPY_ABSOLUTE(TEST_ADDR)
                    .CPY_ABSOLUTE("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void DEC_ABSOLUTE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .DEC_ABSOLUTE(TEST_ADDR)
                    .DEC_ABSOLUTE("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void DEC_ZERO_PAGE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .DEC_ZERO_PAGE(ZP_ADDR)
                    .DEC_ZERO_PAGE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void DEC_ZERO_PAGE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .DEC_ZERO_PAGE_X(ZP_ADDR)
                    .DEC_ZERO_PAGE_X("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void DEC_ABSOLUTE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .DEC_ABSOLUTE_X(TEST_ADDR)
                    .DEC_ABSOLUTE_X("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void EOR_IMMEDIATE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .EOR_IMMEDIATE(TEST_BYTE)
                    .EOR_IMMEDIATE(TEST_CHAR)
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void EOR_IMMEDIATE_Label_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .EOR_IMMEDIATE(0x34)
                    .EOR_IMMEDIATE("TEST_ADDR:LO")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void EOR_ZERO_PAGE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .EOR_ZERO_PAGE(ZP_ADDR)
                    .EOR_ZERO_PAGE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void EOR_INDIRECT_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .EOR_INDIRECT_X(ZP_ADDR)
                    .EOR_INDIRECT_X("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void EOR_INDIRECT_Y_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .EOR_INDIRECT_Y(ZP_ADDR)
                    .EOR_INDIRECT_Y("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void EOR_ZERO_PAGE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .EOR_ZERO_PAGE_X(ZP_ADDR)
                    .EOR_ZERO_PAGE_X("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void EOR_ABSOLUTE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .EOR_ABSOLUTE(TEST_ADDR)
                    .EOR_ABSOLUTE("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void EOR_ABSOLUTE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .EOR_ABSOLUTE_X(TEST_ADDR)
                    .EOR_ABSOLUTE_X("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void EOR_ABSOLUTE_Y_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .EOR_ABSOLUTE_Y(TEST_ADDR)
                    .EOR_ABSOLUTE_Y("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void INC_ABSOLUTE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .INC_ABSOLUTE(TEST_ADDR)
                    .INC_ABSOLUTE("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void INC_ZERO_PAGE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .INC_ZERO_PAGE(ZP_ADDR)
                    .INC_ZERO_PAGE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void INC_ZERO_PAGE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .INC_ZERO_PAGE_X(ZP_ADDR)
                    .INC_ZERO_PAGE_X("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void INC_ABSOLUTE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .INC_ABSOLUTE_X(TEST_ADDR)
                    .INC_ABSOLUTE_X("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void JMP_ABSOLUTE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .JMP_ABSOLUTE(TEST_ADDR)
                    .JMP_ABSOLUTE("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }


        [Test]
        public void JMP_INDIRECT_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .JMP_INDIRECT(TEST_ADDR)
                    .JMP_INDIRECT("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void JSR_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .JSR(TEST_ADDR)
                    .JSR("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void LSR_ABSOLUTE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .LSR_ABSOLUTE(TEST_ADDR)
                    .LSR_ABSOLUTE("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void LSR_ZERO_PAGE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .LSR_ZERO_PAGE(ZP_ADDR)
                    .LSR_ZERO_PAGE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void LSR_ZERO_PAGE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .LSR_ZERO_PAGE_X(ZP_ADDR)
                    .LSR_ZERO_PAGE_X("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void LSR_ABSOLUTE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .LSR_ABSOLUTE_X(TEST_ADDR)
                    .LSR_ABSOLUTE_X("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void ORA_IMMEDIATE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ORA_IMMEDIATE(TEST_BYTE)
                    .ORA_IMMEDIATE(TEST_CHAR)
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void ORA_IMMEDIATE_Label_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ORA_IMMEDIATE(0x12)
                    .ORA_IMMEDIATE("TEST_ADDR:HI")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void ORA_ZERO_PAGE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ORA_ZERO_PAGE(ZP_ADDR)
                    .ORA_ZERO_PAGE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void ORA_ZERO_PAGE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ORA_ZERO_PAGE_X(ZP_ADDR)
                    .ORA_ZERO_PAGE_X("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void ORA_ABSOLUTE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ORA_ABSOLUTE(TEST_ADDR)
                    .ORA_ABSOLUTE("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void ORA_ABSOLUTE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ORA_ABSOLUTE_X(TEST_ADDR)
                    .ORA_ABSOLUTE_X("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void ORA_ABSOLUTE_Y_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ORA_ABSOLUTE_Y(TEST_ADDR)
                    .ORA_ABSOLUTE_Y("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void ORA_INDIRECT_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ORA_INDIRECT_X(ZP_ADDR)
                    .ORA_INDIRECT_X("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void ORA_INDIRECT_Y_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ORA_INDIRECT_Y(ZP_ADDR)
                    .ORA_INDIRECT_Y("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void ROL_ABSOLUTE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ROL_ABSOLUTE(TEST_ADDR)
                    .ROL_ABSOLUTE("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void ROL_ABSOLUTE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ROL_ABSOLUTE_X(TEST_ADDR)
                    .ROL_ABSOLUTE_X("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void ROL_ZERO_PAGE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ROL_ZERO_PAGE(ZP_ADDR)
                    .ROL_ZERO_PAGE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void ROL_ZERO_PAGE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ROL_ZERO_PAGE_X(ZP_ADDR)
                    .ROL_ZERO_PAGE_X("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void ROR_ZERO_PAGE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ROR_ZERO_PAGE(ZP_ADDR)
                    .ROR_ZERO_PAGE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void ROR_ZERO_PAGE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ROR_ZERO_PAGE_X(ZP_ADDR)
                    .ROR_ZERO_PAGE_X("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void ROR_ABSOLUTE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ROR_ABSOLUTE(TEST_ADDR)
                    .ROR_ABSOLUTE("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void ROR_ABSOLUTE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .ROR_ABSOLUTE_X(TEST_ADDR)
                    .ROR_ABSOLUTE_X("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void SBC_IMMEDIATE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .SBC_IMMEDIATE(TEST_BYTE)
                    .SBC_IMMEDIATE(TEST_CHAR)
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void SBC_IMMEDIATE_Label_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .SBC_IMMEDIATE(0x12)
                    .SBC_IMMEDIATE("TEST_ADDR:HI")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void SBC_ZERO_PAGE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .SBC_ZERO_PAGE(ZP_ADDR)
                    .SBC_ZERO_PAGE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void SBC_ZERO_PAGE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .SBC_ZERO_PAGE_X(ZP_ADDR)
                    .SBC_ZERO_PAGE_X("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void SBC_ABSOLUTE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .SBC_ABSOLUTE(TEST_ADDR)
                    .SBC_ABSOLUTE("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void SBC_ABSOLUTE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .SBC_ABSOLUTE_X(TEST_ADDR)
                    .SBC_ABSOLUTE_X("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void SBC_ABSOLUTE_Y_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .SBC_ABSOLUTE_Y(TEST_ADDR)
                    .SBC_ABSOLUTE_Y("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void SBC_INDIRECT_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .SBC_INDIRECT_X(ZP_ADDR)
                    .SBC_INDIRECT_X("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void SBC_INDIRECT_Y_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .SBC_INDIRECT_Y(ZP_ADDR)
                    .SBC_INDIRECT_Y("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

        [Test]
        public void STA_ABSOLUTE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .STA_ABSOLUTE(TEST_ADDR)
                    .STA_ABSOLUTE("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void STA_ABSOLUTE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .STA_ABSOLUTE_X(TEST_ADDR)
                    .STA_ABSOLUTE_X("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void STA_ABSOLUTE_Y_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .STA_ABSOLUTE_Y(TEST_ADDR)
                    .STA_ABSOLUTE_Y("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void STA_INDIRECT_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .STA_INDIRECT_X(ZP_ADDR)
                    .STA_INDIRECT_X("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void STA_INDIRECT_Y_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .STA_INDIRECT_Y(ZP_ADDR)
                    .STA_INDIRECT_Y("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void STA_ZERO_PAGE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .STA_ZERO_PAGE(ZP_ADDR)
                    .STA_ZERO_PAGE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void STA_ZERO_PAGE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .STA_ZERO_PAGE_X(ZP_ADDR)
                    .STA_ZERO_PAGE_X("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void STX_ABSOLUTE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .STX_ABSOLUTE(TEST_ADDR)
                    .STX_ABSOLUTE("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void STX_ZERO_PAGE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .STX_ZERO_PAGE(ZP_ADDR)
                    .STX_ZERO_PAGE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void STX_ZERO_PAGE_Y_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .STX_ZERO_PAGE_Y(ZP_ADDR)
                    .STX_ZERO_PAGE_Y("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void STY_ABSOLUTE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .STY_ABSOLUTE(TEST_ADDR)
                    .STY_ABSOLUTE("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void STY_ZERO_PAGE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .STY_ZERO_PAGE(ZP_ADDR)
                    .STY_ZERO_PAGE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void STY_ZERO_PAGE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .STY_ZERO_PAGE_X(ZP_ADDR)
                    .STY_ZERO_PAGE_X("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void LDA_IMMEDIATE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .LDA_IMMEDIATE(ZP_ADDR)
                    .LDA_IMMEDIATE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void LDA_ZERO_PAGE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .LDA_ZERO_PAGE(ZP_ADDR)
                    .LDA_ZERO_PAGE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void LDA_ZERO_PAGE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .LDA_ZERO_PAGE_X(ZP_ADDR)
                    .LDA_ZERO_PAGE_X("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void LDA_INDIRECT_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .LDA_INDIRECT_X(ZP_ADDR)
                    .LDA_INDIRECT_X("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void LDA_INDIRECT_Y_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .LDA_INDIRECT_Y(ZP_ADDR)
                    .LDA_INDIRECT_Y("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void LDX_IMMEDIATE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .LDX_IMMEDIATE(ZP_ADDR)
                    .LDX_IMMEDIATE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void LDX_ZERO_PAGE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .LDX_ZERO_PAGE(ZP_ADDR)
                    .LDX_ZERO_PAGE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void LDX_ZERO_PAGE_Y_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .LDX_ZERO_PAGE_Y(ZP_ADDR)
                    .LDX_ZERO_PAGE_Y("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void LDX_ABSOLUTE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .LDX_ABSOLUTE(TEST_ADDR)
                    .LDX_ABSOLUTE("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void LDX_ABSOLUTE_Y_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .LDX_ABSOLUTE_Y(ZP_ADDR)
                    .LDX_ABSOLUTE_Y("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void LDY_IMMEDIATE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .LDY_IMMEDIATE(ZP_ADDR)
                    .LDY_IMMEDIATE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void LDY_ZERO_PAGE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .LDY_ZERO_PAGE(ZP_ADDR)
                    .LDY_ZERO_PAGE("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void LDY_ZERO_PAGE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .LDY_ZERO_PAGE_X(ZP_ADDR)
                    .LDY_ZERO_PAGE_X("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void LDY_ABSOLUTE_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .LDY_ABSOLUTE(TEST_ADDR)
                    .LDY_ABSOLUTE("TEST_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }
        [Test]
        public void LDY_ABSOLUTE_X_LoaderIsConsistent()
        {
            ComparisonContext context;

            using(var _ = mem.Load())
            {
                _
                    .LDY_ABSOLUTE_X(ZP_ADDR)
                    .LDY_ABSOLUTE_X("ZP_ADDR")
                    .Stop(out context);
            }

            Assert.IsTrue(mem.IsConsistent(context));
        }

    }
}
