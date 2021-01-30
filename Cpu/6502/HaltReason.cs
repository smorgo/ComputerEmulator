namespace _6502
{
    public enum HaltReason
    {
        None = 0,
        Break,
        StackOverflow,
        StackUnderflow
    }
}