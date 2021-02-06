using System;
using System.Globalization;
using System.Linq;

namespace Debugger
{

    public class Parser
    {
        private readonly ICpuDebug _cpuDebug;
        private readonly IMemoryDebug _memoryDebug;
        private readonly Labels _labels;
        private readonly ILogFormatter _formatter;

        public Parser(ICpuDebug cpuDebug, IMemoryDebug memoryDebug, Labels labels, ILogFormatter formatter)
        {
            _cpuDebug = cpuDebug;
            _memoryDebug = memoryDebug;
            _labels = labels;
            _formatter = formatter;
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
            command = command.Trim();

            if(ParsePeek(command))
            {
                return;
            }

            if(ParseAdd(command))
            {
                return;
            }

            if(ParseDelete(command))
            {
                return;
            }

            if(ParseList(command))
            {
                return;
            }

            _formatter.LogError($"Command not recognise: {command}");
        }

        // Allow shortcuts by just typing the start of the keyword
        private bool KeywordMatches(string keyword, string value, int minLength = 1)
        {
            keyword = keyword.Trim().ToUpper();
            value = value.Trim().ToUpper();

            if(value.Length > keyword.Length || value.Length < minLength)
            {
                return false;
            }

            return keyword.StartsWith(value);
        }
        private bool ParseList(string command)
        {
            var instruction = command.Before(" ");

            if(!KeywordMatches("list", instruction))
            {
                return false;
            }

            var remain = command.After(" ").Trim().ToLower();

            if(remain.Length == 0 || KeywordMatches("all", remain))
            {
                ListAll();
                return true;
            }

            if(KeywordMatches("breakpoints", remain))
            {
                ListBreakpoints();
                return true;
            }

            if(KeywordMatches("watches", remain))
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
            foreach(var breakpoint in _cpuDebug.Breakpoints)
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

            if(!KeywordMatches("breakpoint", resource))
            {
                return false;
            }

            return ParseAddBreakpointAddress(command.After(" "));
        }
        private bool ParseAddBreakpointAddress(string command)
        {
            command = command.Trim();
            ushort address;

            if(TryParseAddress(command, out address))
            {
                _cpuDebug.AddBreakpoint(new ProgramAddressBreakpoint(address));
                return true;
            }

            return false;
        }

        private bool ParseAdd(string command)
        {
            var instruction = command.Before(" ");

            if(!KeywordMatches("add", instruction))
            {
                return false;
            }

            if(ParseAddBreakpoint(command.After(" ")))
            {
                return true;
            }

            return false;
        }
        private bool ParseDeleteBreakpoint(string command)
        {
            // We already know we're in a ADD
            var resource = command.Before(" ");

            if(!KeywordMatches("breakpoint", resource))
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
            if(TryParseAddress(command, out address))
            {
                ProgramAddressBreakpoint breakpoint;

                do
                {
                    breakpoint = (ProgramAddressBreakpoint)_cpuDebug.Breakpoints.FirstOrDefault(x => (x as ProgramAddressBreakpoint)?.Address == address);

                    if(breakpoint != null)
                    {
                        if(_cpuDebug.DeleteBreakpoint(breakpoint))
                        {
                            result = true;
                        }
                    }
                }
                while(breakpoint != null);
                return result;
            }

            return false;
        }

        private bool ParseDelete(string command)
        {
            var instruction = command.Before(" ");

            if(!KeywordMatches("delete", instruction))
            {
                return false;
            }

            if(ParseDeleteBreakpoint(command.After(" ")))
            {
                return true;
            }

            return false;
        }
        private bool ParsePeek(string command)
        {
            if(!command.StartsWith('?'))
            {
                return false;
            }

            var expression = command.After("?").Trim();

            if(PeekRegister(expression))
            {
                return true;
            }

            if(PeekMemory(expression))
            {
                return true;
            }

            return false;
        }

        private bool PeekRegister(string expression)
        {
            ushort value;
            string hexValue;

            if(TryEvaluateRegister(expression, out value, out hexValue))
            {
                _formatter.LogRegister(expression, value, hexValue);
                return true;
            }

            return false;
        }

        private bool PeekMemory(string expression)
        {
            if(PeekMemoryRange(expression))
            {
                return true;
            }

            if(PeekMemoryWord(expression))
            {
                return true;
            }

            return PeekMemoryByte(expression);
        }

        private bool PeekMemoryRange(string expression)
        {
            var ix = expression.IndexOf("-");

            if(ix < 0)
            {
                return PeekMemoryBlock(expression);
            }

            var begin = expression.Before("-");
            var end = expression.After("-");

            ushort beginAddress;
            ushort endAddress;

            if(TryParseAddress(begin, out beginAddress) && TryParseAddress(end, out endAddress))
            {
                LogMemory(beginAddress, endAddress);
                return true;
            }

            return false;
        }

        private bool PeekMemoryBlock(string expression)
        {
            var ix = expression.IndexOf(":");

            if(ix < 0)
            {
                return false;
            }

            var begin = expression.Before(":");
            var size = expression.After(":");

            ushort beginAddress;
            ushort sizeValue;

            if(TryParseAddress(begin, out beginAddress) && TryParseSize(size, out sizeValue))
            {
                LogMemory(beginAddress, beginAddress + sizeValue - 1);
                return true;
            }

            return false;
        }

        private bool PeekMemoryWord(string expression)
        {
            if(!expression.StartsWith("&"))
            {
                return false;
            }

            var addressExpression = expression.After("&");
            ushort address;

            if(TryParseAddress(addressExpression, out address))
            {
                LogWord(address);
                return true;
            }

            return false;
        }

        private bool PeekMemoryByte(string expression)
        {
            ushort address;

            if(TryParseAddress(expression, out address))
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

            if(_labels.TryLookup(expression, out address))
            {
                return true;
            }

            uint value;
            if(uint.TryParse(expression, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value))
            {
                if(value >= 0 && value < 0x10000)
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

            if(uint.TryParse(expression, out value))
            {
                if(value >= 0 && value < 0x10000)
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
            switch(expression.ToUpper())
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
                    value = _cpuDebug.A;
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