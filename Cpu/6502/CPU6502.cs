using System;
using System.Diagnostics;

namespace _6502
{

    public class CPU6502
    {
        public enum OPCODE
        {
            BCC = 0x90,
            BCS = 0xB0,
            BEQ = 0xF0,
            BIT_ZERO_PAGE = 0x24,
            BIT_ABSOLUTE = 0x2C,
            BMI = 0x30,
            BNE = 0xD0,
            BPL = 0x10,
            BRK = 0x00,
            BVC = 0x50,
            BVS = 0x70,
            CLC = 0x18,
            CLD = 0xD8,
            CLI = 0x58,
            CLV = 0xB8,
            CMP_IMMEDIATE = 0xC9,
            CMP_ZERO_PAGE = 0xC5,
            CMP_ZERO_PAGE_X = 0xD5,
            CMP_ABSOLUTE = 0xCD,
            CMP_ABSOLUTE_X = 0xDD,
            CMP_ABSOLUTE_Y = 0xD9,
            CMP_INDIRECT_X = 0xC1,
            CMP_INDIRECT_Y = 0xD1,
            JMP_ABSOLUTE = 0x4C,
            JMP_INDIRECT = 0x6C,
            JSR = 0x20,
            LDA_IMMEDIATE = 0xA9,
            LDA_ZERO_PAGE = 0xA5,
            LDA_ZERO_PAGE_X = 0xB5,
            LDA_ABSOLUTE = 0xAD,
            LDA_ABSOLUTE_X = 0xBD,
            LDA_INDIRECT_X = 0xA1,
            LDA_INDIRECT_Y = 0xB1,
            LDX_IMMEDIATE = 0xA2,
            LDX_ZERO_PAGE = 0xA6,
            LDX_ZERO_PAGE_Y = 0xB6,
            LDX_ABSOLUTE = 0xAE,
            LDX_ABSOLUTE_Y = 0xBE,
            LDY_IMMEDIATE = 0xA0,
            LDY_ZERO_PAGE = 0xA4,
            LDY_ZERO_PAGE_X = 0xB4,
            LDY_ABSOLUTE = 0xAC,
            LDY_ABSOLUTE_X = 0xBC,
            NOP = 0xEA,
            PHA = 0x48,
            PHP = 0x08,
            PLA = 0x68,
            PLP = 0x28,
            RTS = 0x60,
            SEC = 0x38,
            SED = 0xF8,
            SEI = 0x78,
            STA_ZERO_PAGE = 0x85,
            STA_ZERO_PAGE_X = 0x95,
            STA_ABSOLUTE = 0x8D,
            STA_ABSOLUTE_X = 0x9D,
            STA_ABSOLUTE_Y = 0x99,
            STA_INDIRECT_X = 0x81,
            STA_INDIRECT_Y = 0x91,
            STX_ZERO_PAGE = 0x86,
            STX_ZERO_PAGE_Y = 0x96,
            STX_ABSOLUTE = 0x8E,
            STY_ZERO_PAGE = 0x84,
            STY_ZERO_PAGE_X = 0x94,
            STY_ABSOLUTE = 0x8C,
            TAX = 0xAA,
            TAY = 0xA8,
            TSX = 0xBA,
            TXA = 0x8A,
            TXS = 0x9A,
            TYA = 0x98
        }

        public static UInt32 MAX_MEMORY = 0x10000;
        public static ushort STACK_BASE = 0x100;
        public ushort PC;
        public Byte SP;
        public Byte A;
        public Byte X;
        public Byte Y;
        public CpuFlags P = new CpuFlags();
        public DebugLevel DebugLevel {get;set;} = DebugLevel.Errors;
        private AddressMap _addressMap;

        
        public CPU6502(AddressMap addressMap)
        {
            _addressMap = addressMap;    
            LoadOpCodes();
        }

        private void Log(DebugLevel level, string message)
        {
            if((int)level <= (int)DebugLevel)
            {
                Debug.WriteLine(message);
                Console.WriteLine(message);
            }
        }

        private void StartLogInstruction(ushort pc)
        {
            _logPC = pc;
            _logParams = "";
        }
        
        private void EndLogInstruction(OPCODE opcode)
        {
            LogInstruction(opcode, _logPC, _logParams);
        }

        private void LogParams(string logParams)
        {
            _logParams = logParams;
        }

        private void LogInstruction(OPCODE opcode, ushort pc, string logParams)
        {
            var instruction = (opcode.ToString() + " " + logParams + new string(' ', 40)).Substring(0,40);

            Log(DebugLevel.Trace, $"[{pc:X4}] {instruction} -> PC:{PC:X4} A:{A:X2} X:{X:X2} Y:{Y:X2} SP:{SP:X2} P:{P}");
        }

        private void LoadOpCodes()
        {
            for(var ix = 0; ix < 256; ix++)
            {
                OpCodeTable[ix] = null;
            }

            // Yuck!
            OpCodeTable[(int)OPCODE.BCC] = BranchOnCarryClear;
            OpCodeTable[(int)OPCODE.BCS] = BranchOnCarrySet;
            OpCodeTable[(int)OPCODE.BEQ] = BranchEquals;
            OpCodeTable[(int)OPCODE.BNE] = BranchNotEquals;
            OpCodeTable[(int)OPCODE.BRK] = Break;
            OpCodeTable[(int)OPCODE.CLC] = ClearCarryFlag;
            OpCodeTable[(int)OPCODE.CMP_IMMEDIATE] = CompareAccumulatorImmediate;
            OpCodeTable[(int)OPCODE.CMP_ZERO_PAGE] = CompareAccumulatorZeroPage;
            OpCodeTable[(int)OPCODE.CMP_ZERO_PAGE_X] = CompareAccumulatorZeroPageX;
            OpCodeTable[(int)OPCODE.CMP_ABSOLUTE] = CompareAccumulatorAbsolute;
            OpCodeTable[(int)OPCODE.CMP_ABSOLUTE_X] = CompareAccumulatorAbsoluteX;
            OpCodeTable[(int)OPCODE.CMP_INDIRECT_X] = CompareAccumulatorIndirectX;
            OpCodeTable[(int)OPCODE.CMP_INDIRECT_Y] = CompareAccumulatorIndirectY;
            OpCodeTable[(int)OPCODE.JMP_ABSOLUTE] = JumpAbsolute;
            OpCodeTable[(int)OPCODE.JMP_INDIRECT] = JumpIndirect;
            OpCodeTable[(int)OPCODE.JSR] = JumpToSubroutine;
            OpCodeTable[(int)OPCODE.LDA_IMMEDIATE] = LoadAccumulatorImmediate;
            OpCodeTable[(int)OPCODE.LDA_ZERO_PAGE] = LoadAccumulatorZeroPage;
            OpCodeTable[(int)OPCODE.LDA_ZERO_PAGE_X] = LoadAccumulatorZeroPageX;
            OpCodeTable[(int)OPCODE.LDA_ABSOLUTE] = LoadAccumulatorAbsolute;
            OpCodeTable[(int)OPCODE.LDA_ABSOLUTE_X] = LoadAccumulatorAbsoluteX;
            OpCodeTable[(int)OPCODE.LDA_INDIRECT_X] = LoadAccumulatorIndirectX;
            OpCodeTable[(int)OPCODE.LDA_INDIRECT_Y] = LoadAccumulatorIndirectY;
            OpCodeTable[(int)OPCODE.LDX_IMMEDIATE] = LoadXImmediate;
            OpCodeTable[(int)OPCODE.LDX_ZERO_PAGE] = LoadXZeroPage;
            OpCodeTable[(int)OPCODE.LDX_ZERO_PAGE_Y] = LoadXZeroPageY;
            OpCodeTable[(int)OPCODE.LDX_ABSOLUTE] = LoadXAbsolute;
            OpCodeTable[(int)OPCODE.LDX_ABSOLUTE_Y] = LoadXAbsoluteY;
            OpCodeTable[(int)OPCODE.LDY_IMMEDIATE] = LoadYImmediate;
            OpCodeTable[(int)OPCODE.LDY_ZERO_PAGE] = LoadYZeroPage;
            OpCodeTable[(int)OPCODE.LDY_ZERO_PAGE_X] = LoadYZeroPageX;
            OpCodeTable[(int)OPCODE.LDY_ABSOLUTE] = LoadYAbsolute;
            OpCodeTable[(int)OPCODE.LDY_ABSOLUTE_X] = LoadYAbsoluteX;
            OpCodeTable[(int)OPCODE.NOP] = NoOperation;
            OpCodeTable[(int)OPCODE.PHA] = PushAccumulator;
            OpCodeTable[(int)OPCODE.PHP] = PushProcessorStatus;
            OpCodeTable[(int)OPCODE.PLA] = PullAccumulator;
            OpCodeTable[(int)OPCODE.PLP] = PullProcessorStatus;
            OpCodeTable[(int)OPCODE.RTS] = ReturnFromSubroutine;
            OpCodeTable[(int)OPCODE.SEC] = SetCarryFlag;
            OpCodeTable[(int)OPCODE.STA_ZERO_PAGE] = StoreAccumulatorZeroPage;
            OpCodeTable[(int)OPCODE.STA_ZERO_PAGE_X] = StoreAccumulatorZeroPageX;
            OpCodeTable[(int)OPCODE.STA_ABSOLUTE] = StoreAccumulatorAbsolute;
            OpCodeTable[(int)OPCODE.STA_ABSOLUTE_X] = StoreAccumulatorAbsoluteX;
            OpCodeTable[(int)OPCODE.STA_ABSOLUTE_Y] = StoreAccumulatorAbsoluteY;
            OpCodeTable[(int)OPCODE.STA_INDIRECT_X] = StoreAccumulatorIndirectX;
            OpCodeTable[(int)OPCODE.STA_INDIRECT_Y] = StoreAccumulatorIndirectY;
            OpCodeTable[(int)OPCODE.STX_ZERO_PAGE] = StoreXZeroPage;
            OpCodeTable[(int)OPCODE.STX_ZERO_PAGE_Y] = StoreXZeroPageY;
            OpCodeTable[(int)OPCODE.STX_ABSOLUTE] = StoreXAbsolute;
            OpCodeTable[(int)OPCODE.STY_ZERO_PAGE] = StoreYZeroPage;
            OpCodeTable[(int)OPCODE.STY_ZERO_PAGE_X] = StoreYZeroPageX;
            OpCodeTable[(int)OPCODE.STY_ABSOLUTE] = StoreYAbsolute;
            OpCodeTable[(int)OPCODE.TAX] = TransferAccumulatorToX;
            OpCodeTable[(int)OPCODE.TAY] = TransferAccumulatorToY;
            OpCodeTable[(int)OPCODE.TSX] = TransferStackPointerToX;
            OpCodeTable[(int)OPCODE.TXA] = TransferXToAccumulator;
            OpCodeTable[(int)OPCODE.TXS] = TransferXToStackPointer;
            OpCodeTable[(int)OPCODE.TYA] = TransferYToAccumulator;
        }
        public void Reset()
        {
            Log(DebugLevel.Information, "\r\n6502 CPU Emulator");
            // Get the reset vector
            PC = _addressMap.ReadWord(0xFFFC);
            Log(DebugLevel.Information, $"RESET VECTOR [FFFC] is [{PC:X4}]");
            SP = 0xFF;
            A = 0x00;
            X = 0x00;
            Y = 0x00;
            P.Set(0);

            Run();
            Log(DebugLevel.Information, "HALT");
        }

        private void Run()
        {
            while(true)
            {
                StartLogInstruction(PC);
                var OpCode = Fetch();

                var handler = OpCodeTable[OpCode];

                if(handler == null)
                {
                    Log(DebugLevel.Warnings, $"OpCode {OpCode} not handled");
                }
                else
                {
                    handler();
                    EndLogInstruction((OPCODE)OpCode);

                    if(P.B)
                    {
                        // Break
                        return;
                    }
                }
            }
        }

        private void Branch(sbyte offset)
        {
            PC = (ushort)(PC + offset); // offset is signed
        }

        private byte Fetch()
        {
            var value = _addressMap.Read(PC);
            PC++;
            return value;
        }
        
        private ushort FetchWord()
        {
            var lsb = Fetch();
            var msb = Fetch();
            var value = (ushort)(lsb + (msb << 8));

            return value;
        }

        private byte Read(ushort address)
        {
            return _addressMap.Read(address);
        }

        private ushort ReadWord(ushort address)
        {
            return _addressMap.ReadWord(address);
        }

        private void Write(ushort address, byte value)
        {
            _addressMap.Write(address, value);
        }

        private void WriteWord(ushort address, ushort value)
        {
            _addressMap.WriteWord(address, value);
        }

        private void LoadAccumulator(byte value)
        {
            A = value;
            P.Z = (A == 0);
            P.N = 128 == (A & 128);
        }

        private void LoadX(byte value)
        {
            X = value;
            P.Z = (X == 0);
            P.N = 128 == (X & 128);
        }

        private void LoadY(byte value)
        {
            Y = value;
            P.Z = (Y == 0);
            P.N = 128 == (Y & 128);
        }

        private void Push(byte value)
        {
            Write((ushort)(STACK_BASE + SP), value);
            if(SP == 0)
            {
                Log(DebugLevel.Errors, "STACK OVERFLOW!");
                Break();
            }
            SP--;
        }
        private void PushWord(ushort value)
        {
            Push(value.Msb());
            Push(value.Lsb());
        }

        private byte Pull()
        {
            if(SP == 0xff)
            {
                Log(DebugLevel.Errors, "STACK UNDERFLOW!");
                Break();
            }
            SP++;
            var value = Read((ushort)(STACK_BASE + SP));
            return value;
        }

        private ushort PullWord()
        {
            var lsb = Pull();
            var msb = Pull();
            return BitwiseExtensions.UshortFromBytes(msb,lsb);
        }

        private void CompareMemory(ushort address, byte value)
        {
            var memoryValue = Read(address);
            Compare(memoryValue, value);
        }

        private void Compare(byte memoryValue, byte value)
        {
            if(memoryValue == value)
            {
                P.N = false;
                P.Z = true;
                P.C = true;
            }
            else
            {
                P.N = (value > 0x7f);
                P.Z = false;
                P.C = (value > memoryValue);
            }
        }

        // OpCode Implementation
        private void BranchOnCarryClear()
        {
            var offset = (sbyte)Fetch();
            LogParams($"{offset}");
            if(!P.C)
            {
                Branch(offset);
            }
        }

        private void BranchOnCarrySet()
        {
            var offset = (sbyte)Fetch();
            LogParams($"{offset}");
            if(P.C)
            {
                Branch(offset);
            }
        }

        private void BranchEquals()
        {
            var offset = (sbyte)Fetch();
            LogParams($"{offset}");
            if(P.Z)
            {
                Branch(offset);
            }
        }
        private void BranchNotEquals()
        {
            var offset = (sbyte)Fetch();
            LogParams($"{offset}");
            if(!P.Z)
            {
                Branch(offset);
            }
        }

        private void Break()
        {
            P.B = true;            
        }
        private void ClearCarryFlag()
        {
            P.C = false;
        }
        private void CompareAccumulatorImmediate()
        {
            var memoryValue = Fetch();
            LogParams($"{memoryValue:X2}");
            Compare(memoryValue, A);
        }

        private void CompareAccumulatorZeroPage()
        {
            var address = Fetch();
            LogParams($"[{address:X2}]");
            CompareMemory(address, A);
        }

        private void CompareAccumulatorZeroPageX()
        {
            var address = Fetch();
            LogParams($"[{address:X2}] + {X:X2}");
            address += X;
            CompareMemory(address, A);
        }

        private void CompareAccumulatorAbsolute()
        {
            var address = FetchWord();
            LogParams($"[{address:X4}]");
            CompareMemory(address, A);
        }

        private void CompareAccumulatorAbsoluteX()
        {
            var address = FetchWord();
            LogParams($"[{address:X4}] + {X:X2}");
            address += X;
            CompareMemory(address, A);
        }
        private void CompareAccumulatorIndirectX()
        {
            var zpOffset = Fetch();
            var address = ReadWord((ushort)((zpOffset + X) & 0xFF));
            LogParams($"({zpOffset:X2}) [{address:X4}]");
            CompareMemory(address,A);
        }

        private void CompareAccumulatorIndirectY()
        {
            var zpOffset = Fetch();
            var address = ReadWord((ushort)((zpOffset + Y) & 0xFF));
            LogParams($"({zpOffset:X2}) [{address:X4}]");
            CompareMemory(address,A);
        }

        private void JumpAbsolute()
        {
            var address = FetchWord();
            LogParams($"[{address:X4}]");
            PC = address;
        }
        private void JumpIndirect()
        {
            var address = FetchWord();
            var target = ReadWord(address);
            LogParams($"({address:X4}) [{target:X4}]");
            PC = target;
        }
        private void JumpToSubroutine()
        {
            var address = FetchWord();
            PushWord(PC);
            LogParams($"[{address:X4}]");
            PC = address;
        }
        private void LoadAccumulatorImmediate()
        {
            var value = Fetch();
            LogParams($"{value:X2}");
            LoadAccumulator(value);
        }

        private void LoadAccumulatorZeroPage()
        {
            var zpOffset = Fetch();
            LogParams($"{zpOffset:X2}");
            LoadAccumulator(Read(zpOffset));
        }

        private void LoadAccumulatorZeroPageX()
        {
            var zpOffset = Fetch();
            LogParams($"{zpOffset:X2} + {X:X2}");
            LoadAccumulator(Read((ushort)(zpOffset + X)));
        }

        private void LoadAccumulatorAbsolute()
        {
            var address = FetchWord();
            LogParams($"{address:X4}");
            LoadAccumulator(Read(address));
        }

        private void LoadAccumulatorAbsoluteX()
        {
            var address = FetchWord();
            LogParams($"{address:X2} + {X:X2}");
            address += X;
            LoadAccumulator(Read(address));
        }

        private void LoadAccumulatorIndirectX()
        {
            var zpOffset = Fetch();
            var address = ReadWord((ushort)((zpOffset + X) & 0xFF));
            LogParams($"({zpOffset:X2}) [{address:X4}]");
            LoadAccumulator(Read(address));
        }

        private void LoadAccumulatorIndirectY()
        {
            var zpOffset = Fetch();
            var address = ReadWord((ushort)((zpOffset + Y) & 0xFF));
            LogParams($"({zpOffset:X2}) [{address:X4}]");
            LoadAccumulator(Read(address));
        }
        private void LoadXImmediate()
        {
            var value = Fetch();
            LogParams($"{value:X2}");
            LoadX(value);
        }
        private void LoadXZeroPage()
        {
            var zpOffset = Fetch();
            LogParams($"{zpOffset:X2}");
            LoadX(Read(zpOffset));
        }
        private void LoadXZeroPageY()
        {
            var zpOffset = Fetch();
            LogParams($"{zpOffset:X2} + {X:X2}");
            zpOffset += Y;
            LoadX(Read(zpOffset));
        }
        private void LoadXAbsolute()
        {
            var address = FetchWord();
            LogParams($"[{address:X4}]");
            LoadX(Read(address));
        }

        private void LoadXAbsoluteY()
        {
            var address = FetchWord();
            LogParams($"[{address:X4}] + {Y:X2}");
            address += Y;
            LoadX(Read(address));
        }
        private void LoadYImmediate()
        {   
            var value = Fetch();
            LogParams($"{value:X2}");
            LoadY(value);
        }
        private void LoadYZeroPage()
        {
            var zpOffset = Fetch();
            LogParams($"[{zpOffset:X2}]");
            LoadY(Read(zpOffset));
        }
        private void LoadYZeroPageX()
        {
            var zpOffset = Fetch();
            LogParams($"[{zpOffset:X2}] + {X:2}");
            zpOffset += X;
            LoadY(Read(zpOffset));
        }
        private void LoadYAbsolute()
        {
            var address = FetchWord();
            LogParams($"[{address:X4}]");
            LoadY(Read(address));
        }
        private void LoadYAbsoluteX()
        {
            var address = FetchWord();
            LogParams($"[{address:X4}] + {X:2}");
            address += X;
            LoadY(Read(address));
        }
        private void NoOperation()
        {
            // Do nothing
        }
        private void PushAccumulator()
        {
            Push(A);
        }

        private void PushProcessorStatus()
        {
            Push(P.AsByte());
        }

        private void PullAccumulator()
        {
            LoadAccumulator(Pull());
        }

        private void PullProcessorStatus()
        {
            P.Set(Pull());
        }
        private void ReturnFromSubroutine()
        {
            var address = PullWord();
            PC = address;
        }

        private void SetCarryFlag()
        {
            P.C = true;
        }

        private void StoreAccumulatorAbsolute()
        {
            var address = FetchWord();
            LogParams($"[{address:X4}]");
            Write(address, A);
        }
        private void StoreAccumulatorZeroPage()
        {
            var address = Fetch();
            LogParams($"[{address:X2}]");
            Write(address, A);
        }
        private void StoreAccumulatorZeroPageX()
        {
            var address = Fetch();
            LogParams($"[{address:X2}] + {X:X2}");
            address += X;
            Write(address, A);
        }
        private void StoreAccumulatorAbsoluteX()
        {
            var address = FetchWord();
            LogParams($"[{address:X4}] + {X:X2}");
            address += X;
            Write(address, A);
        }
        private void StoreAccumulatorAbsoluteY()
        {
            var address = FetchWord();
            LogParams($"[{address:X4}] + {Y:X2}");
            address += Y;
            Write(address, A);
        }
        private void StoreAccumulatorIndirectX()
        {
            var zpOffset = Fetch();
            var address = ReadWord((ushort)((zpOffset + X) & 0xFF));
            LogParams($"({zpOffset:X2},{X:X2}) [{address:X4}]");
            Write(address, A);
        }
        private void StoreAccumulatorIndirectY()
        {
            var zpOffset = Fetch();
            var address = ReadWord((ushort)((zpOffset + Y) & 0xFF));
            LogParams($"({zpOffset:X2},{Y:X2}) [{address:X4}]");
            Write(address, A);
        }

        private void StoreXAbsolute()
        {
            var address = FetchWord();
            LogParams($"[{address:X4}]");
            Write(address, X);
        }
        private void StoreXZeroPage()
        {
            var address = Fetch();
            LogParams($"[{address:X2}]");
            Write(address, X);
        }
        private void StoreXZeroPageY()
        {
            var address = Fetch();
            LogParams($"[{address:X2}] + {Y:X2}");
            address += Y;
            Write(address, X);
        }
        private void StoreYAbsolute()
        {
            var address = FetchWord();
            LogParams($"[{address:X4}]");
            Write(address, Y);
        }
        private void StoreYZeroPage()
        {
            var address = Fetch();
            LogParams($"[{address:X2}]");
            Write(address, Y);
        }
        private void StoreYZeroPageX()
        {
            var address = Fetch();
            LogParams($"[{address:X2}] + {X:X2}");
            address += X;
            Write(address, Y);
        }

        private void TransferAccumulatorToX()
        {
            LoadX(A);
        }

        private void TransferAccumulatorToY()
        {
            LoadY(A);
        }
        private void TransferStackPointerToX()
        {
            LoadX(SP);
        }
        private void TransferXToAccumulator()
        {
            LoadAccumulator(X);
        }
        private void TransferXToStackPointer()
        {
            SP = X;
        }
        private void TransferYToAccumulator()
        {
            LoadAccumulator(Y);
        }

        // OpCode Map
        private Action[] OpCodeTable = new Action[256];
        private ushort _logPC;
        private string _logParams;
    }
}
