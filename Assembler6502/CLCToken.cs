using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class CLCToken : OpcodeToken
    {
        public CLCToken(Token sourceToken) : base("CLC", sourceToken)
        {
        }
        public override void Emit(ILoader loader)
        {
            loader.CLC();
        }
    }

}