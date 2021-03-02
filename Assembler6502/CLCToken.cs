namespace Assembler6502
{
    public class CLCToken : OpcodeToken
    {
        public CLCToken(Token sourceToken) : base("CLC", sourceToken)
        {
        }
    }

}