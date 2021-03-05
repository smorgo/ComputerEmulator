using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class STAToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public STAToken(Token sourceToken) : 
            base("STA", sourceToken)
        {
        }

        public override void Emit(ILoader loader)
        {
            if(OperandToken is AbsoluteXToken)
            {
                if(OperandToken.ProvidesWord)
                {
                    var address = OperandToken.AsWord();
                    if( address < 0x100 )
                    {
                        loader.STA_ZERO_PAGE_X(OperandToken.AsByte());
                    }
                    else
                    {
                        loader.STA_ABSOLUTE_X(OperandToken.AsWord());
                    }
                }
                else if(OperandToken.ProvidesLabel)
                {
                    var label = OperandToken.AsString();
                    ushort address;
                    if(loader.TryResolveLabel(label, out address))
                    {
                        if(address < 0x100)
                        {
                            loader.STA_ZERO_PAGE_X(label);
                        }
                        else
                        {
                            loader.STA_ABSOLUTE_X(label);
                        }
                    }
                    else
                    {
                        loader.STA_ABSOLUTE_X(label);
                    }
                }
            }
            else if(OperandToken is AbsoluteYToken)
            {
                if(OperandToken.ProvidesWord)
                {
                    loader.STA_ABSOLUTE_Y(OperandToken.AsWord());
                }
                else if(OperandToken.ProvidesLabel)
                {
                    loader.STA_ABSOLUTE_Y(OperandToken.AsString());
                }
            }
            else if(OperandToken is AbsoluteToken || OperandToken is IdentifierToken || OperandToken is NumberToken)
            {
                if(OperandToken.ProvidesWord)
                {
                    var address = OperandToken.AsWord();
                    if( address < 0x100 )
                    {
                        loader.STA_ZERO_PAGE(OperandToken.AsByte());
                    }
                    else
                    {
                        loader.STA_ABSOLUTE(OperandToken.AsWord());
                    }
                }
                else if(OperandToken.ProvidesLabel)
                {
                    var label = OperandToken.AsString();
                    ushort address;
                    if(loader.TryResolveLabel(label, out address))
                    {
                        if(address < 0x100)
                        {
                            loader.STA_ZERO_PAGE(label);
                        }
                        else
                        {
                            loader.STA_ABSOLUTE(label);
                        }
                    }
                    else
                    {
                        loader.STA_ABSOLUTE(label);
                    }
                }
            }
            else if(OperandToken is IndirectXToken)
            {
                if(OperandToken.ProvidesByte)
                {
                    loader.STA_INDIRECT_X(OperandToken.AsByte());
                }
                else
                {
                    loader.STA_INDIRECT_X(OperandToken.AsString());
                }
            }
            else if(OperandToken is IndirectYToken)
            {
                if(OperandToken.ProvidesByte)
                {
                    loader.STA_INDIRECT_Y(OperandToken.AsByte());
                }
                else
                {
                    loader.STA_INDIRECT_Y(OperandToken.AsString());
                }
            }
            else
            {
                DidNotEmit();
            }
        }
    }
}