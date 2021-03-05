using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class EORToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public EORToken(Token sourceToken) : base("EOR", sourceToken)
        {
        }
        public override void Emit(ILoader loader)
        {
            if(OperandToken is ImmediateToken && OperandToken.ProvidesByte)
            {
                loader.EOR_IMMEDIATE(OperandToken.AsByte());
            }
            else if(OperandToken is ImmediateToken && OperandToken.ProvidesLabel)
            {
                loader.EOR_IMMEDIATE(OperandToken.AsString());
            }
            else if(OperandToken is AbsoluteXToken && OperandToken.ProvidesWord)
            {
                if(OperandToken.AsWord() < 0x100)
                {
                    loader.EOR_ZERO_PAGE_X(OperandToken.AsByte());
                }
                else
                {
                    loader.EOR_ABSOLUTE_X(OperandToken.AsWord());
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
                        loader.EOR_ZERO_PAGE_X(label);
                    }
                    else
                    {
                        loader.EOR_ABSOLUTE_X(label);
                    }
                }
                else
                {
                    loader.EOR_ABSOLUTE_X(label);
                }
            }
            else if(OperandToken is AbsoluteYToken && OperandToken.ProvidesWord)
            {
                loader.EOR_ABSOLUTE_Y(OperandToken.AsWord());
            }
            else if(OperandToken is AbsoluteYToken && OperandToken.ProvidesLabel)
            {
                var label = OperandToken.AsString();
                loader.EOR_ABSOLUTE_Y(label);
            }
            else if((OperandToken is AbsoluteToken || OperandToken is NumberToken) && OperandToken.ProvidesWord)
            {
                if(OperandToken.AsWord() < 0x100)
                {
                    loader.EOR_ZERO_PAGE(OperandToken.AsByte());
                }
                else
                {
                    loader.EOR_ABSOLUTE(OperandToken.AsWord());
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
                        loader.EOR_ZERO_PAGE(label);
                    }
                    else
                    {
                        loader.EOR_ABSOLUTE(label);
                    }
                }
                else
                {
                    loader.EOR_ABSOLUTE(label);
                }
            }
            else if(OperandToken is IndirectXToken && OperandToken.ProvidesByte)
            {
                loader.EOR_INDIRECT_X(OperandToken.AsByte());
            }
            else if(OperandToken is IndirectXToken && OperandToken.ProvidesLabel)
            {
                loader.EOR_INDIRECT_X(OperandToken.AsString());
            }
            else if(OperandToken is IndirectYToken && OperandToken.ProvidesByte)
            {
                loader.EOR_INDIRECT_Y(OperandToken.AsByte());
            }
            else if(OperandToken is IndirectYToken && OperandToken.ProvidesLabel)
            {
                loader.EOR_INDIRECT_Y(OperandToken.AsString());
            }
            else
            {
                DidNotEmit();
            }
        }
     }
}