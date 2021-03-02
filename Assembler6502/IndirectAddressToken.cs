namespace Assembler6502
{
    public class IndirectAddressToken : Token
    {
        public override bool ProvidesByte => AddressToken.ProvidesByte;
        public override bool ProvidesWord => AddressToken.ProvidesWord;
        public override bool ProvidesLabel => AddressToken.ProvidesLabel;
        public Token AddressToken {get; private set;}

        public IndirectAddressToken(Token addressToken) : base(addressToken.LineNumber, addressToken.LineOffset)
        {
            AddressToken = addressToken;
        }
    }

}