using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class TYAToken : OpcodeToken
    {
        public TYAToken(Token sourceToken) : base("TYA", sourceToken)
        {
        }

        public override void Emit(ILoader loader)
        {
            loader.TYA();
        }
    }
}