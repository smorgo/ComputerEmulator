namespace Assembler6502
{
    public class PLPToken : OpcodeToken
    {
        public PLPToken(Token sourceToken) : base("PLP", sourceToken)
        {
        }
    }

}