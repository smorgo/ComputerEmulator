namespace Assembler6502
{
    public class SECToken : OpcodeToken
    {
        public SECToken(Token sourceToken) : base("SEC", sourceToken)
        {
        }
    }

}