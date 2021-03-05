using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class TSXToken : OpcodeToken
    {
        public TSXToken(Token sourceToken) : base("TSX", sourceToken)
        {
        }

        public override void Emit(ILoader loader)
        {
            loader.TSX();
        }
    }

}