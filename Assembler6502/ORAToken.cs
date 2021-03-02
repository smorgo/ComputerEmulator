namespace Assembler6502
{
    public class ORAToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public ORAToken(Token sourceToken) : base("ORA", sourceToken)
        {
        }
    }

}