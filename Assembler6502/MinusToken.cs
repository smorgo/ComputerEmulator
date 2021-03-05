namespace Assembler6502
{
    public class MinusToken : Token
    {
        public override string Value { get => "-"; }
        public MinusToken(int lineNumber, int lineOffset) : base(lineNumber, lineOffset)
        {
            
        }
    }

}