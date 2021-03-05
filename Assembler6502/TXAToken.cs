using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class TXAToken : OpcodeToken
    {
        public TXAToken(Token sourceToken) : base("TXA", sourceToken)
        {
        }

        public override void Emit(ILoader loader)
        {
            loader.TXA();
        }
    }

}