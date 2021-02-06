using System.Collections.Generic;
using System.Text;

namespace Debugger
{

    public class LogRow
    {
        const int MaxBytes = 16;
        public string Label {get; set;}
        public ushort Address {get; private set;}
        public List<byte> Bytes {get; private set;}
        public bool IsEmpty => Bytes.Count == 0;
        public bool IsFull => Bytes.Count >= MaxBytes;

        public LogRow(ushort address)
        {
            Bytes = new List<byte>();
            Label = string.Empty;
            Address = address;
        }

        public void Append(byte value)
        {
            Bytes.Add(value);
        }
        public string Hex 
        {
            get
            {
                var buffer = new StringBuilder();
                foreach(var value in Bytes)
                {
                    buffer.Append($"{value:X2} ");
                }

                var hexLength = MaxBytes * 3 - 1;
                
                return buffer.ToString().PadRight(hexLength).Substring(0,hexLength);
            }
        }
        public string Text
        {
            get
            {
                var buffer = new StringBuilder();
                foreach(var value in Bytes)
                {
                    var ch = (char)value;

                    if(ch < ' ' || ch > 0x7F)
                    {
                        ch = '.';
                    }

                    buffer.Append(ch);
                }

                return buffer.ToString().PadRight(MaxBytes).Substring(0,MaxBytes);
            }
        }

        public string FormattedLabel
        {
            get
            {
                return (string.IsNullOrEmpty(Label) ? "" : (Label + ":")).PadRight(16).Substring(0,16);
            }
        }

        public override string ToString()
        {
            return $"[{Address:X4}] {FormattedLabel} {Hex} [{Text}]";
        }
    }
}
