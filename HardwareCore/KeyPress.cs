namespace HardwareCore
{
    public class KeyPress
    {
        public string Key {get; private set;}
        public int Id {get; private set;}

        public KeyPress(string key, int id)
        {
            Key = key;
            Id = id;
        }
    }


}