namespace Assembler6502
{
    public class DecimalNumberToken : NumberToken
    {
        public DecimalNumberToken(int lineNumber, int lineOffset) : base(lineNumber, lineOffset)
        {
            
        }
        public override ushort AsWord()
        {
            return (ushort)(int.Parse(Value));
        }
    }

}