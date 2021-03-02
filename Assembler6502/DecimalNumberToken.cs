namespace Assembler6502
{
    public class DecimalNumberToken : Token
    {
        public override bool ProvidesByte => true;
        public override bool ProvidesWord => true;
        public DecimalNumberToken(int lineNumber, int lineOffset) : base(lineNumber, lineOffset)
        {
            
        }
        public override ushort AsWord()
        {
            return (ushort)(int.Parse(Value));
        }
    }

}