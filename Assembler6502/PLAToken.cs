namespace Assembler6502
{
    public class PLAToken : OpcodeToken
    {
        public PLAToken(Token sourceToken) : base("PLA", sourceToken)
        {
        }
    }

}