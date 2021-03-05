using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class PHPToken : OpcodeToken
    {
        public PHPToken(Token sourceToken) : base("PHP", sourceToken)
        {
        }

        public override void Emit(ILoader loader)
        {
            loader.PHP();
        }
    }

}