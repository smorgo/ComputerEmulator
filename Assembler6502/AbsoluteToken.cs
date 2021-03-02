namespace Assembler6502
{
    public abstract class AbsoluteToken : Token
    {
        public override bool ProvidesByte => AddressToken.ProvidesByte;
        public override bool ProvidesWord => AddressToken.ProvidesWord;
        public override bool ProvidesLabel => AddressToken.ProvidesLabel;
        public override string Value => AddressToken.Value;
        public Token AddressToken {get; private set;}
        public AbsoluteToken(Token addressToken) : base(addressToken.LineNumber, addressToken.LineOffset)
        {
            AddressToken = addressToken;
        }
    }
}