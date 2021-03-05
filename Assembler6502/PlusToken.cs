namespace Assembler6502
{
    public class PlusToken : Token
    {
        public override string Value { get => "+"; }
        public PlusToken(int lineNumber, int lineOffset) : base(lineNumber, lineOffset)
        {
            
        }
    }

}