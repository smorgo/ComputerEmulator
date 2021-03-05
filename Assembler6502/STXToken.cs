using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class STXToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public STXToken(Token sourceToken) : base("STX", sourceToken)
        {
        }
        public override void Emit(ILoader loader)
        {
            if(OperandToken is AbsoluteToken || OperandToken is IdentifierToken || OperandToken is NumberToken)
            {
                if(OperandToken.ProvidesWord)
                {
                    var address = OperandToken.AsWord();
                    if( address < 0x100 )
                    {
                        loader.STX_ZERO_PAGE(OperandToken.AsByte());
                    }
                    else
                    {
                        loader.STX_ABSOLUTE(OperandToken.AsWord());
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
                            loader.STX_ZERO_PAGE(label);
                        }
                        else
                        {
                            loader.STX_ABSOLUTE(label);
                        }
                    }
                    else
                    {
                        loader.STX_ABSOLUTE(label);
                    }
                }
            }
            else
            {
                DidNotEmit();
            }
        }
    }
}