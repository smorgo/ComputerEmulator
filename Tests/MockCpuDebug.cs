using System;
using Debugger;

namespace Tests
{
    public class MockCpuDebug : ICpuDebug
    {
        public bool DebugStop { get; set; }
        public EventHandler HasExecuted { get; set; }
        public EventHandler<CpuLog> Log { get; set; }
        public int Debug {get; set;}
        public ushort PC { get; set; }
        public byte SP { get; set; }
        public byte A { get; set; }
        public byte X { get; set; }
        public byte Y { get; set; }
        public bool C { get; set; }
        public bool Z { get; set; }
        public bool D { get; set; }
        public bool I { get; set; }
        public bool V { get; set; }
        public bool N { get; set; }
        public bool B { get; set; }
        public bool B2 { get; set; }
    }
}