using System;
using System.Collections.Generic;
using Debugger;

namespace Tests
{
    public class MockCpuDebug : IDebuggableCpu
    {
        public void Go() {}
        public void Stop() {}
        public void Step() {}
        public EventHandler<ExecutedEventArgs> HasExecuted { get; set; }
        public EventHandler<CpuLogEventArgs> Log { get; set; }
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