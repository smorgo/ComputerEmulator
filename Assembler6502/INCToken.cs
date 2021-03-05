using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class INCToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public INCToken(Token sourceToken) : base("INC", sourceToken)
        {
        }
        public override void Emit(ILoader loader)
        {
            if(OperandToken is AbsoluteXToken && OperandToken.ProvidesWord)
            {
                if(OperandToken.AsWord() < 0x100)
                {
                    loader.INC_ZERO_PAGE_X(OperandToken.AsByte());
                }
                else
                {
                    loader.INC_ABSOLUTE_X(OperandToken.AsWord());
                }
            }
            else if(OperandToken is AbsoluteXToken && OperandToken.ProvidesLabel)
            {
                ushort address;
                var label = OperandToken.AsString();

                if(loader.TryResolveLabel(label, out address))
                {
                    if(address < 0x100)
                    {
                        loader.INC_ZERO_PAGE_X(label);
                    }
                    else
                    {
                        loader.INC_ABSOLUTE_X(label);
                    }
                }
                else
                {
                    loader.INC_ABSOLUTE_X(label);
                }
            }
            else if((OperandToken is AbsoluteToken || OperandToken is NumberToken) && OperandToken.ProvidesWord)
            {
                if(OperandToken.AsWord() < 0x100)
                {
                    loader.INC_ZERO_PAGE(OperandToken.AsByte());
                }
                else
                {
                    loader.INC_ABSOLUTE(OperandToken.AsWord());
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
                        loader.INC_ZERO_PAGE(label);
                    }
                    else
                    {
                        loader.INC_ABSOLUTE(label);
                    }
                }
                else
                {
                    loader.INC_ABSOLUTE(label);
                }
            }
            else
            {
                DidNotEmit();
            }
        }
     }
}