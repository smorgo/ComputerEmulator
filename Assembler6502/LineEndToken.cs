namespace Assembler6502
{
    public class LineEndToken : Token
    {
        public LineEndToken(int lineNumber, int lineOffset) : base(lineNumber, lineOffset)
        {
        }
    }
}