namespace Assembler6502
{
    public class HexadecimalNumberToken : Token
    {
        public override bool ProvidesByte => true;
        public override bool ProvidesWord => true;
        public HexadecimalNumberToken(int lineNumber, int lineOffset) : base(lineNumber, lineOffset)
        {
        }
        public override ushort AsWord()
        {
            return (ushort)int.Parse(Value, System.Globalization.NumberStyles.HexNumber);
        }
    }
}