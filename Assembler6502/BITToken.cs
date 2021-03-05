using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class BITToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public BITToken(Token sourceToken) : base("BIT", sourceToken)
        {
        }

        public override void Emit(ILoader loader)
        {
            if((OperandToken is AbsoluteToken || OperandToken is NumberToken) && OperandToken.ProvidesWord)
            {
                if(OperandToken.AsWord() < 0x100)
                {
                    loader.BIT_ZERO_PAGE(OperandToken.AsByte());
                }
                else
                {
                    loader.BIT_ABSOLUTE(OperandToken.AsWord());
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
                        loader.BIT_ZERO_PAGE(label);
                    }
                    else
                    {
                        loader.BIT_ABSOLUTE(label);
                    }
                }
                else
                {
                    loader.BIT_ABSOLUTE(label);
                }
            }
            else
            {
                DidNotEmit();
            }
        }
    }

}