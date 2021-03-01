namespace Assembler6502
{
    public class SyntaxErrorToken : Token
    {
        public string Error {get; private set;}
        public SyntaxErrorToken(int lineNumber, int lineOffset, string value, string error) : base(lineNumber, lineOffset)
        {
            Value = value;
            Error = error;
        }
    }
}