using System;

namespace _6502
{
    public struct CpuFlags
    {
        public bool C;
        public bool Z;
        public bool I;
        public bool D;
        public bool B;
        public bool B2;
        public bool V;
        public bool N;

        public void Set(byte flags)
        {
            C = 1 == (flags & 1);
            Z = 2 == (flags & 2);
            I = 4 == (flags & 4);
            D = 8 == (flags & 8);
            B = 16 == (flags & 16);
            B2 = 32 == (flags & 32);
            V = 64 == (flags & 64);
            N = 128 == (flags & 128);
        }

        public byte AsByte()
        {
            return (byte)(
                (C ? 1 : 0) +
                (Z ? 2 : 0) +
                (I ? 4 : 0) + 
                (D ? 8 : 0) +
                (B ? 16 : 0) +
                (B2 ? 32 : 0) +
                (V ? 64 : 0) +
                (N ? 128 : 0));
        }

        public override string ToString()
        {
            var c = C ? "C" : ".";
            var z = Z ? "Z" : ".";
            var i = I ? "I" : ".";
            var d = D ? "D" : ".";
            var b = B ? "B" : ".";
            var v = V ? "V" : ".";
            var n = N ? "N" : ".";

            return $"{c}{z}{i}{d}{b}.{v}{n}";
        }
    }
}
