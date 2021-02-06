using System;

namespace Debugger
{
    public interface ICpuDebug
    {
        bool DebugStop {get; set;}
        EventHandler HasExecuted {get; set;}
        EventHandler<CpuLog> Log {get; set;}
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
    }
}
