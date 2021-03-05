namespace Assembler6502
{
    public class AbsoluteToken : Token
    {
        public override bool ProvidesByte => AddressToken.ProvidesByte;
        public override bool ProvidesWord => AddressToken.ProvidesWord;
        public override bool ProvidesLabel => AddressToken.ProvidesLabel;
        public Token AddressToken {get; private set;}
        public Token OffsetDirectionToken {get; private set;}
        public Token OffsetToken {get; private set;}
        public AbsoluteToken(Token addressToken) : this(addressToken, null, null)
        {}

        public AbsoluteToken(Token addressToken, Token offsetDirectionToken, Token offsetToken) 
            : base(addressToken.LineNumber, addressToken.LineOffset)
        {
            AddressToken = addressToken;
            OffsetDirectionToken = offsetDirectionToken;
            OffsetToken = offsetToken;
        }

        public override string Value
        {
            get
            {
                if(AddressToken.ProvidesLabel && OffsetDirectionToken != null)
                {
                    return $"{AddressToken.Value}{OffsetDirectionToken.Value}{OffsetToken.Value}";
                }
                else
                {
                        return AddressToken.Value;
                }
            }
        }
    }
}