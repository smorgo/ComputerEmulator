using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class DEYToken : OpcodeToken
    {
        public DEYToken(Token sourceToken) : base("DEY", sourceToken)
        {
        }

        public override void Emit(ILoader loader)
        {
            loader.DEY();
        }
    }

}