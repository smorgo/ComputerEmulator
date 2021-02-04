using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using HardwareCore;

namespace _6502
{

    public partial class CPU6502
    {
        public ushort IRQ_VECTOR = 0xFFFE;
        public ushort NMI_VECTOR = 0xFFFA;
        public ushort RESET_VECTOR = 0xFFFC;
        public static UInt32 MAX_MEMORY = 0x10000;
        public static ushort STACK_BASE = 0x100;
        public ushort PC;
        public Byte SP;
        public Byte A;
        public Byte X;
        public Byte Y;
        public CpuFlags P = new CpuFlags();
        public DebugLevel DebugLevel {get;set;} = DebugLevel.Errors;
        public int NopDelayMilliseconds {get; set;} = 0;
        public DateTime? _sleepUntil = null;
        private IAddressMap _addressMap;
        public int EmulationErrorsCount {get; private set;}
        public HaltReason HaltReason {get; private set;}
        public bool InterruptPending {get; private set;}
        public bool InterruptServicing {get; private set;}
        public bool NmiPending {get; private set;}
        public EventHandler OnTick;
        public EventHandler OnStarted;
        private DateTime _terminateAfter;
        public CPU6502(IAddressMap addressMap)
        {
            _addressMap = addressMap;    
            LoadOpCodes();
            InitialiseVectors();
        }

        // Generate a maskable interrupt
        // The arguments allow this method to be hooked to an event handler
        public async Task Interrupt(object sender = null, object e = null)
        {
            while(P.I)
            {
                await Task.Delay(1);
            }
            InterruptPending = true;
        }

        public void NonMaskableInterrupt()
        {
            NmiPending = true;
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
            OpCodeTable[(int)OPCODE.DEC_ZERO_PAGE] = DecrementZeroPage;
            OpCodeTable[(int)OPCODE.DEC_ZERO_PAGE_X] = DecrementZeroPageX;
            OpCodeTable[(int)OPCODE.DEC_ABSOLUTE] = DecrementAbsolute;
            OpCodeTable[(int)OPCODE.DEC_ABSOLUTE_X] = DecrementAbsoluteX;
            OpCodeTable[(int)OPCODE.DEX] = DecrementX;
            OpCodeTable[(int)OPCODE.DEY] = DecrementY;
            OpCodeTable[(int)OPCODE.EOR_IMMEDIATE] = EorImmediate;
            OpCodeTable[(int)OPCODE.EOR_ZERO_PAGE] = EorZeroPage;
            OpCodeTable[(int)OPCODE.EOR_ZERO_PAGE_X] = EorZeroPageX;
            OpCodeTable[(int)OPCODE.EOR_ABSOLUTE] = EorAbsolute;
            OpCodeTable[(int)OPCODE.EOR_ABSOLUTE_X] = EorAbsoluteX;
            OpCodeTable[(int)OPCODE.EOR_ABSOLUTE_Y] = EorAbsoluteY;
            OpCodeTable[(int)OPCODE.EOR_INDIRECT_X] = EorIndirectX;
            OpCodeTable[(int)OPCODE.EOR_INDIRECT_Y] = EorIndirectY;
            OpCodeTable[(int)OPCODE.INC_ZERO_PAGE] = IncrementZeroPage;
            OpCodeTable[(int)OPCODE.INC_ZERO_PAGE_X] = IncrementZeroPageX;
            OpCodeTable[(int)OPCODE.INC_ABSOLUTE] = IncrementAbsolute;
            OpCodeTable[(int)OPCODE.INC_ABSOLUTE_X] = IncrementAbsoluteX;
            OpCodeTable[(int)OPCODE.INX] = IncrementX;
            OpCodeTable[(int)OPCODE.INY] = IncrementY;
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
            OpCodeTable[(int)OPCODE.LSR_ACCUMULATOR] = LsrAccumulator;
            OpCodeTable[(int)OPCODE.LSR_ZERO_PAGE] = LsrZeroPage;
            OpCodeTable[(int)OPCODE.LSR_ZERO_PAGE_X] = LsrZeroPageX;
            OpCodeTable[(int)OPCODE.LSR_ABSOLUTE] = LsrAbsolute;
            OpCodeTable[(int)OPCODE.LSR_ABSOLUTE_X] = LsrAbsoluteX;
            OpCodeTable[(int)OPCODE.NOP] = NoOperation;
            OpCodeTable[(int)OPCODE.ORA_IMMEDIATE] = OrImmediate;
            OpCodeTable[(int)OPCODE.ORA_ZERO_PAGE] = OrZeroPage;
            OpCodeTable[(int)OPCODE.ORA_ZERO_PAGE_X] = OrZeroPageX;
            OpCodeTable[(int)OPCODE.ORA_ABSOLUTE] = OrAbsolute;
            OpCodeTable[(int)OPCODE.ORA_ABSOLUTE_X] = OrAbsoluteX;
            OpCodeTable[(int)OPCODE.ORA_ABSOLUTE_Y] = OrAbsoluteY;
            OpCodeTable[(int)OPCODE.ORA_INDIRECT_X] = OrIndirectX;
            OpCodeTable[(int)OPCODE.ORA_INDIRECT_Y] = OrIndirectY;
            OpCodeTable[(int)OPCODE.PHA] = PushAccumulator;
            OpCodeTable[(int)OPCODE.PHP] = PushProcessorStatus;
            OpCodeTable[(int)OPCODE.PLA] = PullAccumulator;
            OpCodeTable[(int)OPCODE.PLP] = PullProcessorStatus;
            OpCodeTable[(int)OPCODE.ROL_ACCUMULATOR] = RolAccumulator;
            OpCodeTable[(int)OPCODE.ROL_ZERO_PAGE] = RolZeroPage;
            OpCodeTable[(int)OPCODE.ROL_ZERO_PAGE_X] = RolZeroPageX;
            OpCodeTable[(int)OPCODE.ROL_ABSOLUTE] = RolAbsolute;
            OpCodeTable[(int)OPCODE.ROL_ABSOLUTE_X] = RolAbsoluteX;
            OpCodeTable[(int)OPCODE.ROR_ACCUMULATOR] = RorAccumulator;
            OpCodeTable[(int)OPCODE.ROR_ZERO_PAGE] = RorZeroPage;
            OpCodeTable[(int)OPCODE.ROR_ZERO_PAGE_X] = RorZeroPageX;
            OpCodeTable[(int)OPCODE.ROR_ABSOLUTE] = RorAbsolute;
            OpCodeTable[(int)OPCODE.ROR_ABSOLUTE_X] = RorAbsoluteX;
            OpCodeTable[(int)OPCODE.RTI] = ReturnFromInterrupt;
            OpCodeTable[(int)OPCODE.RTS] = ReturnFromSubroutine;
            OpCodeTable[(int)OPCODE.SBC_IMMEDIATE] = SubtractWithCarryImmediate;
            OpCodeTable[(int)OPCODE.SBC_ZERO_PAGE] = SubtractWithCarryZeroPage;
            OpCodeTable[(int)OPCODE.SBC_ZERO_PAGE_X] = SubtractWithCarryZeroPageX;
            OpCodeTable[(int)OPCODE.SBC_ABSOLUTE] = SubtractWithCarryAbsolute;
            OpCodeTable[(int)OPCODE.SBC_ABSOLUTE_X] = SubtractWithCarryAbsoluteX;
            OpCodeTable[(int)OPCODE.SBC_ABSOLUTE_Y] = SubtractWithCarryAbsoluteY;
            OpCodeTable[(int)OPCODE.SBC_INDIRECT_X] = SubtractWithCarryIndirectX;
            OpCodeTable[(int)OPCODE.SBC_INDIRECT_Y] = SubtractWithCarryIndirectY;
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

        public void InitialiseVectors()
        {
            _addressMap.WriteWord(RESET_VECTOR, 0x8000);
            _addressMap.WriteWord(IRQ_VECTOR, 0xFFF0);
            _addressMap.WriteWord(NMI_VECTOR, 0xFFF0);
            _addressMap.WriteWord(0xFFF0, (byte)OPCODE.RTI);
        }
        
        public void Reset(TimeSpan? maxDuration = null)
        {
            AsyncUtil.RunSync(() => ResetAsync(maxDuration));
        }

        public async Task ResetAsync(TimeSpan? maxDuration = null)
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
            
            InterruptPending = false;
            InterruptServicing = false;
            NmiPending = false;

            EmulationErrorsCount = 0;
            HaltReason = HaltReason.None;

            if(maxDuration.HasValue)
            {
                _terminateAfter = DateTime.Now + maxDuration.Value;
            }
            else
            {
                _terminateAfter = DateTime.MaxValue;
            }

            await Run();
            Log(DebugLevel.Information, "HALT");
        }

        private bool Waiting()
        {
            if(_sleepUntil.HasValue)
            {
                if(DateTime.Now >= _sleepUntil.Value)
                {
                    _sleepUntil = null;
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        private async Task Run()
        {
            _sleepUntil = null; 

            OnStarted?.Invoke(this, null);
            while(true)
            {
                OnTick?.Invoke(this, null);

                if(NmiPending && !InterruptServicing)
                {
                    ServiceNmi();
                }
                else if(InterruptPending && !InterruptServicing)
                {
                    ServiceInterrupt();
                }
                else if(!Waiting()  || InterruptServicing)
                {
                    await RunCurrentInstruction();
                }

                if(P.B)
                {
                    // Break
                    return;
                }

                if(DateTime.Now > _terminateAfter)
                {
                    Console.WriteLine("Terminated after maximum duration");
                    Debug.WriteLine("Terminated after maximum duration");
                    return;
                }
            }
        }

        private async Task RunCurrentInstruction()
        {
            StartLogInstruction(PC);
            var OpCode = Fetch();

            var handler = OpCodeTable[OpCode];

            if(handler == null)
            {
                Log(DebugLevel.Warnings, $"OpCode {OpCode} not handled");
                EmulationErrorsCount++;
            }
            else
            {
                handler();
                EndLogInstruction((OPCODE)OpCode);
            }

            await Task.Delay(0);
        }

        private void ServiceNmi()
        {
            InterruptServicing = true;
            NmiPending = false;
            PushWord(PC);
            PushProcessorStatus();
            PC = ReadWord(NMI_VECTOR);
        }

        private void ServiceInterrupt()
        {
            InterruptServicing = true;
            InterruptPending = false;
            Log(DebugLevel.Information, "Interrupt!");
            SetInterruptDisableFlag();
            PushWord(PC);
            PushProcessorStatus();
            PC = ReadWord(IRQ_VECTOR);
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

            P.C = (total & 0x100) == 0x100;
            var b1 = v1.Bit(7);
            var b2 = v2.Bit(7);
            var r = result.Bit(7);
            
            P.V = (v1.Bit(7) == v2.Bit(7)) && (v1.Bit(7) != result.Bit(7));
        }

        private void SubtractWithCarry(byte value)
        {
            byte v1 = A;
            byte v2 = value;

            var total = v1 - v2;
            
            if(P.C)
            {
                total--;
            }

            byte result = (byte)(total & 0xff);

            LoadAccumulator(result);

            P.C = (total & 0x100) == 0x100;
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
                Break(HaltReason.StackOverflow);
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
                Break(HaltReason.StackUnderflow);
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

        private void OrAccumulator(byte value)
        {
            var result = (byte)(A | value);
            LoadAccumulator(result);
        }

        private void Asl(byte value)
        {
            var carry = value.Bit(7);
            value = (byte)(value << 1);
            LoadAccumulator(value);
            P.C = carry != 0;
        }

        private void Rol(byte value)
        {
            var carry = value.Bit(7);
            value = (byte)(value << 1);
            if(P.C)
            {
                value++;
            }
            LoadAccumulator(value);
            P.C = carry != 0;
        }

        private void Lsr(byte value)
        {
            var carry = value.Bit(0);
            value = (byte)(value >> 1);
            LoadAccumulator(value);
            P.C = carry != 0;
        }

        private void Ror(byte value)
        {
            var carry = value.Bit(0);
            value = (byte)(value >> 1);
            if(P.C)
            {
                value |= 0x80;
            }
            LoadAccumulator(value);
            P.C = carry != 0;
        }

        private void DecrementMemory(ushort address)
        {
            var value = Read(address);
            value--;
            Write(address, value);
            P.Z = value == 0;
            P.N = value.Bit(7) != 0;
        }

        private void IncrementMemory(ushort address)
        {
            var value = Read(address);
            value++;
            Write(address, value);
            P.Z = value == 0;
            P.N = value.Bit(7) != 0;
        }

        private void EorAccumulator(byte value)
        {
            LoadAccumulator((byte)(A ^ value));
        }
        private byte FetchImmediate()
        {
            var value = Fetch();
            LogParams($"#${value:X2}");
            return value;
        }
        private void Break(HaltReason reason)
        {
            P.B = true;            
            HaltReason = reason;
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
            Break(HaltReason.Break);
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
        private void DecrementZeroPage()
        {
            DecrementMemory(FetchZeroPageAddress());
        }

        private void DecrementZeroPageX()
        {
            DecrementMemory(FetchZeroPageAddressX());
        }

        private void DecrementAbsolute()
        {
            DecrementMemory(FetchAbsoluteAddress());
        }

        private void DecrementAbsoluteX()
        {
            DecrementMemory(FetchAbsoluteAddressX());
        }

        private void DecrementX()
        {
            LoadX((byte)(X-1));
        }

        private void DecrementY()
        {
            LoadY((byte)(Y-1));
        }
        private void EorImmediate()
        {
            EorAccumulator(FetchImmediate());
        }

        private void EorZeroPage()
        {
            EorAccumulator(Read(FetchZeroPageAddress()));
        }

        private void EorZeroPageX()
        {
            EorAccumulator(Read(FetchZeroPageAddressX()));
        }

        private void EorAbsolute()
        {
            EorAccumulator(Read(FetchAbsoluteAddress()));
        }

        private void EorAbsoluteX()
        {
            EorAccumulator(Read(FetchAbsoluteAddressX()));
        }

        private void EorAbsoluteY()
        {
            EorAccumulator(Read(FetchAbsoluteAddressY()));
        }

        private void EorIndirectX()
        {
            EorAccumulator(Read(FetchIndexedIndirectAddressX()));
        }

        private void EorIndirectY()
        {
            EorAccumulator(Read(FetchIndirectIndexedAddressY()));
        }

        private void IncrementZeroPage()
        {
            IncrementMemory(FetchZeroPageAddress());
        }

        private void IncrementZeroPageX()
        {
            IncrementMemory(FetchZeroPageAddressX());
        }

        private void IncrementAbsolute()
        {
            IncrementMemory(FetchAbsoluteAddress());
        }

        private void IncrementAbsoluteX()
        {
            IncrementMemory(FetchAbsoluteAddressX());
        }

        private void IncrementX()
        {
            LoadX((byte)(X+1));
        }

        private void IncrementY()
        {
            LoadY((byte)(Y+1));
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
            PushWord((ushort)(PC-1));
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
        private void LsrAbsoluteX()
        {
            Lsr(Read(FetchAbsoluteAddressX()));
        }

        private void LsrAbsolute()
        {
            Lsr(Read(FetchAbsoluteAddress()));
        }

        private void LsrZeroPageX()
        {
            Lsr(Read(FetchZeroPageAddressX()));
        }

        private void LsrZeroPage()
        {
            Lsr(Read(FetchZeroPageAddress()));
        }

        private void LsrAccumulator()
        {
            Lsr(A);
        }
        private void NoOperation()
        {
            // Do nothing

            // Except we can slow down execution (outside of interrupt service routines)
            if(!InterruptServicing && NopDelayMilliseconds > 0)
            {
                _sleepUntil = DateTime.Now + TimeSpan.FromMilliseconds(NopDelayMilliseconds);
            }
        }
        private void OrIndirectY()
        {
            OrAccumulator(Read(FetchIndirectIndexedAddressY()));
        }

        private void OrIndirectX()
        {
            OrAccumulator(Read(FetchIndexedIndirectAddressX()));
        }

        private void OrAbsoluteY()
        {
            OrAccumulator(Read(FetchAbsoluteAddressY()));
        }

        private void OrAbsoluteX()
        {
            OrAccumulator(Read(FetchAbsoluteAddressX()));
        }

        private void OrAbsolute()
        {
            OrAccumulator(Read(FetchAbsoluteAddress()));
        }

        private void OrZeroPageX()
        {
            OrAccumulator(Read(FetchZeroPageAddressX()));
        }

        private void OrZeroPage()
        {
            OrAccumulator(Read(FetchZeroPageAddress()));
        }

        private void OrImmediate()
        {
            OrAccumulator(FetchImmediate());
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
        private void ReturnFromInterrupt()
        {
            var p = Pull();
            var address = PullWord();
            P.Set(p);
            PC = address;
            ClearInterruptDisableFlag();
            InterruptServicing = false;
            _sleepUntil = null;
        }

        private void ReturnFromSubroutine()
        {
            var address = PullWord();
            address++;
            PC = address;
        }
        private void RolAbsoluteX()
        {
            Rol(Read(FetchAbsoluteAddressX()));
        }

        private void RolAbsolute()
        {
            Rol(Read(FetchAbsoluteAddress()));
        }

        private void RolZeroPageX()
        {
            Rol(Read(FetchZeroPageAddressX()));
        }

        private void RolZeroPage()
        {
            Rol(Read(FetchZeroPageAddress()));
        }

        private void RolAccumulator()
        {
            Rol(A);
        }
        private void RorAbsoluteX()
        {
            Ror(Read(FetchAbsoluteAddressX()));
        }

        private void RorAbsolute()
        {
            Ror(Read(FetchAbsoluteAddress()));
        }

        private void RorZeroPageX()
        {
            Ror(Read(FetchZeroPageAddressX()));
        }

        private void RorZeroPage()
        {
            Ror(Read(FetchZeroPageAddress()));
        }

        private void RorAccumulator()
        {
            Ror(A);
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

        private void SubtractWithCarryImmediate()
        {
            SubtractWithCarry(FetchImmediate());
        }

        private void SubtractWithCarryIndirectY()
        {
            SubtractWithCarry(Read(FetchIndirectIndexedAddressY()));
        }

        private void SubtractWithCarryIndirectX()
        {
            SubtractWithCarry(Read(FetchIndexedIndirectAddressX()));
        }

        private void SubtractWithCarryAbsoluteY()
        {
            SubtractWithCarry(Read(FetchAbsoluteAddressY()));
        }

        private void SubtractWithCarryAbsoluteX()
        {
            SubtractWithCarry(Read(FetchAbsoluteAddressX()));
        }

        private void SubtractWithCarryAbsolute()
        {
            SubtractWithCarry(Read(FetchAbsoluteAddress()));
        }

        private void SubtractWithCarryZeroPageX()
        {
            SubtractWithCarry(Read(FetchZeroPageAddressX()));
        }

        private void SubtractWithCarryZeroPage()
        {
            SubtractWithCarry(Read(FetchZeroPageAddress()));
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
