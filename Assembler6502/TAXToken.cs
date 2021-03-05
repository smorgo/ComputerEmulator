using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class TAXToken : OpcodeToken
    {
        public TAXToken(Token sourceToken) : base("TAX", sourceToken)
        {
        }

        public override void Emit(ILoader loader)
        {
            loader.TAX();
        }
    }

}