using System;
using HardwareCore;
namespace _6502
{
    public class CpuFlags
    {
        private bool _c;
        public bool C
        {
            get
            {
                return _c;
            }
            set
            {
                if(value != _c)
                {
                    _c = value;
                    _tracker.PostRegisterUpdated(nameof(C), value ? 1 : 0);
                }
            }
        }
        private bool _z;
        public bool Z
        {
            get
            {
                return _z;
            }
            set
            {
                if(value != _z)
                {
                    _z = value;
                    _tracker.PostRegisterUpdated(nameof(Z), value ? 1 : 0);
                }
            }
        }
        private bool _i;
        public bool I
        {
            get
            {
                return _i;
            }
            set
            {
                if(value != _i)
                {
                    _i = value;
                    _tracker.PostRegisterUpdated(nameof(I), value ? 1 : 0);
                }
            }
        }
        private bool _d;
        public bool D
        {
            get
            {
                return _d;
            }
            set
            {
                if(value != _d)
                {
                    _d = value;
                    _tracker.PostRegisterUpdated(nameof(D), value ? 1 : 0);
                }
            }
        }
        private bool _b;
        public bool B
        {
            get
            {
                return _b;
            }
            set
            {
                if(value != _b)
                {
                    _b = value;
                    _tracker.PostRegisterUpdated(nameof(B), value ? 1 : 0);
                }
            }
        }
        private bool _b2;
        public bool B2
        {
            get
            {
                return _b2;
            }
            set
            {
                if(value != _b2)
                {
                    _b2 = value;
                    _tracker.PostRegisterUpdated(nameof(B2), value ? 1 : 0);
                }
            }
        }
        private bool _v;
        public bool V
        {
            get
            {
                return _v;
            }
            set
            {
                if(value != _v)
                {
                    _v = value;
                    _tracker.PostRegisterUpdated(nameof(V), value ? 1 : 0);
                }
            }
        }
        private bool _n;
        public bool N
        {
            get
            {
                return _n;
            }
            set
            {
                if(value != _n)
                {
                    _n = value;
                    _tracker.PostRegisterUpdated(nameof(N), value ? 1 : 0);
                }
            }
        }

        private IRegisterTracker _tracker;
        public CpuFlags(IRegisterTracker tracker)
        {
            _tracker = tracker;
        }
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
