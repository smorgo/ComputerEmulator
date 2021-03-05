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
            IsValid = true;
        }

        public virtual bool DefaultToAccumulatorOperand => false;

        public virtual bool IsValid {get; protected set;}
        public virtual void Emit(ILoader loader)
        {
            // Do nothing
            DidNotEmit();
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
            // Override this if you don't want it in uppercase
            return Value.ToUpper();
        }

        protected virtual void DidNotEmit()
        {
            IsValid = false;
        }

        public override string ToString()
        {
            return $"Line {LineNumber} column {LineOffset} {this.GetType().Name}";
        }
    }
}