namespace Assembler6502
{
    public class SEIToken : OpcodeToken
    {
        public SEIToken(Token sourceToken) : base("SEI", sourceToken)
        {
        }
    }

}