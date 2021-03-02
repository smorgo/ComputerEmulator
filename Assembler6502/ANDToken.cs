namespace Assembler6502
{
    public class ANDToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public ANDToken(Token sourceToken) : base("AND", sourceToken)
        {
        }
    }

}