using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class STYToken : OpcodeToken
    {
        public override bool HasOperand => true;
        public STYToken(Token sourceToken) : base("STY", sourceToken)
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
                        loader.STY_ZERO_PAGE(OperandToken.AsByte());
                    }
                    else
                    {
                        loader.STY_ABSOLUTE(OperandToken.AsWord());
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
                            loader.STY_ZERO_PAGE(label);
                        }
                        else
                        {
                            loader.STY_ABSOLUTE(label);
                        }
                    }
                    else
                    {
                        loader.STY_ABSOLUTE(label);
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