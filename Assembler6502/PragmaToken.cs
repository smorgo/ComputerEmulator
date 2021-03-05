using HardwareCore;

namespace Assembler6502
{
    public class PragmaToken : Token
    {
        public override bool ProvidesLabel => true;
        public TokenList Parameters {get; private set;}
        public PragmaToken(int lineNumber, int lineOffset) : base(lineNumber, lineOffset)
        {
            Parameters = new TokenList();
        }

        public override void Emit(ILoader loader)
        {
            switch(Value.ToUpper())
            {
                case "BYTE":
                    EmitBytes(loader);
                    break;
                default:
                    DidNotEmit();
                    break;
            }
        }

        private void EmitBytes(ILoader loader)
        {
            foreach(var parameter in Parameters)
            {
                if(parameter.ProvidesByte)
                {
                    loader.Write(parameter.AsByte());
                }
                else if(parameter is StringLiteralToken)
                {
                    loader.WriteString(parameter.AsString());
                }
            }
        }
    }
}