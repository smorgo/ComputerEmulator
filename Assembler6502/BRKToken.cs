using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class BRKToken : OpcodeToken
    {
        public BRKToken(Token sourceToken) : base("BRK", sourceToken)
        {
        }
        public override void Emit(ILoader loader)
        {
            loader.BRK();
        }
    }

}