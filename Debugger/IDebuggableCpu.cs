using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Debugger
{

    public interface IDebuggableCpu
    {
        void Go();
        void Stop();
        void Step();
        EventHandler<ExecutedEventArgs> HasExecuted {get; set;}
        int Verbosity {get;set;}
        ushort PC {get; set;}
        byte SP {get; set;}
        byte A {get;set;}
        byte X {get;set;}
        byte Y {get;set;}
        bool C {get; set;}
        bool Z {get;set;}
        bool D {get;set;}
        bool I {get;set;}
        bool V {get;set;}
        bool N {get; set;}
        bool B {get;set;}
        bool B2 {get;set;}
        ProgramBreakpoints Breakpoints {get; }
        EventHandler<ProgramBreakpointEventArgs> BreakpointTriggered {get; set;}
        void ClearBreakpoints();
        bool AddBreakpoint(ProgramBreakpoint breakpoint);
        bool DeleteBreakpoint(ProgramBreakpoint breakpoint);
    }
}
