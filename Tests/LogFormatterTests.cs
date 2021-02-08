using NUnit.Framework;
using System.Threading.Tasks;
using System;
using Debugger;
using System.Diagnostics;

namespace Tests
{
    public class LogFormatterTests
    {
        private IDebuggableCpu _cpuDebug;
        private IMemoryDebug _memoryDebug;
        private ILogFormatter _logFormatter;
        private LabelMap _labels;

        [SetUp]
        public async Task Setup()
        {
            _cpuDebug = new MockCpuDebug();
            _memoryDebug = new MockMemoryDebug();
            _labels = new LabelMap();
            _logFormatter = new DebugLogFormatter(_labels);
            await Task.Delay(0);
        }

        [Test]
        public void CanLogBytes()
        {
            _labels.Add(new Label("Start", 0x1000));
            _labels.Add(new Label("MySubroutine", 0x1012));

            ushort address = 0x1000;
            ushort size = 50;
            var bytes = _memoryDebug.ReadBlock(address, (ushort)(address+size-1));
            _logFormatter.LogBytes(address, bytes);
            var output = _logFormatter.ToString();
            Console.WriteLine(""); // Clear the hanging line
            Debug.WriteLine(output);
            Console.WriteLine(output);
            Assert.IsTrue(output.Contains("[1012] MySubroutine:"));
        }

        [Test]
        public void CanLogWord()
        {
            _logFormatter.LogWord(0x1234, 0xFEDC);
            var output = _logFormatter.ToString();
            Console.WriteLine(""); // Clear the hanging line
            Debug.WriteLine(output);
            Console.WriteLine(output);
            Assert.IsTrue(output.Contains("$[1234] = $FEDC (65244)"));
        }

        [Test]
        public void CanLogRegister()
        {
            _logFormatter.LogRegister("A", 128, "80");
            var output = _logFormatter.ToString();
            Console.WriteLine(""); // Clear the hanging line
            Debug.WriteLine(output);
            Console.WriteLine(output);
            Assert.IsTrue(output.Contains("A = $80 (128)"));
        }

        [Test]
        public void CanLogManyLabels()
        {
            _labels.Add(new Label("ZeroPage", 0x00));
            _labels.Add(new Label("AddOperand1", 0x00));
            _labels.Add(new Label("MultiplyFactor", 0x02));
            _labels.Add(new Label("ArithResult", 0x04));
            _labels.Add(new Label("DisplayVector", 0x06));

            ushort address = 0x00;
            ushort size = 0x100;
            var bytes = _memoryDebug.ReadBlock(address, (ushort)(address+size-1));
            _logFormatter.LogBytes(address, bytes);
            var output = _logFormatter.ToString();
            Console.WriteLine(""); // Clear the hanging line
            Debug.WriteLine(output);
            Console.WriteLine(output);
            Assert.IsTrue(output.Contains("[0006] DisplayVector:"));
        }


    }
}