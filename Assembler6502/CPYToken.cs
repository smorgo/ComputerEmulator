using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class CPYToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public CPYToken(Token sourceToken) : base("CPY", sourceToken)
        {
        }
        public override void Emit(ILoader loader)
        {
            if(OperandToken is ImmediateToken && OperandToken.ProvidesByte)
            {
                loader.CPY_IMMEDIATE(OperandToken.AsByte());
            }
            else if(OperandToken is ImmediateToken && OperandToken.ProvidesLabel)
            {
                loader.CPY_IMMEDIATE(OperandToken.AsString());
            }
            else if((OperandToken is AbsoluteToken || OperandToken is NumberToken) && OperandToken.ProvidesWord)
            {
                if(OperandToken.AsWord() < 0x100)
                {
                    loader.CPY_ZERO_PAGE(OperandToken.AsByte());
                }
                else
                {
                    loader.CPY_ABSOLUTE(OperandToken.AsWord());
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
                        loader.CPY_ZERO_PAGE(label);
                    }
                    else
                    {
                        loader.CPY_ABSOLUTE(label);
                    }
                }
                else
                {
                    loader.CPY_ABSOLUTE(label);
                }
            }
            else
            {
                DidNotEmit();
            }
        }
     }

}