using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class TXSToken : OpcodeToken
    {
        public TXSToken(Token sourceToken) : base("TXS", sourceToken)
        {
        }

        public override void Emit(ILoader loader)
        {
            loader.TXS();
        }
    }

}