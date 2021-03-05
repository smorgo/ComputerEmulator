using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class SEDToken : OpcodeToken
    {
        public SEDToken(Token sourceToken) : base("SED", sourceToken)
        {
        }

        public override void Emit(ILoader loader)
        {
            loader.SED();
        }
    }

}