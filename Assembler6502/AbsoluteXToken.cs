using HardwareCore;

namespace Assembler6502
{
    public class AbsoluteXToken : AbsoluteToken
    {
        public AbsoluteXToken(Token addressToken) : base(addressToken)
        {
        }
    }
}