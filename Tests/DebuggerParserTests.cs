using NUnit.Framework;
using System.Threading.Tasks;
using System;
using Debugger;
using System.Diagnostics;

namespace Tests
{
    public class DebuggerParserTests
    {
        private ICpuDebug _cpuDebug;
        private IMemoryDebug _memoryDebug;
        private ILogFormatter _logFormatter;
        private Labels _labels;
        private Parser _parser;

        [SetUp]
        public async Task Setup()
        {
            _cpuDebug = new MockCpuDebug();
            _memoryDebug = new MockMemoryDebug();
            _labels = new Labels();
            _logFormatter = new DebugLogFormatter(_labels);
            _parser = new Parser(_cpuDebug, _memoryDebug, _labels, _logFormatter);

            await Task.Delay(0);
        }

        [Test]
        public void CanPeekA()
        {
            _cpuDebug.A = 0xAA;
            var command = "?A";
            _parser.Parse(command);
            var output = _logFormatter.ToString();
            Console.WriteLine(output);
            Assert.IsTrue(output.Contains("A = $AA (170)"));
        }
        [Test]
        public void CanPeekX()
        {
            _cpuDebug.X = 0xAB;
            var command = "?X";
            _parser.Parse(command);
            var output = _logFormatter.ToString();
            Console.WriteLine(output);
            Assert.IsTrue(output.Contains("X = $AB (171)"));
        }
        [Test]
        public void CanPeekY()
        {
            _cpuDebug.Y = 0xAC;
            var command = "?Y";
            _parser.Parse(command);
            var output = _logFormatter.ToString();
            Console.WriteLine(output);
            Assert.IsTrue(output.Contains("Y = $AC (172)"));
        }
        [Test]
        public void CanPeekByte()
        {
            var command = "?1234";
            _parser.Parse(command);
            var output = _logFormatter.ToString();
            Console.WriteLine("");
            Console.WriteLine(output);
            Assert.IsTrue(output.Contains("$34 (52)"));
        }
        [Test]
        public void CanPeekWord()
        {
            var command = "?&1234";
            _parser.Parse(command);
            var output = _logFormatter.ToString();
            Console.WriteLine("");
            Console.WriteLine(output);
            Assert.IsTrue(output.Contains("$[1234] = $3534 (13620)"));
        }
        [Test]
        public void CanPeekRange()
        {
            var command = "?1234-2345";
            _parser.Parse(command);
            var output = _logFormatter.ToString();
            Console.WriteLine("");
            Console.WriteLine(output);
            Assert.IsTrue(output.Contains("[2344]                  44 45"));
        }
        [Test]
        public void CanPeekBlock()
        {
            var command = "?1234:30";
            _parser.Parse(command);
            var output = _logFormatter.ToString();
            Console.WriteLine("");
            Console.WriteLine(output);
            Assert.Pass();
            Assert.IsTrue(output.Contains("[1244]                  44 45 46 47 48 49 4A 4B 4C 4D 4E 4F 50 51"));
        }
        [Test]
        public void CanPeekWordByLabel()
        {
            _labels.Add(new Label("TEST", 0x1234));

            var command = "?&TEST";
            _parser.Parse(command);
            var output = _logFormatter.ToString();
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
            var output = _logFormatter.ToString();
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
            var output = _logFormatter.ToString();
            Console.WriteLine("");
            Console.WriteLine(output);
            Assert.IsTrue(output.Contains("[2344]                  44 45"));
        }
        [Test]
        public void CanPeekBlockByLabel()
        {
            _labels.Add(new Label("start", 0x1234));
            var command = "?start:30";
            _parser.Parse(command);
            var output = _logFormatter.ToString();
            Console.WriteLine("");
            Console.WriteLine(output);
            Assert.Pass();
            Assert.IsTrue(output.Contains("[1244]                  44 45 46 47 48 49 4A 4B 4C 4D 4E 4F 50 51"));
        }
        [Test]
        public void CanAddBreakpoint()
        {
            var command = "add breakpoint 1234";
            _parser.Parse(command);
            Assert.AreEqual("", _logFormatter.ToString());
            Assert.AreEqual(0x1234, ((ProgramAddressBreakpoint)(_cpuDebug.Breakpoints[0])).Address);
        }
        [Test]
        public void CanAddBreakpointShortforms()
        {
            _labels.Add(new Label("test", 0x1234));
            var command = "a b test";
            _parser.Parse(command);
            Assert.AreEqual("", _logFormatter.ToString());
            Assert.AreEqual(0x1234, ((ProgramAddressBreakpoint)(_cpuDebug.Breakpoints[0])).Address);
        }
        [Test]
        public void CanDeleteBreakpoint()
        {
            _cpuDebug.AddBreakpoint(new ProgramAddressBreakpoint(0x1234));
            _cpuDebug.AddBreakpoint(new ProgramAddressBreakpoint(0x2345));

            var command = "delete breakpoint 1234";
            _parser.Parse(command);
            Assert.AreEqual("", _logFormatter.ToString());
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

            var command = "d b test2";
            _parser.Parse(command);
            Assert.AreEqual("", _logFormatter.ToString());
            Assert.AreEqual(1, _cpuDebug.Breakpoints.Count);
            Assert.AreEqual(0x1234, ((ProgramAddressBreakpoint)(_cpuDebug.Breakpoints[0])).Address);
        }
    }
}