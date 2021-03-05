using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class LDXToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public LDXToken(Token sourceToken) : base("LDX", sourceToken)
        {
        }
        public override void Emit(ILoader loader)
        {
            if(OperandToken is ImmediateToken && OperandToken.ProvidesByte)
            {
                loader.LDX_IMMEDIATE(OperandToken.AsByte());
            }
            else if(OperandToken is ImmediateToken && OperandToken.ProvidesLabel)
            {
                loader.LDX_IMMEDIATE(OperandToken.AsString());
            }
            else if(OperandToken is AbsoluteYToken && OperandToken.ProvidesWord)
            {
                loader.LDX_ABSOLUTE_Y(OperandToken.AsWord());
            }
            else if(OperandToken is AbsoluteYToken && OperandToken.ProvidesLabel)
            {
                var label = OperandToken.AsString();
                loader.LDX_ABSOLUTE_Y(label);
            }
            else if((OperandToken is AbsoluteToken || OperandToken is NumberToken) && OperandToken.ProvidesWord)
            {
                if(OperandToken.AsWord() < 0x100)
                {
                    loader.LDX_ZERO_PAGE(OperandToken.AsByte());
                }
                else
                {
                    loader.LDX_ABSOLUTE(OperandToken.AsWord());
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
                        loader.LDX_ZERO_PAGE(label);
                    }
                    else
                    {
                        loader.LDX_ABSOLUTE(label);
                    }
                }
                else
                {
                    loader.LDX_ABSOLUTE(label);
                }
            }
            else
            {
                DidNotEmit();
            }
        }
     }
}