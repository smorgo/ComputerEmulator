using System;

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
        public byte BytesPerCharacter
        {
            get
            {
                if( Type == RenderType.Text )
                {
                    return 1;
                }

                // Assume character is 8x8 pixels
                return (byte)(Math.Pow(2, ColourDepth - 1) * 8);
            }
        }
        
        public ushort BytesPerRow => (ushort)(Width * BytesPerCharacter);

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