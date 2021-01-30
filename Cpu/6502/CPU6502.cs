using System;
using System.Diagnostics;

namespace _6502
{

    public class CPU6502
    {
        public enum OPCODE
        {
            ADC_IMMEDIATE = 0x69,
            ADC_ZERO_PAGE = 0x65,
            ADC_ZERO_PAGE_X = 0x75,
            ADC_ABSOLUTE = 0x6D,
            ADC_ABSOLUTE_X = 0x7D,
            ADC_ABSOLUTE_Y = 0x79,
            ADC_INDIRECT_X = 0x61,
            ADC_INDIRECT_Y = 0x71,
            AND_IMMEDIATE = 0x29,
            AND_ZERO_PAGE = 0x25,
            AND_ZERO_PAGE_X = 0x35,
            AND_ABSOLUTE = 0x2D,
            AND_ABSOLUTE_X = 0x3D,
            AND_ABSOLUTE_Y = 0x39,
            AND_INDIRECT_X = 0x21,
            AND_INDIRECT_Y = 0x31,
            ASL_ACCUMULATOR = 0x0A,
            ASL_ZERO_PAGE = 0x06,
            ASL_ZERO_PAGE_X = 0x16,
            ASL_ABSOLUTE = 0x0E,
            ASL_ABSOLUTE_X = 0x1E,
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
            CPX_IMMEDIATE = 0xE0,
            CPX_ZERO_PAGE = 0xE4,
            CPX_ABSOLUTE = 0xEC,
            CPY_IMMEDIATE = 0xC0,
            CPY_ZERO_PAGE = 0xC4,
            CPY_ABSOLUTE = 0xCC,
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

#region Helpers
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
            if(String.IsNullOrEmpty(_logParams))
            {
                _logParams = logParams;
            }
            else
            {
                _logParams += " " + logParams;
            }
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
            OpCodeTable[(int)OPCODE.ADC_IMMEDIATE] = AddWithCarryImmediate;
            OpCodeTable[(int)OPCODE.ADC_ZERO_PAGE] = AddWithCarryZeroPage;
            OpCodeTable[(int)OPCODE.ADC_ZERO_PAGE_X] = AddWithCarryZeroPageX;
            OpCodeTable[(int)OPCODE.ADC_ABSOLUTE] = AddWithCarryAbsolute;
            OpCodeTable[(int)OPCODE.ADC_ABSOLUTE_X] = AddWithCarryAbsoluteX;
            OpCodeTable[(int)OPCODE.ADC_ABSOLUTE_Y] = AddWithCarryAbsoluteY;
            OpCodeTable[(int)OPCODE.ADC_INDIRECT_X] = AddWithCarryIndirectX;
            OpCodeTable[(int)OPCODE.ADC_INDIRECT_Y] = AddWithCarryIndirectY;
            OpCodeTable[(int)OPCODE.AND_IMMEDIATE] = AndImmediate;
            OpCodeTable[(int)OPCODE.AND_ZERO_PAGE] = AndZeroPage;
            OpCodeTable[(int)OPCODE.AND_ZERO_PAGE_X] = AndZeroPageX;
            OpCodeTable[(int)OPCODE.AND_ABSOLUTE] = AndAbsolute;
            OpCodeTable[(int)OPCODE.AND_ABSOLUTE_X] = AndAbsoluteX;
            OpCodeTable[(int)OPCODE.AND_ABSOLUTE_Y] = AndAbsoluteY;
            OpCodeTable[(int)OPCODE.AND_INDIRECT_X] = AndIndirectX;
            OpCodeTable[(int)OPCODE.AND_INDIRECT_Y] = AndIndirectY;
            OpCodeTable[(int)OPCODE.ASL_ACCUMULATOR] = AslAccumulator;
            OpCodeTable[(int)OPCODE.ASL_ZERO_PAGE] = AslZeroPage;
            OpCodeTable[(int)OPCODE.ASL_ZERO_PAGE_X] = AslZeroPageX;
            OpCodeTable[(int)OPCODE.ASL_ABSOLUTE] = AslAbsolute;
            OpCodeTable[(int)OPCODE.ASL_ABSOLUTE_X] = AslAbsoluteX;
            OpCodeTable[(int)OPCODE.BCC] = BranchOnCarryClear;
            OpCodeTable[(int)OPCODE.BCS] = BranchOnCarrySet;
            OpCodeTable[(int)OPCODE.BIT_ZERO_PAGE] = BitTestZeroPage;
            OpCodeTable[(int)OPCODE.BIT_ABSOLUTE] = BitTestAbsolute;
            OpCodeTable[(int)OPCODE.BEQ] = BranchEquals;
            OpCodeTable[(int)OPCODE.BMI] = BranchMinus;
            OpCodeTable[(int)OPCODE.BNE] = BranchNotEquals;
            OpCodeTable[(int)OPCODE.BPL] = BranchPositive;
            OpCodeTable[(int)OPCODE.BRK] = Break;
            OpCodeTable[(int)OPCODE.BVC] = BranchOnOverflowClear;
            OpCodeTable[(int)OPCODE.BVS] = BranchOnOverflowSet;
            OpCodeTable[(int)OPCODE.CLC] = ClearCarryFlag;
            OpCodeTable[(int)OPCODE.CLD] = ClearDecimalFlag;
            OpCodeTable[(int)OPCODE.CLI] = ClearInterruptDisableFlag;
            OpCodeTable[(int)OPCODE.CLV] = ClearOverflowFlag;
            OpCodeTable[(int)OPCODE.CMP_IMMEDIATE] = CompareAccumulatorImmediate;
            OpCodeTable[(int)OPCODE.CMP_ZERO_PAGE] = CompareAccumulatorZeroPage;
            OpCodeTable[(int)OPCODE.CMP_ZERO_PAGE_X] = CompareAccumulatorZeroPageX;
            OpCodeTable[(int)OPCODE.CMP_ABSOLUTE] = CompareAccumulatorAbsolute;
            OpCodeTable[(int)OPCODE.CMP_ABSOLUTE_X] = CompareAccumulatorAbsoluteX;
            OpCodeTable[(int)OPCODE.CMP_INDIRECT_X] = CompareAccumulatorIndirectX;
            OpCodeTable[(int)OPCODE.CMP_INDIRECT_Y] = CompareAccumulatorIndirectY;
            OpCodeTable[(int)OPCODE.CPX_IMMEDIATE] = CompareXImmediate;
            OpCodeTable[(int)OPCODE.CPX_ZERO_PAGE] = CompareXZeroPage;
            OpCodeTable[(int)OPCODE.CPX_ABSOLUTE] = CompareXAbsolute;
            OpCodeTable[(int)OPCODE.CPY_IMMEDIATE] = CompareYImmediate;
            OpCodeTable[(int)OPCODE.CPY_ZERO_PAGE] = CompareYZeroPage;
            OpCodeTable[(int)OPCODE.CPY_ABSOLUTE] = CompareYAbsolute;
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
            OpCodeTable[(int)OPCODE.SED] = SetDecimalFlag;
            OpCodeTable[(int)OPCODE.SEI] = SetInterruptDisableFlag;
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

        private void AddWithCarry(byte value)
        {
            byte v1 = A;
            byte v2 = value;

            var total = v1 + v2;
            
            if(P.C)
            {
                total++;
            }

            byte result = (byte)(total & 0xff);

            LoadAccumulator(result);

            P.C = (total < -128 || total > 127);
            var b1 = v1.Bit(7);
            var b2 = v2.Bit(7);
            var r = result.Bit(7);
            
            P.V = (v1.Bit(7) == v2.Bit(7)) && (v1.Bit(7) != result.Bit(7));
        }

        private void BitTest(ushort address)
        {
            var value = Read(address);
            P.V = (value & 0x40) == 0x40;
            P.N = (value & 0x80) == 0x80;
            P.Z = (value & A) == 0;
        }

        private void Branch(bool condition)
        {
            var offset = (sbyte)Fetch();
            LogParams($"*{offset:+0;-#}");
            if(condition)
            {
                PC = (ushort)(PC + offset); // offset is signed
            }
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

        private ushort FetchAbsoluteAddress()
        {
            var address = FetchWord();
            LogParams($"[${address:X4}]");
            return address;
        }
        private ushort FetchAbsoluteAddressX()
        {
            var address = FetchWord();
            LogParams($"${address:X4},${X:X2}");
            address += X;
            LogParams($"[${address:X4}]");
            return address;
        }
        private ushort FetchAbsoluteAddressY()
        {
            var address = FetchWord();
            LogParams($"${address:X4},${Y:X2}");
            address += Y;
            LogParams($"[${address:X4}]");
            return address;
        }

        private ushort FetchIndirectAddress()
        {
            var vector = FetchWord();
            var address = ReadWord(vector);
            LogParams($"(${vector:X4})");
            LogParams($"[${address:X4}]");
            return address;
        }

        private ushort FetchIndexedIndirectAddressX()
        {
            var vector = Fetch();
            var address = ReadWord((ushort)(vector + X));
            LogParams($"(${vector:X2},{X:X2})");
            LogParams($"[${address:X4}]");
            return address;
        }
        private ushort FetchIndirectIndexedAddressY()
        {
            var vector = Fetch();
            var address = (ushort)(ReadWord(vector) + Y);
            LogParams($"(${vector:X2}),{Y:X2}");
            LogParams($"[${address:X4}]");
            return address;
        }

        private byte FetchZeroPageAddress()
        {
            var address = Fetch();
            LogParams($"[${address:X2}]");
            return address;
        }
        private byte FetchZeroPageAddressX()
        {
            var address = Fetch();
            LogParams($"${address:X2},${X:X2}");
            address += X;
            LogParams($"[${address:X2}]");
            return address;
        }
        private byte FetchZeroPageAddressY()
        {
            var address = Fetch();
            LogParams($"${address:X2},${Y:X2}");
            address += Y;
            LogParams($"[${address:X2}]");
            return address;
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

        private void AndAccumulator(byte value)
        {
            var result = (byte)(A & value);
            LoadAccumulator(result);
        }

        private void Asl(byte value)
        {
            var carry = value.Bit(7);
            value = (byte)(value << 1);
            LoadAccumulator(value);
            P.C = carry != 0;
        }

        private byte FetchImmediate()
        {
            var value = Fetch();
            LogParams($"#${value:X2}");
            return value;
        }
#endregion

        // OpCode Implementation
        private void AddWithCarryImmediate()
        {
            AddWithCarry(FetchImmediate());
        }

        private void AddWithCarryIndirectY()
        {
            AddWithCarry(Read(FetchIndirectIndexedAddressY()));
        }

        private void AddWithCarryIndirectX()
        {
            AddWithCarry(Read(FetchIndexedIndirectAddressX()));
        }

        private void AddWithCarryAbsoluteY()
        {
            AddWithCarry(Read(FetchAbsoluteAddressY()));
        }

        private void AddWithCarryAbsoluteX()
        {
            AddWithCarry(Read(FetchAbsoluteAddressX()));
        }

        private void AddWithCarryAbsolute()
        {
            AddWithCarry(Read(FetchAbsoluteAddress()));
        }

        private void AddWithCarryZeroPageX()
        {
            AddWithCarry(Read(FetchZeroPageAddressX()));
        }

        private void AddWithCarryZeroPage()
        {
            AddWithCarry(Read(FetchZeroPageAddress()));
        }

        private void AndIndirectY()
        {
            AndAccumulator(Read(FetchIndirectIndexedAddressY()));
        }

        private void AndIndirectX()
        {
            AndAccumulator(Read(FetchIndexedIndirectAddressX()));
        }

        private void AndAbsoluteY()
        {
            AndAccumulator(Read(FetchAbsoluteAddressY()));
        }

        private void AndAbsoluteX()
        {
            AndAccumulator(Read(FetchAbsoluteAddressX()));
        }

        private void AndAbsolute()
        {
            AndAccumulator(Read(FetchAbsoluteAddress()));
        }

        private void AndZeroPageX()
        {
            AndAccumulator(Read(FetchZeroPageAddressX()));
        }

        private void AndZeroPage()
        {
            AndAccumulator(Read(FetchZeroPageAddress()));
        }

        private void AndImmediate()
        {
            AndAccumulator(FetchImmediate());
        }
        private void AslAbsoluteX()
        {
            Asl(Read(FetchAbsoluteAddressX()));
        }

        private void AslAbsolute()
        {
            Asl(Read(FetchAbsoluteAddress()));
        }

        private void AslZeroPageX()
        {
            Asl(Read(FetchZeroPageAddressX()));
        }

        private void AslZeroPage()
        {
            Asl(Read(FetchZeroPageAddress()));
        }

        private void AslAccumulator()
        {
            Asl(A);
        }
        private void BitTestAbsolute()
        {
            BitTest(FetchAbsoluteAddress());
        }
        private void BitTestZeroPage()
        {
            BitTest(FetchZeroPageAddress());
        }

        private void BranchOnCarryClear()
        {
            Branch(!P.C);
        }

        private void BranchOnCarrySet()
        {
            Branch(P.C);
        }
        private void BranchOnOverflowClear()
        {
            Branch(!P.V);
        }

        private void BranchOnOverflowSet()
        {
            Branch(P.V);
        }
        private void BranchEquals()
        {
            Branch(P.Z);
        }
        private void BranchMinus()
        {
            Branch(P.N);
        }
        private void BranchNotEquals()
        {
            Branch(!P.Z);
        }
        private void BranchPositive()
        {
            Branch(!P.N);
        }

        private void Break()
        {
            P.B = true;            
        }
        private void ClearCarryFlag()
        {
            P.C = false;
        }

        private void ClearDecimalFlag()
        {
            P.D = false;
        }
        private void ClearOverflowFlag()
        {
            P.V = false;
        }

        private void ClearInterruptDisableFlag()
        {
            P.I = false;
        }
        private void CompareAccumulatorImmediate()
        {
            Compare(FetchImmediate(), A);
        }

        private void CompareAccumulatorZeroPage()
        {
            CompareMemory(FetchZeroPageAddress(), A);
        }

        private void CompareAccumulatorZeroPageX()
        {
            CompareMemory(FetchZeroPageAddressX(), A);
        }

        private void CompareAccumulatorAbsolute()
        {
            CompareMemory(FetchAbsoluteAddress(), A);
        }

        private void CompareAccumulatorAbsoluteX()
        {
            CompareMemory(FetchAbsoluteAddressX(), A);
        }
        private void CompareAccumulatorIndirectX()
        {
            CompareMemory(FetchIndexedIndirectAddressX(), A);
        }
        private void CompareAccumulatorIndirectY()
        {
            CompareMemory(FetchIndirectIndexedAddressY(), A);
        }
        private void CompareXAbsolute()
        {
            CompareMemory(FetchAbsoluteAddress(),X);
        }

        private void CompareXZeroPage()
        {
            CompareMemory(FetchZeroPageAddress(),X);
        }

        private void CompareXImmediate()
        {
            Compare(FetchImmediate(),X);
        }

        private void CompareYAbsolute()
        {
            CompareMemory(FetchAbsoluteAddress(),Y);
        }

        private void CompareYZeroPage()
        {
            CompareMemory(FetchZeroPageAddress(),Y);
        }

        private void CompareYImmediate()
        {
            Compare(FetchImmediate(),Y);
        }
       private void JumpAbsolute()
        {
            PC = FetchAbsoluteAddress();
        }
        private void JumpIndirect()
        {
            PC = FetchIndirectAddress();
        }
        private void JumpToSubroutine()
        {
            var address = FetchAbsoluteAddress();
            PushWord(PC);
            PC = address;
        }
        private void LoadAccumulatorImmediate()
        {
            LoadAccumulator(FetchImmediate());
        }

        private void LoadAccumulatorZeroPage()
        {
            LoadAccumulator(Read(FetchZeroPageAddress()));
        }

        private void LoadAccumulatorZeroPageX()
        {
            LoadAccumulator(Read(FetchZeroPageAddressX()));
        }

        private void LoadAccumulatorAbsolute()
        {
            LoadAccumulator(Read(FetchAbsoluteAddress()));
        }

        private void LoadAccumulatorAbsoluteX()
        {
            LoadAccumulator(Read(FetchAbsoluteAddressX()));
        }

        private void LoadAccumulatorIndirectX()
        {
            LoadAccumulator(Read(FetchIndexedIndirectAddressX()));
        }

        private void LoadAccumulatorIndirectY()
        {
            LoadAccumulator(Read(FetchIndirectIndexedAddressY()));
        }
        private void LoadXImmediate()
        {
            LoadX(FetchImmediate());
        }
        private void LoadXZeroPage()
        {
            LoadX(Read(FetchZeroPageAddress()));
        }
        private void LoadXZeroPageY()
        {
            LoadX(Read(FetchZeroPageAddressY()));
        }
        private void LoadXAbsolute()
        {
            LoadX(Read(FetchAbsoluteAddress()));
        }

        private void LoadXAbsoluteY()
        {
            LoadX(Read(FetchAbsoluteAddressY()));
        }
        private void LoadYImmediate()
        {   
            LoadY(FetchImmediate());
        }
        private void LoadYZeroPage()
        {
            LoadY(Read(FetchZeroPageAddress()));
        }
        private void LoadYZeroPageX()
        {
            LoadY(Read(FetchZeroPageAddressX()));
        }
        private void LoadYAbsolute()
        {
            LoadY(Read(FetchAbsoluteAddress()));
        }
        private void LoadYAbsoluteX()
        {
            LoadY(Read(FetchAbsoluteAddressX()));
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
        private void SetDecimalFlag()
        {
            P.D = true;
        }
        private void SetInterruptDisableFlag()
        {
            P.I = true;
        }

        private void StoreAccumulatorAbsolute()
        {
            Write(FetchAbsoluteAddress(), A);
        }
        private void StoreAccumulatorZeroPage()
        {
            Write(FetchZeroPageAddress(), A);
        }
        private void StoreAccumulatorZeroPageX()
        {
            Write(FetchZeroPageAddressX(), A);
        }
        private void StoreAccumulatorAbsoluteX()
        {
            Write(FetchAbsoluteAddressX(), A);
        }
        private void StoreAccumulatorAbsoluteY()
        {
            Write(FetchAbsoluteAddressY(), A);
        }
        private void StoreAccumulatorIndirectX()
        {
            Write(FetchIndexedIndirectAddressX(), A);
        }
        private void StoreAccumulatorIndirectY()
        {
            Write(FetchIndirectIndexedAddressY(), A);
        }

        private void StoreXAbsolute()
        {
            Write(FetchAbsoluteAddress(), X);
        }
        private void StoreXZeroPage()
        {
            Write(FetchZeroPageAddress(), X);
        }
        private void StoreXZeroPageY()
        {
            Write(FetchZeroPageAddressY(), X);
        }
        private void StoreYAbsolute()
        {
            Write(FetchAbsoluteAddress(), Y);
        }
        private void StoreYZeroPage()
        {
            Write(FetchZeroPageAddress(), Y);
        }
        private void StoreYZeroPageX()
        {
            Write(FetchZeroPageAddressX(), Y);
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
