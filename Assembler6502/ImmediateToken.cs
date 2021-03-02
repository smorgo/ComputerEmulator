namespace Assembler6502
{
    public abstract class ImmediateToken : Token
    {
        public Token SourceToken {get; private set;}
        public ImmediateToken(Token sourceToken) :
            base(sourceToken.LineNumber, sourceToken.LineOffset)
        {
            SourceToken = sourceToken;
        }

        public override string Value 
        { 
            get
            {
                return SourceToken.Value;
            }
        }
    }

}