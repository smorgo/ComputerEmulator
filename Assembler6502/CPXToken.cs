using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class CPXToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public CPXToken(Token sourceToken) : base("CPX", sourceToken)
        {
        }
        public override void Emit(ILoader loader)
        {
            if(OperandToken is ImmediateToken && OperandToken.ProvidesByte)
            {
                loader.CPX_IMMEDIATE(OperandToken.AsByte());
            }
            else if(OperandToken is ImmediateToken && OperandToken.ProvidesLabel)
            {
                loader.CPX_IMMEDIATE(OperandToken.AsString());
            }
            else if((OperandToken is AbsoluteToken || OperandToken is NumberToken) && OperandToken.ProvidesWord)
            {
                if(OperandToken.AsWord() < 0x100)
                {
                    loader.CPX_ZERO_PAGE(OperandToken.AsByte());
                }
                else
                {
                    loader.CPX_ABSOLUTE(OperandToken.AsWord());
                }
            }
            else if((OperandToken is AbsoluteToken  || OperandToken is IdentifierToken) && OperandToken.ProvidesLabel)
            {
                ushort address;
                var label = OperandToken.AsString();

                if(loader.TryResolveLabel(label, out address))
                {
                    if(address < 0x100)
                    {
                        loader.CPX_ZERO_PAGE(label);
                    }
                    else
                    {
                        loader.CPX_ABSOLUTE(label);
                    }
                }
                else
                {
                    loader.CPX_ABSOLUTE(label);
                }
            }
            else
            {
                DidNotEmit();
            }
        }
     }

}