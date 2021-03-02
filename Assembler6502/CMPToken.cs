namespace Assembler6502
{
    public class CMPToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public CMPToken(Token sourceToken) : base("CMP", sourceToken)
        {
        }
    }

}