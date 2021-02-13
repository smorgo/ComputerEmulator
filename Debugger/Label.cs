namespace Debugger
{
    public class Label
    {
        public string Name {get; private set;}
        public ushort Address {get; private set;}

        public Label(string name, ushort address)
        {
            Name = name;
            Address = address;            
        }
    }
}