namespace Assembler6502
{
    public class CLIToken : OpcodeToken
    {
        public CLIToken(Token sourceToken) : base("CLI", sourceToken)
        {
        }
    }

}