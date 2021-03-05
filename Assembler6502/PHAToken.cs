using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class PHAToken : OpcodeToken
    {
        public PHAToken(Token sourceToken) : base("PHA", sourceToken)
        {
        }

        public override void Emit(ILoader loader)
        {
            loader.PHA();
        }
    }

}