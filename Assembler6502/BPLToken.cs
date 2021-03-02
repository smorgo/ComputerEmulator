namespace Assembler6502
{
    public class BPLToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public BPLToken(Token sourceToken) : base("BPL", sourceToken)
        {
        }
    }

}