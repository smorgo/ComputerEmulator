namespace Assembler6502
{
    public class CharLiteralToken : Token
    {
        public override bool ProvidesByte => true;
        public CharLiteralToken(int lineNumber, int lineOffset) : base(lineNumber, lineOffset)
        {

        }

        public override byte AsByte()
        {
            return (byte)(Value[0]);
        }
    }
}