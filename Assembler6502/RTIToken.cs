namespace Assembler6502
{
    public class RTIToken : OpcodeToken
    {
        public RTIToken(Token sourceToken) : base("RTI", sourceToken)
        {
        }
    }

}