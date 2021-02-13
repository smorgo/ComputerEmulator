namespace Repl
{
    public interface IEmulatorConsole
    {
        void OpenFile();
        bool Quit();
        void SaveFile();
        void Start();
        void WriteLine(string message);
    }
}