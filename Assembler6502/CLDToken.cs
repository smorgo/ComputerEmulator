using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class CLDToken : OpcodeToken
    {
        public CLDToken(Token sourceToken) : base("CLD", sourceToken)
        {
        }
        public override void Emit(ILoader loader)
        {
            loader.CLD();
        }
    }

}