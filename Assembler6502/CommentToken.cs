namespace Assembler6502
{
    public class CommentToken : Token
    {
        public CommentToken(int lineNumber, int lineOffset) : base(lineNumber, lineOffset)
        {
        }
    }
}