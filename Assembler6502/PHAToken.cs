namespace Assembler6502
{
    public class PHAToken : OpcodeToken
    {
        public PHAToken(Token sourceToken) : base("PHA", sourceToken)
        {
        }
    }

}