using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Debugger;
using HardwareCore;
using Microsoft.Extensions.Logging;

namespace IntegratedDebugger
{
    public class ConsoleDebugger : IDebugger
    {
        private IEmulatorHost _host;
        private IDebuggableCpu _cpu;
        private CancellationTokenWrapper _cancel;
        private ILogFormatter _formatter;
        private ILabelMap _labels;
        public ILabelMap Labels => _labels;
        private IParser _parser;
        private CpuHoldEvent _debuggerSyncEvent;
        private ILogger<ConsoleDebugger> _logger;
        public ConsoleDebugger(IEmulatorHost host, IDebuggableCpu cpu, ILogFormatter logFormatter, IParser parser, ILabelMap labels, CancellationTokenWrapper cancel, CpuHoldEvent debuggerSyncEvent, ILogger<ConsoleDebugger> logger)
        {
            _host = host;
            _cpu = cpu;
            _formatter = logFormatter;
            _parser = parser;
            _labels = labels;
            _cancel = cancel;
            _debuggerSyncEvent = debuggerSyncEvent;
            _logger = logger;
        }

        public async Task OnHasExecuted(object sender, ExecutedEventArgs e)
        {
            foreach(var breakpoint in _host.Cpu.Breakpoints)
            {
                if(breakpoint.ShouldBreakOnInstruction(e.PC, e.Opcode))
                {
                    _host.Cpu.Stop();
                }
            }

            await Task.Delay(0);
        }

        public void Start()
        {
            _cpu.HasExecuted += async (s,e) => {await OnHasExecuted(s,e);};

            while(!_cancel.Token.IsCancellationRequested)
            {
                Thread.Sleep(10);
            }
        }

        public void RunCommand(string command)
        {
            _parser.Parse(command);
        }
    }
}
