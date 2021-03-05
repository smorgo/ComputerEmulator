using _6502;
using HardwareCore;

namespace Assembler6502
{
    public class RTIToken : OpcodeToken
    {
        public RTIToken(Token sourceToken) : base("RTI", sourceToken)
        {
        }

        public override void Emit(ILoader loader)
        {
            loader.RTI();
        }
    }

}