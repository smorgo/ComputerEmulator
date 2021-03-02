namespace Assembler6502
{
    public class INCToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public INCToken(Token sourceToken) : base("INC", sourceToken)
        {
        }
    }

}