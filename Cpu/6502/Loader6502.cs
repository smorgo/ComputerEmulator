using HardwareCore;

namespace _6502
{
    
    public static partial class LoaderExensions6502
    {
        public static ILoader ADC_IMMEDIATE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.ADC_IMMEDIATE, label);
            loader.Write(operand);
            return loader;
        }

        public static ILoader ADC_IMMEDIATE(this ILoader loader, char operand, string label = null)
        {
            return ADC_IMMEDIATE(loader, (byte)operand, label);
        }

        public static ILoader ADC_ZERO_PAGE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.ADC_ZERO_PAGE, label);
            loader.Write(operand);
            return loader;
        }

        public static ILoader ADC_ZERO_PAGE_X(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.ADC_ZERO_PAGE_X, label);
            loader.Write(operand);
            return loader;
        }

        public static ILoader ADC_ABSOLUTE(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.ADC_ABSOLUTE, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader ADC_ABSOLUTE_X(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.ADC_ABSOLUTE_X, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader ADC_ABSOLUTE_Y(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.ADC_ABSOLUTE_Y, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader ADC_INDIRECT_X(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.ADC_INDIRECT_X, label);
            loader.Write(operand);
            return loader;
        }

        public static ILoader ADC_INDIRECT_Y(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.ADC_INDIRECT_Y, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader AND_IMMEDIATE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.AND_IMMEDIATE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader AND_IMMEDIATE(this ILoader loader, char operand, string label = null)
        {
            return AND_IMMEDIATE(loader, (byte)operand, label);
        }

        public static ILoader AND_ZERO_PAGE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.AND_ZERO_PAGE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader AND_ZERO_PAGE_X(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.AND_ZERO_PAGE_X, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader AND_ABSOLUTE(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.AND_ABSOLUTE, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader AND_ABSOLUTE_X(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.AND_ABSOLUTE_X, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader AND_ABSOLUTE_Y(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.AND_ABSOLUTE_Y, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader AND_INDIRECT_X(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.AND_INDIRECT_X, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader AND_INDIRECT_Y(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.AND_INDIRECT_Y, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader ASL_ACCUMULATOR(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.ASL_ACCUMULATOR, label);
            return loader;
        }
        
        public static ILoader ASL_ZERO_PAGE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.ASL_ZERO_PAGE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader ASL_ZERO_PAGE_X(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.ASL_ZERO_PAGE_X, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader ASL_ABSOLUTE(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.ASL_ABSOLUTE, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader ASL_ABSOLUTE_X(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.ASL_ABSOLUTE_X, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader BCC(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.BCC, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader BCS(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.BCS, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader BEQ(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.BEQ, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader BIT_ZERO_PAGE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.BIT_ZERO_PAGE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader BIT_ABSOLUTE(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.BIT_ABSOLUTE, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader BMI(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.BMI, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader BNE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.BNE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader BPL(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.BPL, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader BRK(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.BRK, label);
            return loader;
        }

        public static ILoader BVC(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.BVC, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader BVS(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.BVS, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader CLC(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.CLC, label);
            return loader;
        }

        public static ILoader CLD(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.CLD, label);
            return loader;
        }

        public static ILoader CLI(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.CLI, label);
            return loader;
        }

        public static ILoader CLV(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.CLV, label);
            return loader;
        }

        public static ILoader CMP_IMMEDIATE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.CMP_IMMEDIATE, label);
            loader.Write(operand);
            return loader;
        }
        public static ILoader CMP_IMMEDIATE(this ILoader loader, char operand, string label = null)
        {
            return CMP_IMMEDIATE(loader, (byte)operand, label);
        }

        public static ILoader CMP_ZERO_PAGE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.CMP_ZERO_PAGE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader CMP_ZERO_PAGE_X(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.CMP_ZERO_PAGE_X, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader CMP_ABSOLUTE(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.CMP_ABSOLUTE, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader CMP_ABSOLUTE_X(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.CMP_ABSOLUTE_X, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader CMP_ABSOLUTE_Y(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.CMP_ABSOLUTE_Y, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader CMP_INDIRECT_X(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.CMP_INDIRECT_X, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader CMP_INDIRECT_Y(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.CMP_INDIRECT_Y, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader CPX_IMMEDIATE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.CPX_IMMEDIATE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader CPX_IMMEDIATE(this ILoader loader, char operand, string label = null)
        {
            return CPX_IMMEDIATE(loader, (byte)operand, label);
        }

        public static ILoader CPX_ZERO_PAGE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.CPX_ZERO_PAGE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader CPX_ABSOLUTE(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.CPX_ABSOLUTE, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader CPY_IMMEDIATE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.CPY_IMMEDIATE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader CPY_IMMEDIATE(this ILoader loader, char operand, string label = null)
        {
            return CPY_IMMEDIATE(loader, (byte)operand, label);
        }

        public static ILoader CPY_ZERO_PAGE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.CPY_ZERO_PAGE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader CPY_ABSOLUTE(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.CPY_ABSOLUTE, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader DEC_ZERO_PAGE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.DEC_ZERO_PAGE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader DEC_ZERO_PAGE_X(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.DEC_ZERO_PAGE_X, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader DEC_ABSOLUTE(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.DEC_ABSOLUTE, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader DEC_ABSOLUTE_X(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.DEC_ABSOLUTE_X, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader DEX(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.DEX, label);
            return loader;
        }

        public static ILoader DEY(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.DEY, label);
            return loader;
        }

        public static ILoader EOR_IMMEDIATE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.EOR_IMMEDIATE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader EOR_IMMEDIATE(this ILoader loader, char operand, string label = null)
        {
            return EOR_IMMEDIATE(loader, (byte)operand, label);
        }

        public static ILoader EOR_ZERO_PAGE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.EOR_ZERO_PAGE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader EOR_ZERO_PAGE_X(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.EOR_ZERO_PAGE_X, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader EOR_ABSOLUTE(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.EOR_ABSOLUTE, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader EOR_ABSOLUTE_X(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.EOR_ABSOLUTE_X, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader EOR_ABSOLUTE_Y(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.EOR_ABSOLUTE_Y, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader EOR_INDIRECT_X(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.EOR_INDIRECT_X, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader EOR_INDIRECT_Y(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.EOR_INDIRECT_Y, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader INC_ZERO_PAGE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.INC_ZERO_PAGE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader INC_ZERO_PAGE_X(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.INC_ZERO_PAGE_X, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader INC_ABSOLUTE(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.INC_ABSOLUTE, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader INC_ABSOLUTE_X(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.INC_ABSOLUTE_X, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader INX(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.INX, label);
            return loader;
        }

        public static ILoader INY(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.INY, label);
            return loader;
        }

        public static ILoader JMP_ABSOLUTE(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.JMP_ABSOLUTE, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader JMP_INDIRECT(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.JMP_INDIRECT, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader JSR(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.JSR, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader LDA_IMMEDIATE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.LDA_IMMEDIATE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader LDA_IMMEDIATE(this ILoader loader, char operand, string label = null)
        {
            return LDA_IMMEDIATE(loader, (byte)operand, label);
        }

        public static ILoader LDA_ZERO_PAGE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.LDA_ZERO_PAGE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader LDA_ZERO_PAGE_X(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.LDA_ZERO_PAGE_X, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader LDA_ABSOLUTE(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.LDA_ABSOLUTE, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader LDA_ABSOLUTE_X(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.LDA_ABSOLUTE_X, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader LDA_INDIRECT_X(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.LDA_INDIRECT_X, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader LDA_INDIRECT_Y(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.LDA_INDIRECT_Y, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader LDX_IMMEDIATE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.LDX_IMMEDIATE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader LDX_IMMEDIATE(this ILoader loader, char operand, string label = null)
        {
            return LDX_IMMEDIATE(loader, (byte)operand, label);
        }

        public static ILoader LDX_ZERO_PAGE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.LDX_ZERO_PAGE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader LDX_ZERO_PAGE_Y(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.LDX_ZERO_PAGE_Y, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader LDX_ABSOLUTE(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.LDX_ABSOLUTE, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader LDX_ABSOLUTE_Y(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.LDX_ABSOLUTE_Y, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader LDY_IMMEDIATE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.LDY_IMMEDIATE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader LDY_IMMEDIATE(this ILoader loader, char operand, string label = null)
        {
            return LDY_IMMEDIATE(loader, (byte)operand, label);
        }

        public static ILoader LDY_ZERO_PAGE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.LDY_ZERO_PAGE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader LDY_ZERO_PAGE_X(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.LDY_ZERO_PAGE_X, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader LDY_ABSOLUTE(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.LDY_ABSOLUTE, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader LDY_ABSOLUTE_X(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.LDY_ABSOLUTE_X, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader LSR_ACCUMULATOR(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.LSR_ACCUMULATOR, label);
            return loader;
        }

        public static ILoader LSR_ZERO_PAGE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.LSR_ZERO_PAGE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader LSR_ZERO_PAGE_X(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.LSR_ZERO_PAGE_X, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader LSR_ABSOLUTE(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.LSR_ABSOLUTE, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader LSR_ABSOLUTE_X(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.LSR_ABSOLUTE_X, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader NOP(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.NOP, label);
            return loader;
        }

        public static ILoader ORA_IMMEDIATE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.ORA_IMMEDIATE, label);
            loader.Write(operand);
            return loader;
        }
        public static ILoader ORA_IMMEDIATE(this ILoader loader, char operand, string label = null)
        {
            return ORA_IMMEDIATE(loader, (byte)operand, label);
        }

        
        public static ILoader ORA_ZERO_PAGE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.ORA_ZERO_PAGE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader ORA_ZERO_PAGE_X(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.ORA_ZERO_PAGE_X, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader ORA_ABSOLUTE(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.ORA_ABSOLUTE, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader ORA_ABSOLUTE_X(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.ORA_ABSOLUTE_X, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader ORA_ABSOLUTE_Y(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.ORA_ABSOLUTE_Y, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader ORA_INDIRECT_X(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.ORA_INDIRECT_X, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader ORA_INDIRECT_Y(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.ORA_INDIRECT_Y, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader PHA(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.PHA, label);
            return loader;
        }

        public static ILoader PHP(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.PHP, label);
            return loader;
        }

        public static ILoader PLA(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.PLA, label);
            return loader;
        }

        public static ILoader PLP(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.PLP, label);
            return loader;
        }

        public static ILoader ROL_ACCUMULATOR(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.ROL_ACCUMULATOR, label);
            return loader;
        }

        public static ILoader ROL_ZERO_PAGE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.ROL_ZERO_PAGE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader ROL_ZERO_PAGE_X(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.ROL_ZERO_PAGE_X, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader ROL_ABSOLUTE(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.ROL_ABSOLUTE, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader ROL_ABSOLUTE_X(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.ROL_ABSOLUTE_X, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader ROR_ACCUMULATOR(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.ROR_ACCUMULATOR, label);
            return loader;
        }

        public static ILoader ROR_ZERO_PAGE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.ROR_ZERO_PAGE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader ROR_ZERO_PAGE_X(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.ROR_ZERO_PAGE_X, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader ROR_ABSOLUTE(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.ROR_ABSOLUTE, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader ROR_ABSOLUTE_X(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.ROR_ABSOLUTE_X, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader RTI(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.RTI, label);
            return loader;
        }

        public static ILoader RTS(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.RTS, label);
            return loader;
        }

        public static ILoader SBC_IMMEDIATE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.SBC_IMMEDIATE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader SBC_IMMEDIATE(this ILoader loader, char operand, string label = null)
        {
            return SBC_IMMEDIATE(loader, (byte)operand, label);
        }

        public static ILoader SBC_ZERO_PAGE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.SBC_ZERO_PAGE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader SBC_ZERO_PAGE_X(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.SBC_ZERO_PAGE_X, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader SBC_ABSOLUTE(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.SBC_ABSOLUTE, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader SBC_ABSOLUTE_X(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.SBC_ABSOLUTE_X, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader SBC_ABSOLUTE_Y(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.SBC_ABSOLUTE_Y, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader SBC_INDIRECT_X(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.SBC_INDIRECT_X, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader SBC_INDIRECT_Y(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.SBC_INDIRECT_Y, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader SEC(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.SEC, label);
            return loader;
        }

        public static ILoader SED(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.SED, label);
            return loader;
        }

        public static ILoader SEI(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.SEI, label);
            return loader;
        }

        public static ILoader STA_ZERO_PAGE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.STA_ZERO_PAGE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader STA_ZERO_PAGE_X(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.STA_ZERO_PAGE_X, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader STA_ABSOLUTE(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.STA_ABSOLUTE, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader STA_ABSOLUTE_X(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.STA_ABSOLUTE_X, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader STA_ABSOLUTE_Y(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.STA_ABSOLUTE_Y, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader STA_INDIRECT_X(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.STA_INDIRECT_X, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader STA_INDIRECT_Y(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.STA_INDIRECT_Y, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader STX_ZERO_PAGE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.STX_ZERO_PAGE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader STX_ZERO_PAGE_Y(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.STX_ZERO_PAGE_Y, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader STX_ABSOLUTE(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.STX_ABSOLUTE, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader STY_ZERO_PAGE(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.STY_ZERO_PAGE, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader STY_ZERO_PAGE_X(this ILoader loader, byte operand, string label = null)
        {
            loader.Write(OPCODE.STY_ZERO_PAGE_X, label);
            loader.Write(operand);
            return loader;
        }
        
        public static ILoader STY_ABSOLUTE(this ILoader loader, ushort operand, string label = null)
        {
            loader.Write(OPCODE.STY_ABSOLUTE, label);
            loader.WriteWord(operand);
            return loader;
        }

        public static ILoader TAX(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.TAX, label);
            return loader;
        }

        public static ILoader TAY(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.TAY, label);
            return loader;
        }

        public static ILoader TSX(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.TSX, label);
            return loader;
        }

        public static ILoader TXA(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.TXA, label);
            return loader;
        }

        public static ILoader TXS(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.TXS, label);
            return loader;
        }

        public static ILoader TYA(this ILoader loader, string label = null)
        {
            loader.Write(OPCODE.TYA, label);
            return loader;
        }
    }
}