namespace Assembler6502
{
    public class INXToken : OpcodeToken
    {
        public INXToken(Token sourceToken) : base("INX", sourceToken)
        {
        }
    }

}