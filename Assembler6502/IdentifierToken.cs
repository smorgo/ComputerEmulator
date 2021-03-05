namespace Assembler6502
{
    public class IdentifierToken : Token
    {
        public override bool ProvidesLabel => true;
        public IdentifierToken(int lineNumber, int lineOffset) : base(lineNumber, lineOffset)
        {
        }

        public override string AsString()
        {
            return Value.ToUpper();
        }
    }
}