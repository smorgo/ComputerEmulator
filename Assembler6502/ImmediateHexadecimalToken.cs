namespace Assembler6502
{
    public class ImmediateHexadecimalToken : ImmediateToken
    {
        public override bool ProvidesByte => true;
        public override bool ProvidesWord => true;
        public ImmediateHexadecimalToken(HexadecimalNumberToken sourceToken) :
            base(sourceToken)
        {
        }

        public override ushort AsWord()
        {
            return (ushort)int.Parse(Value, System.Globalization.NumberStyles.HexNumber);
        }
    }

}