using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using HardwareCore;
using Microsoft.Extensions.Logging;

namespace Debugger
{

    public class Parser : IParser
    {
        public enum RunMode
        {
            Unknown = 0,
            Paused = 1,
            Running = 2,
            Stepping = 3
        }
        private readonly IDebuggableCpu _cpuDebug;
        private readonly IMemoryDebug _memoryDebug;
        private readonly ILabelMap _labels;
        private readonly ILogFormatter _formatter;
        public ICpuHoldEvent _debuggerSyncEvent;
        public ICpuStepEvent _debuggerStepEvent;
        private readonly ILogger<Parser> _logger;
        private readonly IRegisterTracker _tracker;
        private RunMode _runMode;
        public Parser(
            IDebuggableCpu cpuDebug, 
            IMemoryDebug memoryDebug, 
            ILabelMap labels, 
            ILogFormatter formatter, 
            ICpuHoldEvent debuggerSyncEvent, 
            ICpuStepEvent debuggerStepEvent, 
            ILogger<Parser> logger,
            IRegisterTracker tracker)
        {
            _cpuDebug = cpuDebug;
            _memoryDebug = memoryDebug;
            _labels = labels;
            _formatter = formatter;
            _debuggerSyncEvent = debuggerSyncEvent;
            _debuggerStepEvent = debuggerStepEvent;
            _logger = logger;
            _tracker = tracker;
        }

        /*
         * Patterns to parse:
         *
         * ?<expression>
         * <id>=<expression>
         * list breakpoint|watch|all
         * add breakpoint|watch <breakpoint expression>
         * delete breakpoint|watch <expression>
         * disable breakpoint|watch <expression>
         * enable breakpoint|watch <expression>
         * clear breakpoint|watch <expression>
         * clear all
         * go
         * step
         * quit
         * help
         *
         * <expression> = <address>, <address range>, <word address>
         * <address> = <label name>, <hex>
         * <address range> = <address>-<address>, <address>:<length>
         * <word address> = &<address>
         */
        public void Parse(string command)
        {
            _formatter.Clear();

            if (ParseInternal(command))
            {
                _logger.LogInformation(_formatter.ToString());
            }
            else
            {
                _logger.LogError($"Command not recognise: {command}");
            }

            _formatter.Clear();
        }

        private bool ParseInternal(string command)
        {
            command = command.Trim();

            if (ParseControlCommand(command))
            {
                return true;
            }

            if (ParsePeek(command))
            {
                return true;
            }

            if (ParseAdd(command))
            {
                return true;
            }

            if (ParseDelete(command))
            {
                return true;
            }

            if (ParseList(command))
            {
                return true;
            }

            return false;
        }

        private void SetRunMode(RunMode mode)
        {
            _runMode = mode;
            _tracker?.PostRegisterUpdated("MODE", (ushort)_runMode);
        }

        private bool ParseControlCommand(string command)
        {
            if (KeywordMatches("go", command))
            {
                SetRunMode(RunMode.Running);
                _debuggerSyncEvent.Set();
                _debuggerStepEvent.Set();
                return true;
            }

            if (KeywordMatches("step", command) || (string.IsNullOrEmpty(command) && _runMode == RunMode.Stepping))
            {
                SetRunMode(RunMode.Stepping);
                _debuggerSyncEvent.Reset();
                Thread.Sleep(10);
                _debuggerStepEvent.Set(); // If we're stuck on the Step wait
                Thread.Sleep(10);
                _debuggerStepEvent.Reset(); // If we're stuck on the Step wait
                Thread.Sleep(10);
                _debuggerSyncEvent.Set();
                return true;
            }

            if (KeywordMatches("pause", command))
            {
                SetRunMode(RunMode.Paused);
                _debuggerSyncEvent.Reset();
                _debuggerStepEvent.Set();
                return true;
            }

            if (KeywordMatches("help", command) || command == "?")
            {
                DisplayHelp();
                return true;
            }

            return false;
        }
        // Allow shortcuts by just typing the start of the keyword
        private bool KeywordMatches(string keyword, string value, int minLength = 1)
        {
            keyword = keyword.Trim().ToUpper();
            value = value.Trim().ToUpper();

            if (value.Length > keyword.Length || value.Length < minLength)
            {
                return false;
            }

            return keyword.StartsWith(value);
        }
        private bool DisplayHelp()
        {
            _logger.LogInformation(
                @"
    Debugger Help
    Definitions:
        <hex byte>                  1-2 hex digits defining an 8-bit value. No hex specifier required. E.g. 7F.
        <hex addr>                  1-4 hex digits defining a 16-bit value or a label name. No hex specifier required. E.g. E7B0
        <hex range>                 A range of hex addresses defined as:
            <hex addr>-<hex addr>     The range of 16-bit values, inclusive. E.g. E7B0-E8FF
            <hex addr>:<n>            A block of addresses from <hex addr> of <n> bytes. <n> is decimal. E.g. E7B0:16

    Keywords listed in UPPERCASE can be shortened, as long as they remain unique in their context. E.g. P for PAUSE. 
    Note potential clashes, e.g. DELETE and DISABLE. These can be shorted to DE and DI, respectively.

    Keywords and hex values are case-insensitive. They are shown here in CAPS for clarity.

    Commands:
        HELP (or ?)                 Show this help information
        GO                          Run the CPU at the current program counter
        PAUSE                       Pause the CPU after the current instruction
        STEP                        Run a single CPU instruction
        QUIT                        Terminate the running program

    Inspection:
        ?A|X|Y                      Show the specified CPU register
        ?PC                         Show the current Program Counter
        ?SP                         Show the current Stack Pointer
        ?C|Z|I|D|B|B2|V|N           Show the specified CPU status flag
        ?<hex addr>                 Show the byte at the specified address
        ?&<hex addr>                Show the 16-bit word at the specified address
        ?<hex range>                Show the memory in the specified address range

    Assignment:
        A|X|Y=<hex byte>            Set the specified CPU register
        PC=<hex addr>               Set the Program Counter to the specified address
        SP=<hex byte>               Set the Stack Pointer (offset) to the specified value
        C|Z|I|D|B|B2|V|N=0|1        Set the specified CPU status flag to 0 or 1
        <hex addr>=<hex byte>       Set the specified byte
        &<hex addr>=<hex word>      Set the specified word

    Breakpoints and Watches:
    Breakpoints cause the debugger to stop when the CPU attempts to execute an instruction
    at the specified address.
        LIST BREAKPOINTS            List breakpoints and their IDs
        ADD BREAKPOINT <hex addr>   Add a breakpoint
        DELETE BREAKPOINT <id>      Delete a breakpoint
        DISABLE BREAKPOINT <id>     Disable a breakpoint
        ENABLE BREAKPOINT <id>      Enable a disabled breakpoint
        CLEAR BREAKPOINTS           Delete all breakpoints
    Watches cause the debugger to display the new state of memory when it changes.
        LIST WATCHES                List watches and their IDs
        ADD WATCH <hex range>       Add a watch
        DELETE WATCH <id>           Delete a watch
        DISABLE WATCH <id>          Disable a watch
        ENABLE WATCH <id>           Enable a disabled watch
        CLEAR WATCHES               Delete all watches
    Combined breakpoint and watch operations:
        LIST ALL                    List breakpoints and watches
        CLEAR ALL                   Delete all breakpoints and watches

    Labels:
        LIST LABELS                 List the labels from the Label Table
");

            return true;
        }
        private bool ParseList(string command)
        {
            var instruction = command.Before(" ");

            if (!KeywordMatches("list", instruction))
            {
                return false;
            }

            var remain = command.After(" ").Trim().ToLower();

            if (remain.Length == 0 || KeywordMatches("all", remain))
            {
                ListAll();
                return true;
            }

            if (KeywordMatches("breakpoints", remain))
            {
                ListBreakpoints();
                return true;
            }

            if (KeywordMatches("watches", remain))
            {
                ListWatches();
                return true;
            }

            return false;
        }

        private void ListAll()
        {
            ListBreakpoints();
            ListWatches();
        }

        private void ListBreakpoints()
        {
            foreach (var breakpoint in _cpuDebug.Breakpoints)
            {
                _formatter.Log(breakpoint.Description);
            }
        }

        private void ListWatches()
        {

        }
        private bool ParseAddBreakpoint(string command)
        {
            // We already know we're in a ADD
            var resource = command.Before(" ");

            if (!KeywordMatches("breakpoint", resource))
            {
                return false;
            }

            return ParseAddBreakpointAddress(command.After(" "));
        }
        private bool ParseAddBreakpointAddress(string command)
        {
            command = command.Trim();
            ushort address;

            if (TryParseAddress(command, out address))
            {
                _cpuDebug.AddBreakpoint(new ProgramAddressBreakpoint(address));
                return true;
            }

            return false;
        }

        private bool ParseAdd(string command)
        {
            var instruction = command.Before(" ");

            if (!KeywordMatches("add", instruction))
            {
                return false;
            }

            if (ParseAddBreakpoint(command.After(" ")))
            {
                return true;
            }

            return false;
        }
        private bool ParseDeleteBreakpoint(string command)
        {
            // We already know we're in a ADD
            var resource = command.Before(" ");

            if (!KeywordMatches("breakpoint", resource))
            {
                return false;
            }

            return ParseDeleteBreakpointAddress(command.After(" "));
        }
        private bool ParseDeleteBreakpointAddress(string command)
        {
            command = command.Trim();
            ushort address;
            var result = false;
            if (TryParseAddress(command, out address))
            {
                ProgramAddressBreakpoint breakpoint;

                do
                {
                    breakpoint = (ProgramAddressBreakpoint)_cpuDebug.Breakpoints.FirstOrDefault(x => (x as ProgramAddressBreakpoint)?.Address == address);

                    if (breakpoint != null)
                    {
                        if (_cpuDebug.DeleteBreakpoint(breakpoint))
                        {
                            result = true;
                        }
                    }
                }
                while (breakpoint != null);
                return result;
            }

            return false;
        }

        private bool ParseDelete(string command)
        {
            var instruction = command.Before(" ");

            if (!KeywordMatches("delete", instruction))
            {
                return false;
            }

            if (ParseDeleteBreakpoint(command.After(" ")))
            {
                return true;
            }

            return false;
        }
        private bool ParsePeek(string command)
        {
            if (!command.StartsWith('?'))
            {
                return false;
            }

            var expression = command.After("?").Trim();

            if (PeekRegister(expression))
            {
                return true;
            }

            if (PeekMemory(expression))
            {
                return true;
            }

            return false;
        }

        private bool PeekRegister(string expression)
        {
            ushort value;
            string hexValue;

            if (TryEvaluateRegister(expression, out value, out hexValue))
            {
                _formatter.LogRegister(expression, value, hexValue);
                return true;
            }

            return false;
        }

        private bool PeekMemory(string expression)
        {
            if (PeekMemoryRange(expression))
            {
                return true;
            }

            if (PeekMemoryWord(expression))
            {
                return true;
            }

            return PeekMemoryByte(expression);
        }

        private bool PeekMemoryRange(string expression)
        {
            var ix = expression.IndexOf("-");

            if (ix < 0)
            {
                return PeekMemoryBlock(expression);
            }

            var begin = expression.Before("-");
            var end = expression.After("-");

            ushort beginAddress;
            ushort endAddress;

            if (TryParseAddress(begin, out beginAddress) && TryParseAddress(end, out endAddress))
            {
                LogMemory(beginAddress, endAddress);
                return true;
            }

            return false;
        }

        private bool PeekMemoryBlock(string expression)
        {
            var ix = expression.IndexOf(":");

            if (ix < 0)
            {
                return false;
            }

            var begin = expression.Before(":");
            var size = expression.After(":");

            ushort beginAddress;
            ushort sizeValue;

            if (TryParseAddress(begin, out beginAddress) && TryParseSize(size, out sizeValue))
            {
                LogMemory(beginAddress, beginAddress + sizeValue - 1);
                return true;
            }

            return false;
        }

        private bool PeekMemoryWord(string expression)
        {
            if (!expression.StartsWith("&"))
            {
                return false;
            }

            var addressExpression = expression.After("&");
            ushort address;

            if (TryParseAddress(addressExpression, out address))
            {
                LogWord(address);
                return true;
            }

            return false;
        }

        private bool PeekMemoryByte(string expression)
        {
            ushort address;

            if (TryParseAddress(expression, out address))
            {
                LogByte(address);
                return true;
            }

            return false;
        }

        private void LogByte(ushort address)
        {
            var value = _memoryDebug.Read(address);
            _formatter.LogByte(value);
        }
        private void LogWord(ushort address)
        {
            var value = _memoryDebug.ReadWord(address);
            _formatter.LogWord(address, value);
        }
        private void LogMemory(int startAddress, int endAddress)
        {
            _formatter.LogBytes((ushort)startAddress, _memoryDebug.ReadBlock((ushort)startAddress, (ushort)endAddress));
        }
        private bool TryParseAddress(string expression, out ushort address)
        {
            expression = expression.Trim();

            if (_labels.TryLookup(expression, out address))
            {
                return true;
            }

            uint value;
            if (uint.TryParse(expression, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value))
            {
                if (value >= 0 && value < 0x10000)
                {
                    address = (ushort)value;
                    return true;
                }
            }

            return false;
        }
        private bool TryParseSize(string expression, out ushort size)
        {
            expression = expression.Trim();

            uint value;

            if (uint.TryParse(expression, out value))
            {
                if (value >= 0 && value < 0x10000)
                {
                    size = (ushort)value;
                    return true;
                }
            }

            size = 0;
            return false;
        }
        private bool TryEvaluateRegister(string expression, out ushort value, out string hexValue)
        {
            switch (expression.ToUpper())
            {
                case "A":
                    value = _cpuDebug.A;
                    hexValue = $"{value:X2}";
                    break;
                case "X":
                    value = _cpuDebug.X;
                    hexValue = $"{value:X2}";
                    break;
                case "Y":
                    value = _cpuDebug.Y;
                    hexValue = $"{value:X2}";
                    break;
                case "SP":
                    value = _cpuDebug.SP;
                    hexValue = $"{value:X2}";
                    break;
                case "PC":
                    value = _cpuDebug.PC;
                    hexValue = $"{value:X4}";
                    break;
                case "C":
                    value = _cpuDebug.C ? 1 : 0;
                    hexValue = $"{value}";
                    break;
                case "Z":
                    value = _cpuDebug.Z ? 1 : 0;
                    hexValue = $"{value}";
                    break;
                case "D":
                    value = _cpuDebug.D ? 1 : 0;
                    hexValue = $"{value}";
                    break;
                case "I":
                    value = _cpuDebug.I ? 1 : 0;
                    hexValue = $"{value}";
                    break;
                case "V":
                    value = _cpuDebug.V ? 1 : 0;
                    hexValue = $"{value}";
                    break;
                case "N":
                    value = _cpuDebug.N ? 1 : 0;
                    hexValue = $"{value}";
                    break;
                case "B":
                    value = _cpuDebug.B ? 1 : 0;
                    hexValue = $"{value}";
                    break;
                case "B2":
                    value = _cpuDebug.B2 ? 1 : 0;
                    hexValue = $"{value}";
                    break;
                case "VERBOSITY":
                    value = (ushort)_cpuDebug.Verbosity;
                    hexValue = $"{value}";
                    break;
                default:
                    value = 0;
                    hexValue = string.Empty;
                    return false;
            }

            return true;
        }
    }
}