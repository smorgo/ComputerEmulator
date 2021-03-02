namespace Assembler6502
{
    public class JSRToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public JSRToken(Token sourceToken) : base("JSR", sourceToken)
        {
        }
    }

}