namespace Assembler6502
{
    public class JMPToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public JMPToken(Token sourceToken) : base("JMP", sourceToken)
        {
        }
    }

}