namespace Assembler6502
{
    public class StringLiteralToken : Token
    {
        public StringLiteralToken(int lineNumber, int lineOffset) : base(lineNumber, lineOffset)
        {
        }

        public override string AsString()
        {
            return Value;
        }
    }
}