using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class JSRToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public JSRToken(Token sourceToken) : base("JSR", sourceToken)
        {
        }
        public override void Emit(ILoader loader)
        {
            if((OperandToken is AbsoluteToken || OperandToken is NumberToken) && OperandToken.ProvidesWord)
            {
                loader.JSR(OperandToken.AsWord());
            }
            else if((OperandToken is AbsoluteToken  || OperandToken is IdentifierToken) && OperandToken.ProvidesLabel)
            {
                loader.JSR(OperandToken.AsString());    
            }
            else
            {
                DidNotEmit();
            }
         }
     }

}