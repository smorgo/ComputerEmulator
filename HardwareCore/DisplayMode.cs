namespace HardwareCore
{
    public class DisplayMode
    {
        public enum RenderType
        {
            Text,
            Bitmapped
        }

        public byte Mode {get; private set;}
        public RenderType Type {get; private set;}
        public int ColourDepth {get; private set;}
        public int Width {get; private set;}
        public int Height {get; private set;}

        public DisplayMode(byte mode, RenderType type, int colourDepth, int width, int height)
        {
            Mode = mode;
            Type = type;
            ColourDepth = colourDepth;
            Width = width;
            Height = height;
        }

        public static DisplayMode Mode7 = new DisplayMode(0x07, RenderType.Text, 1, 40, 25);
    }
}