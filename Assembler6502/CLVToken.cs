using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class CLVToken : OpcodeToken
    {
        public CLVToken(Token sourceToken) : base("CLV", sourceToken)
        {
        }
        public override void Emit(ILoader loader)
        {
            loader.CLV();
        }
    }

}