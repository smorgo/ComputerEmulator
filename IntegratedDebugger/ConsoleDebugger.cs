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
        private IAddressMap _addressMap;
        private IDebuggableMemory MemoryDebugger => (IDebuggableMemory)_addressMap;
        private CancellationTokenWrapper _cancel;
        private ILogFormatter _formatter;
        private ILabelMap _labels;
        public ILabelMap Labels => _labels;
        private IParser _parser;
        // private CpuHoldEvent _debuggerSyncEvent;
        private ILogger<ConsoleDebugger> _logger;
        public ConsoleDebugger(
            IEmulatorHost host, 
            IDebuggableCpu cpu, 
            IAddressMap addressMap,
            ILogFormatter logFormatter, 
            IParser parser, 
            ILabelMap labels, 
            CancellationTokenWrapper cancel, 
            // CpuHoldEvent debuggerSyncEvent, 
            ILogger<ConsoleDebugger> logger)
        {
            _host = host;
            _cpu = cpu;
            _addressMap = addressMap;
            _formatter = logFormatter;
            _parser = parser;
            _labels = labels;
            _cancel = cancel;
            // _debuggerSyncEvent = debuggerSyncEvent;
            _logger = logger;
        }


        public async Task OnProgramBreakpointTriggered(object sender, ProgramBreakpointEventArgs e)
        {
            _logger.LogInformation($"Stopped at program breakpoint {e.Breakpoint.Id}");
            _host.Cpu.Stop();
            await Task.Delay(0);
        }
        public async Task OnMemoryBreakpointTriggered(object sender, MemoryBreakpointEventArgs e)
        {
            _logger.LogInformation($"Stopped at memory breakpoint {e.Breakpoint.Id}");
            _host.Cpu.Stop();
            await Task.Delay(0);
        }

        public void Start()
        {
            _cpu.BreakpointTriggered += async (s,e) => {await OnProgramBreakpointTriggered(s,e);};
            MemoryDebugger.BreakpointTriggered += async (s,e) => {await OnMemoryBreakpointTriggered(s,e);};


            while(!_cancel.Token.IsCancellationRequested)
            {
                Thread.Sleep(1000);
            }
        }

        public void RunCommand(string command)
        {
            _parser.Parse(command);
        }
    }
}
