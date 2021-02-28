using NUnit.Framework;
using _6502;
using HardwareCore;
using System;
using RemoteDisplayConnector;
using System.Threading.Tasks;
using Memory;
using System.Threading;
using Microsoft.Extensions.Logging;

using Microsoft.Extensions.DependencyInjection;
using Debugger;
using System.Collections.Generic;
using KeyboardConnector;

namespace Tests
{
    public class ComplexCpuTests
    {
        private CPU6502 _cpu;
        private MockCpuHoldEvent _cpuHoldEvent;
        private MockCpuStepEvent _cpuStepEvent;

        private IMemoryMappedDisplay _display;
        private IAddressMap mem;
        const ushort DISPLAY_BASE_ADDR = 0xF000;
        const ushort PROG_START = 0x8000;
        const ushort DISPLAY_SIZE = 0x400;  // 1kB
        const byte NoCarryNoOverflow = 0x00;
        const byte CarryNoOverflow = 0x01;
        const byte NoCarryOverflow = 0x40;
        const byte CarryOverflow = 0x41;
        private ServiceProvider _serviceProvider;
        private UnitTestLogger<CPU6502> _logger;
        private CancellationTokenWrapper _cancellationTokenWrapper;
        public ComplexCpuTests()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
            ServiceProviderLocator.ServiceProvider = _serviceProvider;
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services
                 .AddScoped(typeof(ILogger<CPU6502>), typeof(UnitTestLogger<CPU6502>))
                 .AddScoped(typeof(ILogger<Loader>), typeof(UnitTestLogger<Loader>))
                 .AddSingleton<ILoaderLabelTable>(new LoaderLabelTable())
                 .AddTransient<IDebuggableCpu, CPU6502>()
                 .AddScoped<IAddressMap, AddressMap>()
                 .AddScoped<IMemoryMappedDisplay, MockMemoryMappedDisplay>()
                 .AddScoped<IRemoteDisplayConnection, NoRemoteDisplayConnection>()
                 .AddScoped<IRegisterTracker, DebugRegisterTracker>()
                 .AddTransient<ILoader, Loader>()
                 .AddScoped<ICpuHoldEvent,MockCpuHoldEvent>()
                 .AddScoped<ICpuStepEvent,MockCpuStepEvent>()
                 .AddScoped<CancellationTokenWrapper, CancellationTokenWrapper>();
        }
        [SetUp]
        public void Setup()
        {
            _logger = (UnitTestLogger<CPU6502>)_serviceProvider.GetService<ILogger<CPU6502>>();
            _logger.GetOutput(); // Clear any buffered output

            mem = _serviceProvider.GetService<IAddressMap>();
            mem.Install(new Ram(0x0000, 0x10000));

            _display = _serviceProvider.GetService<IMemoryMappedDisplay>();
            _display.Locate(DISPLAY_BASE_ADDR, DISPLAY_SIZE);
            mem.Install(_display);
            AsyncUtil.RunSync(mem.Initialise);
            _display.Clear();
            _cpu = (CPU6502)_serviceProvider.GetService<IDebuggableCpu>();
            _cpu.LogLevel = LogLevel.Trace;
            mem.WriteWord(_cpu.RESET_VECTOR, PROG_START);
            mem.Labels.Clear();
            mem.Labels.Add("DISPLAY_CONTROL_ADDR", MemoryMappedDisplay.DISPLAY_CONTROL_BLOCK_ADDR);
            mem.Labels.Add("DISPLAY_BASE_ADDR", DISPLAY_BASE_ADDR);
            mem.Labels.Add("DISPLAY_SIZE", DISPLAY_SIZE);
            mem.Labels.Add("RESET_VECTOR", _cpu.RESET_VECTOR);
            mem.Labels.Add("IRQ_VECTOR", _cpu.IRQ_VECTOR);
            mem.Labels.Add("NMI_VECTOR", _cpu.NMI_VECTOR);
            _cpuHoldEvent = (MockCpuHoldEvent)_serviceProvider.GetService<ICpuHoldEvent>();
            _cpuStepEvent = (MockCpuStepEvent)_serviceProvider.GetService<ICpuStepEvent>();
            _cpuHoldEvent.Init();
            _cpuStepEvent.Init();
            _cancellationTokenWrapper = _serviceProvider.GetService<CancellationTokenWrapper>();
            _cancellationTokenWrapper.Reset();
        }

        [Test]
        public void GoTriggersEvents()
        {
            _cpu.Go();
            Assert.IsTrue(_cpuHoldEvent.IsSet);
            Assert.IsTrue(_cpuStepEvent.IsSet);
            Assert.AreEqual(1, _cpuHoldEvent.SetCount);
            Assert.AreEqual(1, _cpuStepEvent.SetCount);
            Assert.AreEqual(0, _cpuHoldEvent.ResetCount);
            Assert.AreEqual(0, _cpuStepEvent.ResetCount);
        }

        [Test]
        public void StepTriggersEvents()
        {
            _cpuHoldEvent.Set();
            _cpu.Step();
            Assert.IsTrue(_cpuHoldEvent.IsSet);
            Assert.IsFalse(_cpuStepEvent.IsSet);
            Assert.AreEqual(2, _cpuHoldEvent.SetCount);
            Assert.AreEqual(1, _cpuStepEvent.SetCount);
            Assert.AreEqual(1, _cpuHoldEvent.ResetCount);
            Assert.AreEqual(1, _cpuStepEvent.ResetCount);
        }

        [Test]
        public void StopTriggersEvents()
        {
            _cpuHoldEvent.Set();
            _cpuStepEvent.Set();
            _cpu.Stop();
            Assert.IsFalse(_cpuHoldEvent.IsSet);
            Assert.IsTrue(_cpuStepEvent.IsSet);
            Assert.AreEqual(1, _cpuHoldEvent.SetCount);
            Assert.AreEqual(2, _cpuStepEvent.SetCount);
            Assert.AreEqual(1, _cpuHoldEvent.ResetCount);
            Assert.AreEqual(0, _cpuStepEvent.ResetCount);
        }

        [Test]
        public void CanCancelCpu()
        {
            using(var _ = mem.Load(0x8000))
            {
                _
                .NOP("loop")
                .JMP_ABSOLUTE("loop");
            }

            _cancellationTokenWrapper.Source.CancelAfter(TimeSpan.FromSeconds(5));
            
            _cpu.Reset(TimeSpan.FromSeconds(10));

            var output = _logger.GetOutput();
            Assert.IsTrue(output.Contains("Task cancelled"));
        }

        [Test]
        public void CanTimeoutCpu()
        {
            using(var _ = mem.Load(0x8000))
            {
                _
                .NOP("loop")
                .JMP_ABSOLUTE("loop");
            }

            _cancellationTokenWrapper.Source.CancelAfter(TimeSpan.FromSeconds(10));
            
            _cpu.Reset(TimeSpan.FromSeconds(5));

            var output = _logger.GetOutput();
            Assert.IsTrue(output.Contains("Terminated after maximum duration"));
        }

        [Test]
        public void CanTimeoutStepEvent()
        {
            var e = new CpuStepEvent();
            e.Reset();
            Assert.IsFalse(e.WaitOne(TimeSpan.FromSeconds(2)));
        }

        [Test]
        public void CanTimeoutHoldEvent()
        {
            var e = new CpuHoldEvent();
            e.Reset();
            Assert.IsFalse(e.WaitOne(TimeSpan.FromSeconds(2)));
        }

        [TestCase(256,64)]
        [TestCase(2,8)]
        [TestCase(4,16)]
        [TestCase(8,24)]
        [TestCase(16,32)]
        public void DisplayModeCalculatesBytesPerCharacterForGraphicsMode(int colours, int expected)
        {
            var mode = new DisplayMode(0, DisplayMode.RenderType.Bitmapped, colours, 640, 480);
            Assert.AreEqual(expected, mode.BytesPerCharacter);
        }

        [Test]
        public void PeekEmptyFifoReturnsFalse()
        {
            var fifo = new FifoBuffer<KeyboardEvent>(5);
            KeyboardEvent result;
            Assert.IsFalse(fifo.Peek(out result));

        }
        [Test]
        public void PeekNonEmptyFifoReturnsTrue()
        {
            var fifo = new FifoBuffer<int>(5);
            fifo.Write(99);
            int result;
            Assert.IsTrue(fifo.Peek(out result));
            Assert.AreEqual(99, result);
        }
        [Test]
        public void ReadEmptyFifoReturnsFalse()
        {
            var fifo = new FifoBuffer<KeyboardEvent>(5);
            KeyboardEvent result;
            Assert.IsFalse(fifo.Read(out result));
            Assert.IsNull(result);
        }
        [Test]
        public void ReadOneValueEmptiesFifo()
        {
            var fifo = new FifoBuffer<int>(5);
            fifo.Write(99);
            int result;
            Assert.IsTrue(fifo.Read(out result));
            Assert.IsFalse(fifo.Read(out result));
        }
        [Test]
        public void EmptyFifoReturnIsEmpty()
        {
            var fifo = new FifoBuffer<KeyboardEvent>(5);
            Assert.IsTrue(fifo.IsEmpty());
        }   
        [Test]
        public void NonEmptyFifoDoesNotReturnIsEmpty()
        {
            var fifo = new FifoBuffer<int>(5);
            fifo.Write(99);
            Assert.IsFalse(fifo.IsEmpty());
        }   
        [Test]
        public void FullFifoReturnIsFull()
        {
            var fifo = new FifoBuffer<int>(5);
            Assert.IsTrue(fifo.Write(1));
            Assert.IsTrue(fifo.Write(2));
            Assert.IsTrue(fifo.Write(3));
            Assert.IsTrue(fifo.Write(4));
            Assert.IsTrue(fifo.Write(5));
            Assert.IsTrue(fifo.IsFull());
        }   
        [Test]
        public void CantOverflowFifo()
        {
            var fifo = new FifoBuffer<int>(5);
            Assert.IsTrue(fifo.Write(1));
            Assert.IsTrue(fifo.Write(2));
            Assert.IsTrue(fifo.Write(3));
            Assert.IsTrue(fifo.Write(4));
            Assert.IsTrue(fifo.Write(5));
            Assert.IsFalse(fifo.Write(6));
        }   
        [Test]
        public void NonFullFifoDoesntReturnIsFull()
        {
            var fifo = new FifoBuffer<int>(5);
            Assert.IsTrue(fifo.Write(1));
            Assert.IsTrue(fifo.Write(2));
            Assert.IsTrue(fifo.Write(3));
            Assert.IsTrue(fifo.Write(4));
            Assert.IsFalse(fifo.IsFull());
        }   

        [Test]
        public void CanTrackRegisterChanges()
        {
            string register = "";
            ushort value = 0x0000;

            using(var _ = mem.Load(0x8000))
            {
                _
                .LDA_IMMEDIATE(0x56)
                .BRK();
            }

            var tracker = _serviceProvider.GetService<IRegisterTracker>();

            tracker.RegisterUpdated += (s,e) =>
            {
                if(e.Register == "A") // Ignore anything else (liek PC or P.B)
                {
                    register = e.Register;
                    value = e.Value;    
                }
            };

            _cpu.Reset();

            Assert.AreEqual("A", register);
            Assert.AreEqual(0x56, value);

        }

        [Test]
        public void NoRegisterTrackerDoesntTrack()
        {
            var tracked = false;

            var tracker = new NoRegisterTracker();

            tracker.RegisterUpdated += (s,e) =>
            {
                tracked = true;
            };

            tracker.PostRegisterUpdated("A", 0x56);

            Assert.IsFalse(tracked);
        }

        [Test]
        public void CantPopLastLabelTableScope()
        {
            var labels = new LoaderLabelTable();

            Assert.AreEqual(0,labels.CurrentScope);

            labels.Pop();

            Assert.AreEqual(0,labels.CurrentScope);
        }

        [Test]
        public void LoaderLabelTableTryResolveOnUndefinedLabelReturnsFalse()
        {
            var labels = new LoaderLabelTable();
            ushort value;
            Assert.IsFalse(labels.TryResolve("NonExistentLabel", out value));
            Assert.AreEqual(0x0000, value);
        }
        [Test]
        public void LoaderLabelTableResolveOnUndefinedLabelFails()
        {
            var labels = new LoaderLabelTable();
            
            try
            {
                var value = labels.Resolve("NonExistentLabel");
            }
            catch(KeyNotFoundException)
            {
                Assert.Pass();
            }
            Assert.Fail();
        }

        [Test]
        public void LoaderInvalidByteQualifierFails()
        {
            try
            {
                using(var _ = mem.Load(0x8000))
                {
                    _
                    .Label("MyLabel")
                    .BRK()
                    .RefByte("MyLabel:invalid");
                }
            }
            catch(InvalidOperationException)
            {
                Assert.Pass();
            }
            Assert.Fail();
        }
        [Test]
        public void LoaderNegativeOffsetStoresCorrectValue()
        {
            using(var _ = mem.Load(0x8000))
            {
                _
                .Label("Start")
                .RefByte("Start:LO", -2);
            };

            var value = mem.Read(0x8000);
            Assert.AreEqual(0xfe, value);
        }
        [Test]
        public void LoaderLabelNegativeOffsetStoresCorrectValue()
        {
            using(var _ = mem.Load(0x8000))
            {
                _
                .Label("Start")
                .Ref("Start-2");
            };

            var value = mem.ReadWord(0x8000);
            Assert.AreEqual(0x7ffe, value);
        }
        [Test]
        public void LoaderLabelPositiveOffsetStoresCorrectValue()
        {
            using(var _ = mem.Load(0x8000))
            {
                _
                .Label("Start")
                .Ref("Start+2");
            };

            var value = mem.ReadWord(0x8000);
            Assert.AreEqual(0x8002, value);
        }

        [Test]
        public void LoaderLabelRelativeAddressStoresCorrectValue()
        {
            using(var _ = mem.Load(0x8000))
            {
                _
                .Label("Start")
                .RelativeAddress(2);
            };

            var value = mem.ReadWord(0x8000);
            Assert.AreEqual(0x8002, value);
        }
        [Test]
        public void LoaderLabelFromSetsCursorCorrectly()
        {
            using(var _ = mem.Load(0x8000))
            {
                _
                .From(0x9000)
                .Label("Start")
                .RelativeAddress(2);
            };

            var value = mem.ReadWord(0x9000);
            Assert.AreEqual(0x9002, value);
        }

        [Test]
        public void CanClearLoaderLabelTable()
        {
            using(var _ = mem.Load(0x8000))
            {
                _
                .Label("Start")
                .Ref("Start+2");
            };

            ushort value;
            Assert.IsTrue(mem.Labels.TryResolve("Start", out value));

            mem.Labels.Clear();
            Assert.IsFalse(mem.Labels.TryResolve("Start", out value));
        }
        [Test]
        public void AddDuplicateLabelReturnsError()
        {
            try
            {
                using(var _ = mem.Load(0x8000))
                {
                    _
                    .Label("Start")
                    .Ref("Start+2")
                    .Label("Start");
                };
            }
            catch(InvalidProgramException)
            {
                Assert.Pass();
            }

            Assert.Fail();
        }
        [Test]
        public void LabelOverflowReturnsError()
        {
            try
            {
                using(var _ = mem.Load(0x8000))
                {
                    _
                    .Label("Start")
                    .RefByte("Start+2");
                };
            }
            catch(InvalidProgramException)
            {
                Assert.Pass();
            }

            Assert.Fail();
        }
        [Test]
        public void RelativeLabelOffsetOverflowReturnsError()
        {
            try
            {
                using(var _ = mem.Load(0xFFF0))
                {
                    _
                    .Label("Start")
                    .Ref("Start+20");
                };
            }
            catch(InvalidProgramException)
            {
                Assert.Pass();
            }

            Assert.Fail();
        }
        [Test]
        public void UndefinedLabelReferenceReturnsError()
        {
            try
            {
                using(var _ = mem.Load(0x8000))
                {
                    _
                    .Ref("Start+20");
                };
            }
            catch(InvalidProgramException)
            {
                Assert.Pass();
            }

            Assert.Fail();
        }
        [Test]
        public void CanExportLoaderLabelTable()
        {
            ILoaderLabelTable table = null;

            using(var _ = mem.Load(0x8000))
            {
                _
                .Label("Start")
                .Ref("Start+2")
                .Fixup(out table);
            };

            Assert.IsNotNull(table);
        }
    }
}