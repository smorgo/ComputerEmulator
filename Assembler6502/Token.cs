using HardwareCore;

namespace Assembler6502
{
    public abstract class Token
    {
        public virtual string Value {get; set;}
        public int LineNumber {get; private set;}
        public int LineOffset {get; private set;}
        public Token(int lineNumber, int lineOffset)
        {
            LineNumber = lineNumber;
            LineOffset = lineOffset;
        }

        public virtual bool IsValid => true;
        public virtual void Emit(ILoader loader)
        {
            // Do nothing
        }

        public virtual bool ProvidesByte => false;
        public virtual bool ProvidesWord => false;
        public virtual bool ProvidesLabel => false;

        public virtual byte AsByte()
        {
            return (byte)AsWord();
        }
        public virtual ushort AsWord()
        {
            return 0x0000;
        }
        public virtual string AsString()
        {
            return Value;
        }
    }
}