namespace Assembler6502
{
    public class DECToken : OpcodeToken
    {
        public DECToken(Token sourceToken) : base("DEC", sourceToken)
        {
        }
    }

}