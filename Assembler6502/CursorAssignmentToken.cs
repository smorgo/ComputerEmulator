using HardwareCore;

namespace Assembler6502
{
    public class CursorAssignmentToken : Token
    {
        public Token AddressToken {get; private set;}

        public CursorAssignmentToken(Token addressToken) : base(addressToken.LineNumber, addressToken.LineOffset)
        {
            AddressToken = addressToken;
        }

        public override void Emit(ILoader loader)
        {
            loader.From(AddressToken.AsWord());
        }
    }

}