namespace Assembler6502
{
    public class TAXToken : OpcodeToken
    {
        public TAXToken(Token sourceToken) : base("TAX", sourceToken)
        {
        }
    }

}