namespace Assembler6502
{
    public abstract class OpcodeToken : Token
    {
        public string Opcode {get; private set;}
        public Token SourceToken {get; private set;}
        public Token OperandToken {get; set;}
        public virtual bool HasOperand => false;
        public OpcodeToken(string opcode, Token sourceToken) 
            : base(sourceToken.LineNumber, sourceToken.LineOffset)
        {
            Opcode = opcode;
            SourceToken = sourceToken;
        }
    }

}