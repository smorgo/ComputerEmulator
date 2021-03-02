namespace Assembler6502
{
    public class BVSToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public BVSToken(Token sourceToken) : base("BVS", sourceToken)
        {
        }
    }

}