using NUnit.Framework;
using _6502;
using HardwareCore;
using System;
using RemoteDisplayConnector;
using System.Threading.Tasks;
using FilePersistence;
using System.Collections.Generic;
using Memory;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Debugger;

namespace Tests
{
    public class FilePersistenceTests
    {
        private CPU6502 _cpu;
        private IMemoryMappedDisplay _display;
        private IAddressMap mem;
        private ILoaderPersistence _persistence;
        const ushort DISPLAY_BASE_ADDR = 0xF000;
        const ushort PROG_START = 0x8000;
        const ushort DISPLAY_SIZE = 0x400;  // 1kB

        private ServiceProvider _serviceProvider;

        public FilePersistenceTests()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
            ServiceProviderLocator.ServiceProvider = _serviceProvider;
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services
                 .AddLogging(cfg => cfg.AddConsole().AddDebug())
                 .AddScoped<IDebuggableCpu, CPU6502>()
                 .AddScoped<ILoaderLabelTable, LoaderLabelTable>()
                 .AddScoped<IAddressMap, AddressMap>()
                 .AddScoped<ILabelMap, LabelMap>()
                 .AddScoped<CpuHoldEvent,CpuHoldEvent>()
                 .AddScoped<ILogFormatter, DebugLogFormatter>()
                 .AddScoped<IMemoryMappedDisplay, MockMemoryMappedDisplay>()
                 .AddScoped<IParser, Parser>()
                 .AddTransient<ILoader, Loader>()
                 .AddSingleton<CancellationTokenWrapper>(new CancellationTokenWrapper(default(CancellationToken)));
        }

        [SetUp]
        public async Task Setup()
        {
            mem = _serviceProvider.GetService<IAddressMap>();
            mem.Install(new Ram(0x0000, 0x10000));

            _display = _serviceProvider.GetService<IMemoryMappedDisplay>();
            _display.Locate(DISPLAY_BASE_ADDR, DISPLAY_SIZE);
            mem.Install(_display);
            await _display.Initialise();
            _display.Clear();

            _cpu = (CPU6502)_serviceProvider.GetService<IDebuggableCpu>();
            _cpu.LogLevel = LogLevel.Trace;

            mem.WriteWord(_cpu.RESET_VECTOR, PROG_START);

            _persistence = new MemoryFilePersistence
            {
                WorkingDirectory = "~/6502Programs"
            };

            mem.Labels.Add("DISPLAY_CONTROL_ADDR", MemoryMappedDisplay.DISPLAY_CONTROL_BLOCK_ADDR);
            mem.Labels.Add("DISPLAY_BASE_ADDR", DISPLAY_BASE_ADDR);
            mem.Labels.Add("DISPLAY_SIZE", DISPLAY_SIZE);
            mem.Labels.Add("RESET_VECTOR", _cpu.RESET_VECTOR);
            mem.Labels.Add("IRQ_VECTOR", _cpu.IRQ_VECTOR);
            mem.Labels.Add("NMI_VECTOR", _cpu.NMI_VECTOR);

            mem.Labels.Push();

        }


        [Test]
        public void CanSaveTestProgram()
        {
            DoSave();
        }

        private void DoSave()
        {
            Assert.IsTrue(_display.Mode.Type == DisplayMode.RenderType.Text);

            var w = _display.Mode.Width;
            var h = _display.Mode.Height;
            var bpr = _display.Mode.BytesPerRow;

            using (var loader = mem.Load(PROG_START))
            {
                loader
                // Write column header
                .LDX_IMMEDIATE(0x00, "StartOfProgram")
                .JSR("ResetDigit")

                .LDA_ZERO_PAGE("CurrentDigit", "ColumnLoop")
                .STA_ABSOLUTE_X(DISPLAY_BASE_ADDR)
                .INX()
                .CPX_IMMEDIATE((byte)w)
                .BCS("DoneColumns")

                .JSR("IncrementDigit")
                .JMP_ABSOLUTE("ColumnLoop")

                // Write row labels
                .JSR("ResetDigit", "DoneColumns")
                .LDY_IMMEDIATE((byte)(h - 1))

                .CLC("IncrementRowAddress")
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
                .BRK("Finished")

                // Subroutine ResetDigit
                .LDA_IMMEDIATE((byte)'0', "ResetDigit")
                .STA_ZERO_PAGE("CurrentDigit")
                .RTS()

                // Subroutine IncrementDigit
                .LDA_ZERO_PAGE("CurrentDigit", "IncrementDigit")
                .CMP_IMMEDIATE((byte)'9')
                .BCS("ResetDigit") // Sneakily jump to the reset routine
                .INC_ZERO_PAGE("CurrentDigit")
                .RTS()
                .BRK("EndOfProgram")

                .Write(0x10, '0', "CurrentDigit")
                .WriteWord(0x12, DISPLAY_BASE_ADDR, "DisplayVector");
            }

            var start = mem.Labels.Resolve("StartOfProgram");
            var end = mem.Labels.Resolve("EndOfProgram");
            var length = (ushort)(end - start);

            _persistence.Save("TestProgram.bin", start, length, mem);
        }

        [Test]
        public void CanLoadAndRunTestProgram()
        {
            Assert.IsTrue(_display.Mode.Type == DisplayMode.RenderType.Text);

            DoSave();
            
            var w = _display.Mode.Width;
            var h = _display.Mode.Height;

            _persistence.Load("TestProgram.bin", mem);

            // Initialise Working Memory
            using (var loader = mem.Load())
            {
                loader
                .Write(0x10, '0')
                .WriteWord(0x12, DISPLAY_BASE_ADDR);
            }

            _cpu.Reset();
            Assert.AreEqual('0', mem.Read(DISPLAY_BASE_ADDR));
            var expected = (h + 9) % 10 + '0';
            Assert.AreEqual(expected, mem.Read((ushort)(DISPLAY_BASE_ADDR + (h - 1) * w)));
        }
    }
}