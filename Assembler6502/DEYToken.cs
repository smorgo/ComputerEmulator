namespace Assembler6502
{
    public class DEYToken : OpcodeToken
    {
        public DEYToken(Token sourceToken) : base("DEY", sourceToken)
        {
        }
    }

}