using HardwareCore;

namespace Assembler6502
{
    public class LabelDefinitionToken : Token
    {
        public override bool ProvidesLabel => true;
        private IdentifierToken _sourceToken;
        private ushort? _address;
        public LabelDefinitionToken(IdentifierToken sourceToken) : base(sourceToken.LineNumber, sourceToken.LineOffset)
        {
            _sourceToken = sourceToken;
            _address = null;
        }

        public LabelDefinitionToken(IdentifierToken sourceToken, ushort address) : base(sourceToken.LineNumber, sourceToken.LineOffset)
        {
            _sourceToken = sourceToken;
            _address = address;
        }

        public override void Emit(ILoader loader)
        {
            if(_address.HasValue)
            {
                loader.Label(_address.Value, _sourceToken.AsString().ToUpper());
            }
            else
            {
                loader.Label(_sourceToken.AsString().ToUpper());
            }
        }
    }

}