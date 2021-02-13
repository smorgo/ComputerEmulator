namespace HardwareCore
{
    public class RegisterUpdatedEventArgs
    {
        public string Register {get; private set;}
        public ushort Value {get; private set;}

        public RegisterUpdatedEventArgs(string register, ushort value)
        {
            Register = register;
            Value = value;
        }
    }
}