namespace Assembler6502
{
    public class BEQToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public BEQToken(Token sourceToken) : base("BEQ", sourceToken)
        {
        }
    }

}