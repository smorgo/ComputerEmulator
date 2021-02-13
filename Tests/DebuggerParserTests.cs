using NUnit.Framework;
using System.Threading.Tasks;
using System;
using Debugger;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using HardwareCore;
using Microsoft.Extensions.Logging;

namespace Tests
{
    public class DebuggerParserTests
    {
        private IDebuggableCpu _cpuDebug;
        private IAddressMap _addressMap;
        private ILogFormatter _logFormatter;
        private ILabelMap _labels;
        private IParser _parser;
        private ServiceProvider _serviceProvider;
        private UnitTestLogger<Parser> _logger;

        public DebuggerParserTests()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
            ServiceProviderLocator.ServiceProvider = _serviceProvider;
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services
                 .AddScoped(typeof(ILogger<Parser>), typeof(UnitTestLogger<Parser>))
                 .AddScoped<IDebuggableCpu, MockCpuDebug>()
                 .AddScoped<IAddressMap, MockMemoryDebug>()
                 .AddScoped<ILabelMap, LabelMap>()
                 .AddScoped<ILogFormatter, DebugLogFormatter>()
                 .AddScoped<IParser, Parser>()
                 .AddScoped<ICpuHoldEvent,CpuDontHoldEvent>()
                 .AddScoped<ICpuStepEvent,CpuDontStepEvent>()
                 .AddScoped<IRegisterTracker, NoRegisterTracker>()
                 .AddSingleton<CancellationTokenWrapper>(new CancellationTokenWrapper(default(CancellationToken)));
        }
        [SetUp]
        public void Setup()
        {
            _cpuDebug = _serviceProvider.GetService<IDebuggableCpu>();
            _addressMap = _serviceProvider.GetService<IAddressMap>();
            _labels = _serviceProvider.GetService<ILabelMap>();
            _labels.Clear();
            _logFormatter = _serviceProvider.GetService<ILogFormatter>();
            _parser = _serviceProvider.GetService<IParser>();
            _logger = (UnitTestLogger<Parser>)_serviceProvider.GetService<ILogger<Parser>>();
            _cpuDebug.Breakpoints.Clear();
        }

        [Test]
        public void CanPeekA()
        {
            _cpuDebug.A = 0xAA;
            var command = "?A";
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Console.WriteLine(output);
            Assert.IsTrue(output.Contains("A = $AA (170)"));
        }
        [Test]
        public void CanPeekX()
        {
            _cpuDebug.X = 0xAB;
            var command = "?X";
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Console.WriteLine(output);
            Assert.IsTrue(output.Contains("X = $AB (171)"));
        }
        [Test]
        public void CanPeekY()
        {
            _cpuDebug.Y = 0xAC;
            var command = "?Y";
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Console.WriteLine(output);
            Assert.IsTrue(output.Contains("Y = $AC (172)"));
        }
        [Test]
        public void CanPeekByte()
        {
            var command = "?1234";
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Console.WriteLine("");
            Console.WriteLine(output);
            Assert.IsTrue(output.Contains("$34 (52)"));
        }
        [Test]
        public void CanPeekWord()
        {
            var command = "?&1234";
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Console.WriteLine("");
            Console.WriteLine(output);
            Assert.IsTrue(output.Contains("$[1234] = $3534 (13620)"));
        }
        [Test]
        public void CanPeekRange()
        {
            var command = "?1234-2345";
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Console.WriteLine("");
            Console.WriteLine(output);
            Assert.IsTrue(output.Replace(" ","").Contains("[2344]4445"));
        }
        [Test]
        public void CanPeekBlock()
        {
            var command = "?1234:30";
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Console.WriteLine("");
            Console.WriteLine(output);
            output = output.Replace(" ","");
            Assert.IsTrue(output.Contains("[1244]4445464748494A4B4C4D4E4F5051"));
        }
        [Test]
        public void CanPeekWordByLabel()
        {
            _labels.Add(new Label("TEST", 0x1234));

            var command = "?&TEST";
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Console.WriteLine("");
            Console.WriteLine(output);
            Assert.IsTrue(output.Contains("$[1234] = $3534 (13620)"));
        }
        [Test]
        public void CanPeekByteByLabel()
        {
            _labels.Add(new Label("TEST", 0x1234));

            var command = "?TEST";
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Console.WriteLine("");
            Console.WriteLine(output);
            Assert.IsTrue(output.Contains("$34 (52)"));
        }
        [Test]
        public void CanPeekRangeByLabel()
        {
            _labels.Add(new Label("start", 0x1234));
            _labels.Add(new Label("end", 0x2345));
            var command = "?start-end";
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Console.WriteLine("");
            Console.WriteLine(output);
            Assert.IsTrue(output.Contains("[2345] end:                 45"));
        }
        [Test]
        public void CanPeekBlockByLabel()
        {
            _labels.Add(new Label("start", 0x1234));
            var command = "?start:30";
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Console.WriteLine("");
            Console.WriteLine(output);
            output = output.Replace(" ","");
            Assert.IsTrue(output.Contains("[1244]4445464748494A4B4C4D4E4F5051"));
        }
        [Test]
        public void CanAddBreakpoint()
        {
            var command = "add breakpoint 1234";
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Assert.IsTrue(output.Contains("Breakpoint") && output.Contains("added"));
            Assert.AreEqual(0x1234, ((ProgramAddressBreakpoint)(_cpuDebug.Breakpoints[0])).Address);
        }
        [Test]
        public void CanAddBreakpointShortforms()
        {
            _labels.Add(new Label("test", 0x1234));
            var command = "a b test";
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Assert.IsTrue(output.Contains("Breakpoint") && output.Contains("added"));
            Assert.AreEqual(0x1234, ((ProgramAddressBreakpoint)(_cpuDebug.Breakpoints[0])).Address);
        }
        [Test]
        public void CanDeleteBreakpoint()
        {
            _cpuDebug.AddBreakpoint(new ProgramAddressBreakpoint(0x1234));
            _cpuDebug.AddBreakpoint(new ProgramAddressBreakpoint(0x2345));

            var command = "delete breakpoint 1";
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Assert.AreEqual("", output);
            Assert.AreEqual(1, _cpuDebug.Breakpoints.Count);
            Assert.AreEqual(0x2345, ((ProgramAddressBreakpoint)(_cpuDebug.Breakpoints[0])).Address);
        }
        [Test]
        public void CanDeleteBreakpointShortForms()
        {
            _cpuDebug.AddBreakpoint(new ProgramAddressBreakpoint(0x1234));
            _cpuDebug.AddBreakpoint(new ProgramAddressBreakpoint(0x2345));
            _labels.Add(new Label("test1", 0x1234));
            _labels.Add(new Label("test2", 0x2345));

            var command = "de b 2";
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Assert.AreEqual("", output);
            Assert.AreEqual(1, _cpuDebug.Breakpoints.Count);
            Assert.AreEqual(0x1234, ((ProgramAddressBreakpoint)(_cpuDebug.Breakpoints[0])).Address);
        }
        [Test]
        public void CanListBreakpoints()
        {
            _cpuDebug.AddBreakpoint(new ProgramAddressBreakpoint(0x1234));
            _cpuDebug.AddBreakpoint(new ProgramAddressBreakpoint(0x2345));

            var command = "list breakpoints";
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Assert.IsTrue(output.Contains("PC==$2345 (9029)"));
        }
        [Test]
        public void CanListBreakpointsShortForm()
        {
            _cpuDebug.AddBreakpoint(new ProgramAddressBreakpoint(0x1234));
            _cpuDebug.AddBreakpoint(new ProgramAddressBreakpoint(0x2345));

            var command = "l b";
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Assert.IsTrue(output.Contains("PC==$2345 (9029)"));
        }
        [Test]
        public void CanListAll()
        {
            _cpuDebug.AddBreakpoint(new ProgramAddressBreakpoint(0x1234));
            _cpuDebug.AddBreakpoint(new ProgramAddressBreakpoint(0x2345));

            var command = "list all";
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Assert.IsTrue(output.Contains("PC==$1234 (4660)"));
        }
        [Test]
        public void CanListAllShortForm()
        {
            _cpuDebug.AddBreakpoint(new ProgramAddressBreakpoint(0x1234));
            _cpuDebug.AddBreakpoint(new ProgramAddressBreakpoint(0x2345));

            var command = "l a";
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Assert.IsTrue(output.Contains("PC==$1234 (4660)"));
        }
        [Test]
        public void CanListDefault()
        {
            _cpuDebug.AddBreakpoint(new ProgramAddressBreakpoint(0x1234));
            _cpuDebug.AddBreakpoint(new ProgramAddressBreakpoint(0x2345));

            var command = "list";
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Assert.IsTrue(output.Contains("PC==$1234 (4660)"));
        }
        [Test]
        public void CanListDefaultShortForm()
        {
            _cpuDebug.AddBreakpoint(new ProgramAddressBreakpoint(0x1234));
            _cpuDebug.AddBreakpoint(new ProgramAddressBreakpoint(0x2345));

            var command = "l";
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Assert.IsTrue(output.Contains("PC==$1234 (4660)"));
        }

    }
}