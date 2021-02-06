using System;
using System.Collections.Generic;
using System.Text;
using Debugger;

namespace Debugger
{
    public class DebugLogFormatter : ILogFormatter
    {
        public List<string> Output {get; private set;}
        private Labels _labels;
    
        public DebugLogFormatter(Labels labels)
        {
            _labels = labels;
            Output = new List<string>();
        }

        public void LogBytes(ushort startAddress, byte[] bytes)
        {
            var rows = new List<LogRow>();

            var ix = startAddress;

            var current = new LogRow(ix);
            string label;

            for(var jx = 0; jx < bytes.Length; jx++)
            {
                if(current.IsFull)
                {
                    Output.Add(current.ToString());
                    current = new LogRow(ix);
                    if(_labels.TryLookup(ix, out label))
                    {
                        current.Label = label;
                    }
                } 
                else if(_labels.TryLookup(ix, out label))
                {
                    if(!current.IsEmpty)
                    {
                        Output.Add(current.ToString());
                        current = new LogRow(ix);
                    }

                    current.Label = label;
                }

                current.Append(bytes[jx]);

                ix++;
            }

            if(!current.IsEmpty)
            {
                Output.Add(current.ToString());
            }
        }

        public void LogRegister(string register, ushort value, string hexValue)
        {
            Output.Add($"{register} = ${hexValue} ({value})");
        }

        public void LogByte(byte value)
        {
            Output.Add($"${value:X2} ({value})");
        }

        public void LogWord(ushort address, ushort value)
        {
            Output.Add($"$[{address:X4}] = ${value:X4} ({value})");
        }

        public void LogError(string message)
        {
            Output.Add($"ERROR: {message}");
        }
        public void Log(string message)
        {
            Output.Add(message);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            var first = true;
            foreach(var row in Output)
            {
                if(!first)
                {
                    builder.Append(Environment.NewLine);
                }
                else
                {
                    first = false;
                }

                builder.Append(row);
            }

            return builder.ToString();
        }
    }
}
