using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class INXToken : OpcodeToken
    {
        public INXToken(Token sourceToken) : base("INX", sourceToken)
        {
        }

        public override void Emit(ILoader loader)
        {
            loader.INX();
        }
    }

}