using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class LSRToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public override bool DefaultToAccumulatorOperand => true;
        public LSRToken(Token sourceToken) : base("LSR", sourceToken)
        {
        }
        public override void Emit(ILoader loader)
        {
            if(OperandToken is IdentifierToken && OperandToken.Value == "A")
            {
                loader.LSR_ACCUMULATOR();
            }
            else if(OperandToken is AbsoluteXToken && OperandToken.ProvidesWord)
            {
                if(OperandToken.AsWord() < 0x100)
                {
                    loader.LSR_ZERO_PAGE_X(OperandToken.AsByte());
                }
                else
                {
                    loader.LSR_ABSOLUTE_X(OperandToken.AsWord());
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
                        loader.LSR_ZERO_PAGE_X(label);
                    }
                    else
                    {
                        loader.LSR_ABSOLUTE_X(label);
                    }
                }
                else
                {
                    loader.LSR_ABSOLUTE_X(label);
                }
            }
            else if((OperandToken is AbsoluteToken || OperandToken is NumberToken) && OperandToken.ProvidesWord)
            {
                if(OperandToken.AsWord() < 0x100)
                {
                    loader.LSR_ZERO_PAGE(OperandToken.AsByte());
                }
                else
                {
                    loader.LSR_ABSOLUTE(OperandToken.AsWord());
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
                        loader.LSR_ZERO_PAGE(label);
                    }
                    else
                    {
                        loader.LSR_ABSOLUTE(label);
                    }
                }
                else
                {
                    loader.LSR_ABSOLUTE(label);
                }
            }
            else
            {
                DidNotEmit();
            }
        }
    }
}