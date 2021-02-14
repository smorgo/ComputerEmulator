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
        private MockCpuHoldEvent _cpuHoldEvent;
        private MockCpuStepEvent _cpuStepEvent;


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
                 .AddScoped<ICpuHoldEvent,MockCpuHoldEvent>()
                 .AddScoped<ICpuStepEvent,MockCpuStepEvent>()
                 .AddScoped<IRegisterTracker, NoRegisterTracker>()
                 .AddSingleton<CancellationTokenWrapper>(new CancellationTokenWrapper(default(CancellationToken)));
        }
        [SetUp]
        public void Setup()
        {
            _cpuDebug = _serviceProvider.GetService<IDebuggableCpu>();
            _addressMap = _serviceProvider.GetService<IAddressMap>();
            AsyncUtil.RunSync(_addressMap.Initialise);

            _labels = _serviceProvider.GetService<ILabelMap>();
            _labels.Clear();
            _logFormatter = _serviceProvider.GetService<ILogFormatter>();
            _parser = _serviceProvider.GetService<IParser>();
            _logger = (UnitTestLogger<Parser>)_serviceProvider.GetService<ILogger<Parser>>();
            _cpuDebug.Breakpoints.Clear();
            _cpuHoldEvent = (MockCpuHoldEvent)_serviceProvider.GetService<ICpuHoldEvent>();
            _cpuStepEvent = (MockCpuStepEvent)_serviceProvider.GetService<ICpuStepEvent>();
            _cpuHoldEvent.Init();
            _cpuStepEvent.Init();
            _logger.GetOutput(); // Flush any old content
        }

        [TestCase("This isn't a valid command")]
        [TestCase("add banana")]
        [TestCase("list banana")]
        [TestCase("disable banana")]
        [TestCase("enable banana")]
        [TestCase("clear banana")]
        [TestCase("delete banana")]
        [TestCase("?banana")]
        [TestCase("banana=fruit")]
        [TestCase("&banana=fruit")]
        [TestCase("$1234=1")]
        [TestCase("add breakpoint 10101")]
        [TestCase("delete breakpoint banana")]
        [TestCase("enable breakpoint banana")]
        [TestCase("disable breakpoint banana")]
        [TestCase("?a-z")]
        [TestCase("?00-z")]
        [TestCase("?z-00")]
        [TestCase("?00:Z")]
        [TestCase("?12345")]
        [TestCase("?Z:01")]
        [TestCase("?&banana")]
        [TestCase("?00=10101")]
        [TestCase("&banana=1")]
        [TestCase("?00:65537")]
        [TestCase("?100:0")]
        public void ParserReturnsErrorOnInvalidCommand(string command)
        {
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Assert.IsTrue(output.StartsWith("Command not recognised:"));
            Assert.IsTrue(output.EndsWith(command));
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

        [TestCase("list watches", "No watches defined")]
        [TestCase(" L w  ", "No watches defined")]
        public void CanListWatches(string command, string expected)
        {
            // Can't add a watch at the moment!
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Assert.IsTrue(output.Contains(expected));
            
        }

        [TestCase("list labels", "There are 2 labels defined:")]
        [TestCase(" L L  ", "finished = $2345")]
        public void CanListLabels(string command, string expected)
        {
            var labels = _serviceProvider.GetService<ILabelMap>();
            labels.Add(new Label("loop", 0x1234));
            labels.Add(new Label("finished", 0x2345));
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Assert.IsTrue(output.Contains(expected));
            
        }
        
        [TestCase("list labels", "No labels defined")]
        [TestCase("list breakpoints", "No breakpoints defined")]
        public void ListWithNoneDefinedReturnsAppropriateMessage(string command, string expected)
        {
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Assert.IsTrue(output.Contains(expected));
            
        }

        [TestCase("go")]
        [TestCase("g")]
        [TestCase(" GO ")]
        public void CanGo(string command)
        {
            _parser.Parse(command);
            Assert.IsTrue(_cpuHoldEvent.IsSet);
            Assert.IsTrue(_cpuStepEvent.IsSet);
            Assert.AreEqual(1, _cpuHoldEvent.SetCount);
            Assert.AreEqual(1, _cpuStepEvent.SetCount);
            Assert.AreEqual(0, _cpuHoldEvent.ResetCount);
            Assert.AreEqual(0, _cpuStepEvent.ResetCount);
            Assert.AreEqual(RunMode.Running, _parser.RunMode);
        }

        [TestCase("step")]
        [TestCase("ste")]
        [TestCase("st")]
        [TestCase("s")]
        [TestCase(" STEP ")]
        public void CanStep(string command)
        {
            _cpuHoldEvent.Set();
            _parser.Parse(command);
            Assert.IsTrue(_cpuHoldEvent.IsSet);
            Assert.IsFalse(_cpuStepEvent.IsSet);
            Assert.AreEqual(2, _cpuHoldEvent.SetCount);
            Assert.AreEqual(1, _cpuStepEvent.SetCount);
            Assert.AreEqual(1, _cpuHoldEvent.ResetCount);
            Assert.AreEqual(1, _cpuStepEvent.ResetCount);
            Assert.AreEqual(RunMode.Stepping, _parser.RunMode);
        }

        [TestCase("pause")]
        [TestCase("paus")]
        [TestCase("pau")]
        [TestCase("pa")]
        [TestCase("p")]
        [TestCase(" PAUSE ")]
        public void CanPause(string command)
        {
            _cpuHoldEvent.Set();
            _cpuStepEvent.Set();
            _parser.Parse(command);
            Assert.IsFalse(_cpuHoldEvent.IsSet);
            Assert.IsTrue(_cpuStepEvent.IsSet);
            Assert.AreEqual(1, _cpuHoldEvent.SetCount);
            Assert.AreEqual(2, _cpuStepEvent.SetCount);
            Assert.AreEqual(1, _cpuHoldEvent.ResetCount);
            Assert.AreEqual(0, _cpuStepEvent.ResetCount);
            Assert.AreEqual(RunMode.Paused, _parser.RunMode);
        }

        [TestCase("help")]
        [TestCase("hel")]
        [TestCase("he")]
        [TestCase("h")]
        [TestCase("?")]
        [TestCase(" HELP ")]
        public void CanDisplayHelp(string command)
        {
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Assert.IsTrue(output.Contains("Keywords and hex values are case-insensitive."));
        }

        [TestCase("disable breakpoint 1")]
        [TestCase("dis br 1")]
        [TestCase("di b 01")]
        [TestCase(" DISable   breakPOINT   1  ")]
        public void CanDisableBreakpoint(string command)
        {
            _cpuDebug.AddBreakpoint(new ProgramAddressBreakpoint(0x1234));
            _cpuDebug.AddBreakpoint(new ProgramAddressBreakpoint(0x2345));

            _parser.Parse(command);

            Assert.IsTrue(_cpuDebug.Breakpoints[0].Disabled);
        }

        [TestCase("enable breakpoint 2")]
        [TestCase("ena br 2")]
        [TestCase("e b 02")]
        [TestCase(" ENable   breakPOINT   2  ")]
        public void CanEnableBreakpoint(string command)
        {
            _cpuDebug.AddBreakpoint(new ProgramAddressBreakpoint(0x1234));
            _cpuDebug.AddBreakpoint(new ProgramAddressBreakpoint(0x2345) { Disabled = true });

            _parser.Parse(command);

            Assert.IsFalse(_cpuDebug.Breakpoints[1].Disabled);
        }

        [TestCase("A=1", 1, "A")]
        [TestCase("X=2", 2, "X")]
        [TestCase("Y=3", 3, "Y")]
        [TestCase("  Pc  = 1234", 0x1234, "PC")]
        [TestCase(" SP=27f", 0x7F, "SP")]
        public void CanAssignRegister(string command, int expected, string register)
        {
            _parser.Parse(command);
            Assert.AreEqual(expected, ((MockCpuDebug)_cpuDebug).GetRegister(register));
        }

        [TestCase("C=1", true, "C")]
        [TestCase("C=0", false, "C")]
        [TestCase("Z=1", true, "Z")]
        [TestCase("Z=0", false, "Z")]
        [TestCase("I=1", true, "I")]
        [TestCase("I=0", false, "I")]
        [TestCase("D=1", true, "D")]
        [TestCase("D=0", false, "D")]
        [TestCase("B=1", true, "B")]
        [TestCase("B=0", false, "B")]
        [TestCase("B2=1", true, "B2")]
        [TestCase("B2=0", false, "B2")]
        [TestCase("V=1", true, "V")]
        [TestCase("V=0", false, "V")]
        [TestCase("N=1", true, "N")]
        [TestCase(" n  =  0  ", false, "N")]
        public void CanAssignFlag(string command, bool expected, string flag)
        {
            _parser.Parse(command);
            Assert.AreEqual(expected, ((MockCpuDebug)_cpuDebug).GetFlag(flag));
        }

        [TestCase("?C", "C=1", "C = $1 (1)")]
        [TestCase("?C", "C=0", "C = $0 (0)")]
        [TestCase("?Z", "Z=1", "Z = $1 (1)")]
        [TestCase("?Z", "Z=0", "Z = $0 (0)")]
        [TestCase("?I", "I=1", "I = $1 (1)")]
        [TestCase("?I", "I=0", "I = $0 (0)")]
        [TestCase("?D", "D=1", "D = $1 (1)")]
        [TestCase("?D", "D=0", "D = $0 (0)")]
        [TestCase("?B", "B=1", "B = $1 (1)")]
        [TestCase("?B", "B=0", "B = $0 (0)")]
        [TestCase("?B2", "B2=1", "B2 = $1 (1)")]
        [TestCase("?B2", "B2=0", "B2 = $0 (0)")]
        [TestCase("?V", "V=1", "V = $1 (1)")]
        [TestCase("?V", "V=0", "V = $0 (0)")]
        [TestCase("?N", "N=1", "N = $1 (1)")]
        [TestCase("?N", "N=0", "N = $0 (0)")]
        public void CanPeekFlag(string command, string assignCommand, string expected)
        {
            _parser.Parse(assignCommand);
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Assert.AreEqual(expected, output);
        }

        [TestCase("0=1", 0x0000, 1)]
        [TestCase("  1234  = 101", 0x1234, 1)]
        public void CanAssignByte(string command, int address, byte expected)
        {
            _parser.Parse(command);
            Assert.AreEqual(expected, _addressMap.Read((ushort)address));
        }

        [TestCase("&0=1010", 0x0000, 0x1010)]
        [TestCase("  &1234  = F1F", 0x1234, 0x0F1F)]
        public void CanAssignWord(string command, int address, int expected)
        {
            _parser.Parse(command);
            Assert.AreEqual((ushort)expected, _addressMap.ReadWord((ushort)address));
        }

        [TestCase("?A", "A=1", "A", 1)]
        [TestCase("?X", "X=2", "X", 2)]
        [TestCase("?Y", "Y=3", "Y", 3)]
        [TestCase(" ? pC", "PC=1234", "PC", 0x1234)]
        [TestCase(" ?sp", "SP=55", "SP", 0x55)]
        public void CanPeekRegister(string peekCommand, string assignCommand, string register, int expected)
        {
            _parser.Parse(assignCommand);
            _parser.Parse(peekCommand);
            Assert.AreEqual((ushort)expected, ((MockCpuDebug)_cpuDebug).GetRegister(register));
        }

        [TestCase("?verbosity", 1, "VERBOSITY = $1 (1)")]
        [TestCase("?verb", 2, "VERBOSITY = $2 (2)")]
        [TestCase("  ?  VERBOsitY", 3, "VERBOSITY = $3 (3)")]
        public void CanPeekVerbosity(string command, int value, string expected)
        {
            _cpuDebug.Verbosity = value;
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Assert.AreEqual(expected, output);
        }

        [TestCase("clear breakpoints")]
        [TestCase("cl br")]
        [TestCase("c b")]
        [TestCase("clEa brEAK")]
        public void CanClearBreakpoints(string command)
        {
            _cpuDebug.Breakpoints.Add(new ProgramAddressBreakpoint(0x1234));
            Assert.AreEqual(1, _cpuDebug.Breakpoints.Count);
            _parser.Parse(command);
            Assert.AreEqual(0, _cpuDebug.Breakpoints.Count);
        }

        [TestCase("clear watches", "Watches cleared")]
        [TestCase("  C  Wat  ", "Watches cleared")]
        public void CanClearWatches(string command, string expected)
        {
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Assert.AreEqual(expected, output);
        }

        [TestCase("clear all", "Breakpoints and watches cleared")]
        [TestCase("  C  a  ", "Breakpoints and watches cleared")]
        public void CanClearAll(string command, string expected)
        {
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Assert.AreEqual(expected, output);
        }

        [TestCase("enable breakpoint 40", "Breakpoint 40 not found")]
        [TestCase("disable breakpoint 40", "Breakpoint 40 not found")]
        [TestCase("delete breakpoint 40", "Breakpoint 40 not found")]
        public void InvalidBreakpointIdReturnsAppropriateError(string command, string expected)
        {
            _parser.Parse(command);
            var output = _logger.GetOutput();
            Assert.AreEqual(expected, output);
        }

        [Test]
        public void PeekMemoryLimitedTo16BitEndAddress()
        {
            _parser.Parse("?fff0:100");
            var output = _logger.GetOutput().Replace(" ","").Replace(Environment.NewLine,"");
            Assert.AreEqual("[FFF0]F0F1F2F3F4F5F6F7F8F9FAFBFCFDFEFF[................]", output);
        }
    }
}