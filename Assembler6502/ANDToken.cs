using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class ANDToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public ANDToken(Token sourceToken) : base("AND", sourceToken)
        {
        }

        public override void Emit(ILoader loader)
        {
            if(OperandToken is ImmediateToken && OperandToken.ProvidesByte)
            {
                loader.AND_IMMEDIATE(OperandToken.AsByte());
            }
            else if(OperandToken is ImmediateToken && OperandToken.ProvidesLabel)
            {
                loader.AND_IMMEDIATE(OperandToken.AsString());
            }
            else if(OperandToken is AbsoluteXToken && OperandToken.ProvidesWord)
            {
                if(OperandToken.AsWord() < 0x100)
                {
                    loader.AND_ZERO_PAGE_X(OperandToken.AsByte());
                }
                else
                {
                    loader.AND_ABSOLUTE_X(OperandToken.AsWord());
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
                        loader.AND_ZERO_PAGE_X(label);
                    }
                    else
                    {
                        loader.AND_ABSOLUTE_X(label);
                    }
                }
                else
                {
                    loader.AND_ABSOLUTE_X(label);
                }
            }
            else if(OperandToken is AbsoluteYToken && OperandToken.ProvidesWord)
            {
                loader.AND_ABSOLUTE_Y(OperandToken.AsWord());
            }
            else if(OperandToken is AbsoluteYToken && OperandToken.ProvidesLabel)
            {
                var label = OperandToken.AsString();
                loader.AND_ABSOLUTE_Y(label);
            }
            else if((OperandToken is AbsoluteToken || OperandToken is NumberToken) && OperandToken.ProvidesWord)
            {
                if(OperandToken.AsWord() < 0x100)
                {
                    loader.AND_ZERO_PAGE(OperandToken.AsByte());
                }
                else
                {
                    loader.AND_ABSOLUTE(OperandToken.AsWord());
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
                        loader.AND_ZERO_PAGE(label);
                    }
                    else
                    {
                        loader.AND_ABSOLUTE(label);
                    }
                }
                else
                {
                    loader.AND_ABSOLUTE(label);
                }
            }
            else if(OperandToken is IndirectXToken && OperandToken.ProvidesByte)
            {
                loader.AND_INDIRECT_X(OperandToken.AsByte());
            }
            else if(OperandToken is IndirectXToken && OperandToken.ProvidesLabel)
            {
                loader.AND_INDIRECT_X(OperandToken.AsString());
            }
            else if(OperandToken is IndirectYToken && OperandToken.ProvidesByte)
            {
                loader.AND_INDIRECT_Y(OperandToken.AsByte());
            }
            else if(OperandToken is IndirectYToken && OperandToken.ProvidesLabel)
            {
                loader.AND_INDIRECT_Y(OperandToken.AsString());
            }
            else
            {
                DidNotEmit();
            }

        }
    }

}