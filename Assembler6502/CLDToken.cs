namespace Assembler6502
{
    public class CLDToken : OpcodeToken
    {
        public CLDToken(Token sourceToken) : base("CLD", sourceToken)
        {
        }
    }

}