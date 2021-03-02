namespace Assembler6502
{
    public class CPYToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public CPYToken(Token sourceToken) : base("CPY", sourceToken)
        {
        }
    }

}