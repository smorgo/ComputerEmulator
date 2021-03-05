namespace Assembler6502
{
    public class NumberToken : Token
    {
        public override bool ProvidesByte => true;
        public override bool ProvidesWord => true;
        public NumberToken(int lineNumber, int lineOffset) : base(lineNumber, lineOffset)
        {}
    }
}