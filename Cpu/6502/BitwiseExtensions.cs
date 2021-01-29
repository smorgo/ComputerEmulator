using System;

namespace _6502
{
    public static class BitwiseExtensions
    {
        public static byte Lsb(this ushort value)
        {
            return (byte)(value & 0xff);
        }

        public static byte Msb(this ushort value)
        {
            return (byte)(value >> 8);
        }

        public static ushort UshortFromBytes(byte msb, byte lsb)
        {
            return (ushort)((msb << 8) | lsb);
        }

        public static byte Bit(this byte value, int bit)
        {
            var mask = (byte)Math.Pow(2, bit);

            return (value & mask) == 0 ? 0 : 1;
        }
    }
}
