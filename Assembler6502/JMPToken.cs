using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class JMPToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public JMPToken(Token sourceToken) : base("JMP", sourceToken)
        {
        }
        public override void Emit(ILoader loader)
        {
            if((OperandToken is AbsoluteToken || OperandToken is NumberToken) && OperandToken.ProvidesWord)
            {
                loader.JMP_ABSOLUTE(OperandToken.AsWord());
            }
            else if((OperandToken is AbsoluteToken  || OperandToken is IdentifierToken) && OperandToken.ProvidesLabel)
            {
                var label = OperandToken.AsString();

                loader.JMP_ABSOLUTE(label);
            }
            else if(OperandToken is IndirectToken && OperandToken.ProvidesByte)
            {
                loader.JMP_INDIRECT(OperandToken.AsWord());
            }
            else if(OperandToken is IndirectToken && OperandToken.ProvidesLabel)
            {
                loader.JMP_INDIRECT(OperandToken.AsString());
            }
            else
            {
                DidNotEmit();
            }
        }
     }

}