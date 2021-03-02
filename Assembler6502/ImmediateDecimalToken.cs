namespace Assembler6502
{
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