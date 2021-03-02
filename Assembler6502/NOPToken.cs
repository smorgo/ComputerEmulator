namespace Assembler6502
{
    public class NOPToken : OpcodeToken
    {
        public NOPToken(Token sourceToken) : base("NOP", sourceToken)
        {
        }
    }

}