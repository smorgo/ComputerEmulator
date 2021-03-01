namespace Assembler6502
{
    public abstract class Token
    {
        public string Value {get; set;}
        public int LineNumber {get; private set;}
        public int LineOffset {get; private set;}
        public Token(int lineNumber, int lineOffset)
        {
            LineNumber = lineNumber;
            LineOffset = lineOffset;
        }
    }
}