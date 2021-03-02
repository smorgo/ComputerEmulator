namespace Assembler6502
{
    public class ROLToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public ROLToken(Token sourceToken) : base("ROL", sourceToken)
        {
        }
    }

}