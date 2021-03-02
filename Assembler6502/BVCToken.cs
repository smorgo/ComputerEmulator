namespace Assembler6502
{
    public class BVCToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public BVCToken(Token sourceToken) : base("BVC", sourceToken)
        {
        }
    }

}