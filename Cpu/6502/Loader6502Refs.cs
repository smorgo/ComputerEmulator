using HardwareCore;

namespace _6502
{
    public static partial class LoaderExensions6502
    {
        public static Loader ADC_IMMEDIATE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ADC_IMMEDIATE, label);
            loader.RefByte(refLabel);
            return loader;
        }

        public static Loader ADC_ZERO_PAGE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ADC_ZERO_PAGE, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }

        public static Loader ADC_ZERO_PAGE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ADC_ZERO_PAGE_X, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }

        public static Loader ADC_ABSOLUTE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ADC_ABSOLUTE, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader ADC_ABSOLUTE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ADC_ABSOLUTE_X, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader ADC_ABSOLUTE_Y(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ADC_ABSOLUTE_Y, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader ADC_INDIRECT_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ADC_INDIRECT_X, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }

        public static Loader ADC_INDIRECT_Y(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ADC_INDIRECT_Y, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader AND_IMMEDIATE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.AND_IMMEDIATE, label);
            loader.RefByte(refLabel);
            return loader;
        }
        
        public static Loader AND_ZERO_PAGE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.AND_ZERO_PAGE, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader AND_ZERO_PAGE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.AND_ZERO_PAGE_X, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader AND_ABSOLUTE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.AND_ABSOLUTE, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader AND_ABSOLUTE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.AND_ABSOLUTE_X, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader AND_ABSOLUTE_Y(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.AND_ABSOLUTE_Y, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader AND_INDIRECT_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.AND_INDIRECT_X, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader AND_INDIRECT_Y(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.AND_INDIRECT_Y, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader ASL_ZERO_PAGE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ASL_ZERO_PAGE, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader ASL_ZERO_PAGE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ASL_ZERO_PAGE_X, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader ASL_ABSOLUTE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ASL_ABSOLUTE, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader ASL_ABSOLUTE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ASL_ABSOLUTE_X, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader BCC(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.BCC, label);
            loader.RelativeRef(refLabel);
            return loader;
        }
        
        public static Loader BCS(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.BCS, label);
            loader.RelativeRef(refLabel);
            return loader;
        }
        
        public static Loader BEQ(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.BEQ, label);
            loader.RelativeRef(refLabel);
            return loader;
        }
        
        public static Loader BIT_ZERO_PAGE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.BIT_ZERO_PAGE, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader BIT_ABSOLUTE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.BIT_ABSOLUTE, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader BMI(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.BMI, label);
            loader.RelativeRef(refLabel);
            return loader;
        }
        
        public static Loader BNE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.BNE, label);
            loader.RelativeRef(refLabel);
            return loader;
        }
        
        public static Loader BPL(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.BPL, label);
            loader.RelativeRef(refLabel);
            return loader;
        }
        
        public static Loader BVC(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.BVC, label);
            loader.RelativeRef(refLabel);
            return loader;
        }
        
        public static Loader BVS(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.BVS, label);
            loader.RelativeRef(refLabel);
            return loader;
        }
        
        public static Loader CMP_IMMEDIATE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.CMP_IMMEDIATE, label);
            loader.RefByte(refLabel);
            return loader;
        }
        
        public static Loader CMP_ZERO_PAGE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.CMP_ZERO_PAGE, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader CMP_ZERO_PAGE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.CMP_ZERO_PAGE_X, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader CMP_ABSOLUTE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.CMP_ABSOLUTE, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader CMP_ABSOLUTE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.CMP_ABSOLUTE_X, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader CMP_ABSOLUTE_Y(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.CMP_ABSOLUTE_Y, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader CMP_INDIRECT_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.CMP_INDIRECT_X, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader CMP_INDIRECT_Y(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.CMP_INDIRECT_Y, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader CPX_IMMEDIATE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.CPX_IMMEDIATE, label);
            loader.RefByte(refLabel);
            return loader;
        }
        
        public static Loader CPX_ZERO_PAGE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.CPX_ZERO_PAGE, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader CPX_ABSOLUTE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.CPX_ABSOLUTE, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }

        public static Loader CPY_IMMEDIATE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.CPY_IMMEDIATE, label);
            loader.RefByte(refLabel);
            return loader;
        }
        
        public static Loader CPY_ZERO_PAGE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.CPY_ZERO_PAGE, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader CPY_ABSOLUTE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.CPY_ABSOLUTE, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader DEC_ZERO_PAGE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.DEC_ZERO_PAGE, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader DEC_ZERO_PAGE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.DEC_ZERO_PAGE_X, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader DEC_ABSOLUTE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.DEC_ABSOLUTE, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader DEC_ABSOLUTE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.DEC_ABSOLUTE_X, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader EOR_IMMEDIATE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.EOR_IMMEDIATE, label);
            loader.RefByte(refLabel);
            return loader;
        }
        
        public static Loader EOR_ZERO_PAGE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.EOR_ZERO_PAGE, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader EOR_ZERO_PAGE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.EOR_ZERO_PAGE_X, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader EOR_ABSOLUTE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.EOR_ABSOLUTE, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader EOR_ABSOLUTE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.EOR_ABSOLUTE_X, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader EOR_ABSOLUTE_Y(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.EOR_ABSOLUTE_Y, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader EOR_INDIRECT_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.EOR_INDIRECT_X, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader EOR_INDIRECT_Y(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.EOR_INDIRECT_Y, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader INC_ZERO_PAGE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.INC_ZERO_PAGE, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader INC_ZERO_PAGE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.INC_ZERO_PAGE_X, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader INC_ABSOLUTE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.INC_ABSOLUTE, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader INC_ABSOLUTE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.INC_ABSOLUTE_X, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader JMP_ABSOLUTE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.JMP_ABSOLUTE, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader JMP_INDIRECT(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.JMP_INDIRECT, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader JSR(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.JSR, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader LDA_IMMEDIATE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.LDA_IMMEDIATE, label);
            loader.RefByte(refLabel);
            return loader;
        }
        
        public static Loader LDA_ZERO_PAGE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.LDA_ZERO_PAGE, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader LDA_ZERO_PAGE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.LDA_ZERO_PAGE_X, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader LDA_ABSOLUTE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.LDA_ABSOLUTE, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader LDA_ABSOLUTE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.LDA_ABSOLUTE_X, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader LDA_INDIRECT_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.LDA_INDIRECT_X, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader LDA_INDIRECT_Y(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.LDA_INDIRECT_Y, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader LDX_IMMEDIATE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.LDX_IMMEDIATE, label);
            loader.RefByte(refLabel);
            return loader;
        }
        
        public static Loader LDX_ZERO_PAGE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.LDX_ZERO_PAGE, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader LDX_ZERO_PAGE_Y(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.LDX_ZERO_PAGE_Y, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader LDX_ABSOLUTE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.LDX_ABSOLUTE, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader LDX_ABSOLUTE_Y(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.LDX_ABSOLUTE_Y, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader LDY_IMMEDIATE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.LDY_IMMEDIATE, label);
            loader.RefByte(refLabel);
            return loader;
        }
        
        public static Loader LDY_ZERO_PAGE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.LDY_ZERO_PAGE, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader LDY_ZERO_PAGE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.LDY_ZERO_PAGE_X, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader LDY_ABSOLUTE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.LDY_ABSOLUTE, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader LDY_ABSOLUTE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.LDY_ABSOLUTE_X, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader LSR_ZERO_PAGE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.LSR_ZERO_PAGE, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader LSR_ZERO_PAGE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.LSR_ZERO_PAGE_X, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader LSR_ABSOLUTE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.LSR_ABSOLUTE, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader LSR_ABSOLUTE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.LSR_ABSOLUTE_X, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader ORA_IMMEDIATE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ORA_IMMEDIATE, label);
            loader.RefByte(refLabel);
            return loader;
        }
        
        public static Loader ORA_ZERO_PAGE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ORA_ZERO_PAGE, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader ORA_ZERO_PAGE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ORA_ZERO_PAGE_X, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader ORA_ABSOLUTE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ORA_ABSOLUTE, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader ORA_ABSOLUTE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ORA_ABSOLUTE_X, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader ORA_ABSOLUTE_Y(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ORA_ABSOLUTE_Y, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader ORA_INDIRECT_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ORA_INDIRECT_X, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader ORA_INDIRECT_Y(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ORA_INDIRECT_Y, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader ROL_ZERO_PAGE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ROL_ZERO_PAGE, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader ROL_ZERO_PAGE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ROL_ZERO_PAGE_X, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader ROL_ABSOLUTE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ROL_ABSOLUTE, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader ROL_ABSOLUTE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ROL_ABSOLUTE_X, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader ROR_ZERO_PAGE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ROR_ZERO_PAGE, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader ROR_ZERO_PAGE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ROR_ZERO_PAGE_X, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader ROR_ABSOLUTE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ROR_ABSOLUTE, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader ROR_ABSOLUTE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.ROR_ABSOLUTE_X, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader SBC_IMMEDIATE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.SBC_IMMEDIATE, label);
            loader.RefByte(refLabel);
            return loader;
        }
        
        public static Loader SBC_ZERO_PAGE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.SBC_ZERO_PAGE, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader SBC_ZERO_PAGE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.SBC_ZERO_PAGE_X, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader SBC_ABSOLUTE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.SBC_ABSOLUTE, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader SBC_ABSOLUTE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.SBC_ABSOLUTE_X, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader SBC_ABSOLUTE_Y(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.SBC_ABSOLUTE_Y, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader SBC_INDIRECT_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.SBC_INDIRECT_X, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader SBC_INDIRECT_Y(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.SBC_INDIRECT_Y, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader STA_ZERO_PAGE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.STA_ZERO_PAGE, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader STA_ZERO_PAGE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.STA_ZERO_PAGE_X, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader STA_ABSOLUTE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.STA_ABSOLUTE, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader STA_ABSOLUTE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.STA_ABSOLUTE_X, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader STA_ABSOLUTE_Y(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.STA_ABSOLUTE_Y, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader STA_INDIRECT_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.STA_INDIRECT_X, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader STA_INDIRECT_Y(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.STA_INDIRECT_Y, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader STX_ZERO_PAGE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.STX_ZERO_PAGE, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader STX_ZERO_PAGE_Y(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.STX_ZERO_PAGE_Y, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader STX_ABSOLUTE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.STX_ABSOLUTE, label);
            loader.Ref(refLabel);
            return loader;
        }

        public static Loader STY_ZERO_PAGE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.STY_ZERO_PAGE, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader STY_ZERO_PAGE_X(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.STY_ZERO_PAGE_X, label);
            loader.ZeroPageRef(refLabel);
            return loader;
        }
        
        public static Loader STY_ABSOLUTE(this Loader loader, string refLabel, string label = null)
        {
            loader.Write(OPCODE.STY_ABSOLUTE, label);
            loader.Ref(refLabel);
            return loader;
        }
    }
}