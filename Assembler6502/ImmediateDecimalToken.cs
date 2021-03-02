namespace Assembler6502
{
    public class ImmediateCharToken : ImmediateToken
    {
        public override bool ProvidesByte => true;
        public ImmediateCharToken(CharLiteralToken sourceToken) :
            base(sourceToken)
        {
        }
        public override byte AsByte()
        {
            return SourceToken.AsByte();
        }
    }
    public class ImmediateDecimalToken : ImmediateToken
    {
        public override bool ProvidesByte => true;
        public override bool ProvidesWord => true;
        public ImmediateDecimalToken(DecimalNumberToken sourceToken) :
            base(sourceToken)
        {
        }
        public override ushort AsWord()
        {
            return (ushort)(int.Parse(Value));
        }
    }
}