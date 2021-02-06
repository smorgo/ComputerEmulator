using System;

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
         * list breakpoint|watch
         * add breakpoint|watch <breakpoint expression>
         * delete breakpoint|watch <expression>
         * disable breakpoint|watch <expression>
         * enable breakpoint|watch <expression>
         * clear breakpoint|watch <expression>
         * clear all
         * go
         * step
         * stop
         * help
         *
         * <expression> = <address>, <address range>, <word address>
         * <address> = <label name>, <hex>, <int>
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
            _formatter.LogBytes(address, new byte[] { value });
        }
        private void LogWord(ushort address)
        {
            var value = _memoryDebug.ReadWord(address);
            _formatter.LogWord(address, value);
        }
        private void LogMemory(int startAddress, int endAddress)
        {
            if(startAddress < endAddress)
            {
                var temp = startAddress;
                startAddress = endAddress;
                endAddress = temp;
            }

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

            if(uint.TryParse($"0x{expression}", out value))
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
                case "DEBUG":
                    value = (ushort)_cpuDebug.Debug;
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