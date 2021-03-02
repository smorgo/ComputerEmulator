namespace Assembler6502
{
    public class BRKToken : OpcodeToken
    {
        public BRKToken(Token sourceToken) : base("BRK", sourceToken)
        {
        }
    }

}