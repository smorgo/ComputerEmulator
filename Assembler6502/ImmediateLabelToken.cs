namespace Assembler6502
{
    public class ImmediateLabelToken : ImmediateToken
    {
        public override bool ProvidesLabel => true;
        public ImmediateLabelToken(IdentifierToken sourceToken) :
            base(sourceToken)
        {
        }
    }

}