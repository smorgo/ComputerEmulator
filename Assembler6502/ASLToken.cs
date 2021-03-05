using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class ASLToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public override bool DefaultToAccumulatorOperand => false;
        public ASLToken(Token sourceToken) : base("ASL", sourceToken)
        {
        }

        public override void Emit(ILoader loader)
        {
            if(OperandToken is AbsoluteXToken && OperandToken.ProvidesWord)
            {
                if(OperandToken.AsWord() < 0x100)
                {
                    loader.ASL_ZERO_PAGE_X(OperandToken.AsByte());
                }
                else
                {
                    loader.ASL_ABSOLUTE_X(OperandToken.AsWord());
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
                        loader.ASL_ZERO_PAGE_X(label);
                    }
                    else
                    {
                        loader.ASL_ABSOLUTE_X(label);
                    }
                }
                else
                {
                    loader.ASL_ABSOLUTE_X(label);
                }
            }
            else if((OperandToken is AbsoluteToken || OperandToken is NumberToken) && OperandToken.ProvidesWord)
            {
                if(OperandToken.AsWord() < 0x100)
                {
                    loader.ASL_ZERO_PAGE(OperandToken.AsByte());
                }
                else
                {
                    loader.ASL_ABSOLUTE(OperandToken.AsWord());
                }
            }
            else if(OperandToken is IdentifierToken && OperandToken.Value.ToUpper() == "A")
            {
                loader.ASL_ACCUMULATOR();
            }
            else if((OperandToken is AbsoluteToken  || OperandToken is IdentifierToken) && OperandToken.ProvidesLabel)
            {
                ushort address;
                var label = OperandToken.AsString();

                if(loader.TryResolveLabel(label, out address))
                {
                    if(address < 0x100)
                    {
                        loader.ASL_ZERO_PAGE(label);
                    }
                    else
                    {
                        loader.ASL_ABSOLUTE(label);
                    }
                }
                else
                {
                    loader.ASL_ABSOLUTE(label);
                }
            }
            else
            {
                DidNotEmit();
            }
        }
    }

}