using System;
using System.Collections.Generic;
using System.Threading;
using Debugger;
using HardwareCore;

namespace Tests
{
    public class MockCpuDebug : IDebuggableCpu
    {
        private ICpuHoldEvent _debuggerSyncEvent;
        private ICpuStepEvent _debuggerStepEvent;

        public void Go()
        {
            _debuggerSyncEvent.Set();
            _debuggerStepEvent.Set();
        }
        public void Stop()
        {
            _debuggerSyncEvent.Reset();
            _debuggerStepEvent.Set();
        }

        public void Step()
        {
            _debuggerSyncEvent.Reset();
            Thread.Sleep(10);
            _debuggerStepEvent.Set(); // If we're stuck on the Step wait
            Thread.Sleep(10);
            _debuggerStepEvent.Reset(); // If we're stuck on the Step wait
            Thread.Sleep(10);
            _debuggerSyncEvent.Set();
        }

        public EventHandler<ExecutedEventArgs> HasExecuted { get; set; }
        public int Verbosity {get; set;}
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
        public ProgramBreakpoints Breakpoints {get;} = new ProgramBreakpoints();
        public EventHandler<ProgramBreakpointEventArgs> BreakpointTriggered {get; set;}
        public MockCpuDebug(
            ICpuHoldEvent debuggerSyncEvent, 
            ICpuStepEvent debuggerStepEvent)
        {
            _debuggerStepEvent = debuggerStepEvent;
            _debuggerSyncEvent = debuggerSyncEvent;
        }
        public void ClearBreakpoints()
        {
            Breakpoints.Clear();
        }
        public bool AddBreakpoint(ProgramBreakpoint breakpoint)
        {
            // Should ensure it's not a duplicate
            Breakpoints.Add(breakpoint);
            return true;
        }
        public bool DeleteBreakpoint(ProgramBreakpoint breakpoint)
        {
            if(Breakpoints.Contains(breakpoint))
            {
                Breakpoints.Remove(breakpoint);
                return true;
            }

            return false;
        }

        public ushort GetRegister(string register)
        {
            switch(register.ToUpper())
            {
                case "A":
                    return A;
                case "X":
                    return X;
                case "Y":
                    return Y;
                case "PC":
                    return PC;
                case "SP":
                    return SP;
                default:
                    throw new NotImplementedException();
            }
        }

        public bool GetFlag(string flag)
        {
            switch(flag.ToUpper())
            {
                case "C":
                    return C;
                case "Z":
                    return Z;
                case "I":
                    return I;
                case "D":
                    return D;
                case "B":
                    return B;
                case "B2":
                    return B2;
                case "V":
                    return V;
                case "N":
                    return N;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}