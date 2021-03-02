using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HardwareCore;

namespace Assembler6502
{
    public class AssemblyParser
    {
        private IAddressMap _map;
        private LexicalAnalyser _analyser;
        private TokenList _tokens;
        public AssemblyParser(IAddressMap map, LexicalAnalyser analyser)
        {
            _map = map;
            _analyser = analyser;
            _analyser.TokenParsed += AnalyserTokenParsed;
        }

        private void AnalyserTokenParsed(object sender, Token e)
        {
            _tokens.Add(e);
        }

        public void ParseFile(string filename)
        {
            using(var input = File.OpenText(filename))
            {
                var lines = input.ReadToEnd();
                Parse(lines);
            }
        }

        public void Parse(string code)
        {
            _tokens = new TokenList();
            _analyser.Parse(code);
            _tokens = RemoveComments(_tokens);
            _tokens = ResolveAssignments(_tokens);
            _tokens = FindOpcodes(_tokens);
            _tokens = ResolveImmediates(_tokens);
            _tokens = ResolveIndirects(_tokens);
            _tokens = ResolveAbsolutes(_tokens);
            _tokens = ResolveOperands(_tokens);
            _tokens = ResolveLabels(_tokens);
            _tokens = RemoveUnwantedTokens(_tokens);

            using(var loader = _map.Load(0x8000))
            {
                foreach(var token in _tokens)
                {
                    token.Emit(loader);
                }
            }
        }
        private TokenList ResolveAssignments(TokenList tokens)
        {
            var outputTokens = new TokenList();

            Token current = tokens.Take();

            while(!(current is EofToken))
            {
                if(current is IdentifierToken && tokens.Peek() is AssignmentOperatorToken)
                {
                    tokens.Take();
                    var rhs = tokens.Take();
                    outputTokens.Add(new LabelDefinitionToken((IdentifierToken)current, rhs.AsWord()));
                }
                else if(current is AsteriskToken && tokens.Peek() is AssignmentOperatorToken)
                {
                    tokens.Take();
                    var rhs = tokens.Take();
                    outputTokens.Add(new CursorAssignmentToken(rhs));
                }

                else
                {
                    outputTokens.Add(current);
                }

                current = tokens.Take();
            }

            return outputTokens;
        }

        private TokenList ResolveAbsolutes(TokenList tokens)
        {
            var outputTokens = new TokenList();

            Token current = tokens.Take();

            while(!(current is EofToken))
            {
                if(!(current is HexadecimalNumberToken || current is DecimalNumberToken))
                {
                    if(tokens.Peek() is IndexerDelimiterToken)
                    {
                        tokens.Take();
                        var indexer = tokens.Take();
                        if(indexer.Value == "X")
                        {
                            outputTokens.Add(new AbsoluteXToken(current));
                        }
                        else if(indexer.Value == "Y")
                        {
                            outputTokens.Add(new AbsoluteYToken(current));
                        }
                        else
                        {
                            outputTokens.Add(new SyntaxErrorToken(current, "Invalid offset specifier"));
                        }
                    }
                    else
                    {
                        outputTokens.Add(current);
                    }
                }
                else
                {
                    outputTokens.Add(current);
                }
                current = tokens.Take();
            }
            return outputTokens;
        }
        private TokenList RemoveUnwantedTokens(TokenList tokens)
        {
            var outputTokens = new TokenList();

            Token current = tokens.Take();

            while(!(current is EofToken))
            {
                if(!(current is LineEndToken))
                {
                    outputTokens.Add(current);
                }
                current = tokens.Take();
            }

            return outputTokens;
        }

        private TokenList ResolveOperands(TokenList tokens)
        {
            var outputTokens = new TokenList();

            Token current = tokens.Take();

            while(!(current is EofToken))
            {
                if(current is OpcodeToken)
                {
                    var opcode = (OpcodeToken)current;

                    if(opcode.HasOperand)
                    {
                        var operand = tokens.Take();
                        opcode.OperandToken = operand;
                    }
                }
                outputTokens.Add(current);
                current = tokens.Take();
            }

            return outputTokens;
        }
        private TokenList ResolveLabels(TokenList tokens)
        {
            bool startLine = true;

            var outputTokens = new TokenList();

            Token current = tokens.Take();

            while(!(current is EofToken))
            {
                if(current is LineEndToken)
                {
                    startLine = true;
                    outputTokens.Add(current);
                }
                else if(startLine && current is IdentifierToken)
                {
                    outputTokens.Add(new LabelDefinitionToken((IdentifierToken)current));
                    startLine = false;
                }
                else
                {
                    outputTokens.Add(current);
                    startLine = false;
                }

                current = tokens.Take();
            }

            return outputTokens;
        }

        private TokenList ResolveIndirects(TokenList tokens)
        {
            /*
            (address)
            (address,X)
            (address),Y
            */
            var outputTokens = new TokenList();

            Token current = tokens.Take();

            while(!(current is EofToken))
            {
                if(current is IndirectStartToken)
                {
                    var addressToken = tokens.Take();

                    if(addressToken is HexadecimalNumberToken || 
                        addressToken is DecimalNumberToken || addressToken is IdentifierToken)
                    {
                        var nextToken = tokens.Take();

                        if(nextToken is IndirectEndToken)
                        {
                            if(tokens.Peek() is IndexerDelimiterToken)
                            {
                                // (address),Y?
                                nextToken = tokens.Peek();
                                if(nextToken is IdentifierToken && nextToken.Value == "Y")
                                {
                                    outputTokens.Add(new IndirectYToken(addressToken));
                                    tokens.Take(); // Take the ,
                                    tokens.Take(); // Take the Y
                                }
                                else
                                {
                                    outputTokens.Add(new SyntaxErrorToken(nextToken, "Invalid index"));
                                    tokens.Take();
                                }
                            }
                            else 
                            {
                                outputTokens.Add(new IndirectAddressToken(addressToken));
                            }
                        }
                        else if(nextToken is IndexerDelimiterToken)
                        {
                            nextToken = tokens.Peek();

                            if(nextToken is IdentifierToken && nextToken.Value == "X" &&
                                tokens.Peek() is IndirectEndToken)
                            {
                                outputTokens.Add(new IndirectXToken(addressToken));
                                tokens.Take(); // Take the X
                                tokens.Take(); // Take the )
                            }
                        }
                        else
                        {
                            outputTokens.Add(new SyntaxErrorToken(current, "Invalid indirect or indexed address"));
                        }
                    }
                }
                else
                {
                    outputTokens.Add(current);
                }

                current = tokens.Take();
            }

            return outputTokens;
        }

        private TokenList RemoveComments(TokenList tokens)
        {
            var outputTokens = tokens.Where(x => !(x is CommentToken)).ToList();
            return new TokenList(outputTokens);
        }

        private TokenList FindOpcodes(TokenList tokens)
        {
            var outputTokens = new TokenList();

            foreach(var token in tokens)
            {
                outputTokens.Add(ResolveOpcodeToken(token));
            }

            return outputTokens;
        }

        private TokenList ResolveImmediates(TokenList tokens)
        {
            var outputTokens = new TokenList();
            var isImmediate = false;

            foreach(var token in tokens)
            {
                if(isImmediate)
                {
                    if(token is HexadecimalNumberToken)
                    {
                        outputTokens.Add(new ImmediateHexadecimalToken((HexadecimalNumberToken)token));
                    }
                    else if(token is DecimalNumberToken)
                    {
                        outputTokens.Add(new ImmediateDecimalToken((DecimalNumberToken)token));
                    }
                    else if(token is CharLiteralToken)
                    {
                        outputTokens.Add(new ImmediateCharToken((CharLiteralToken)token));
                    }
                    else if(token is IdentifierToken)
                    {
                        outputTokens.Add(new ImmediateLabelToken((IdentifierToken)token));
                    }
                    else
                    {
                        outputTokens.Add(new SyntaxErrorToken(token, "Invalid immediate value"));
                    }
                    isImmediate = false;
                }
                else if(token is ImmediateValueToken)
                {
                    isImmediate = true;
                }
                else
                {
                    outputTokens.Add(token);
                }
            }

            return outputTokens;
        }

        private Token ResolveOpcodeToken(Token token)
        {
            var identifierToken = token as IdentifierToken;

            if(identifierToken != null)
            {
                switch(identifierToken.Value.ToUpper())
                {
                    case "ADC":
                        return new ADCToken(token);
                    case "AND":
                        return new ANDToken(token);
                    case "ASL":
                        return new ASLToken(token);
                    case "BCC":
                        return new BCCToken(token);
                    case "BCS":
                        return new BCSToken(token);
                    case "BEQ":
                        return new BEQToken(token);
                    case "BIT":
                        return new BITToken(token);
                    case "BMI":
                        return new BMIToken(token);
                    case "BNE":
                        return new BNEToken(token);
                    case "BPL":
                        return new BPLToken(token);
                    case "BRK":
                        return new BRKToken(token);
                    case "BVC":
                        return new BVCToken(token);
                    case "BVS":
                        return new BVSToken(token);
                    case "CLC":
                        return new CLCToken(token);
                    case "CLD":
                        return new CLDToken(token);
                    case "CLI":
                        return new CLIToken(token);
                    case "CLV":
                        return new CLVToken(token);
                    case "CMP":
                        return new CMPToken(token);
                    case "CPX":
                        return new CPXToken(token);
                    case "CPY":
                        return new CPYToken(token);
                    case "DEC":
                        return new DECToken(token);
                    case "DEX":
                        return new DEXToken(token);
                    case "DEY":
                        return new DEYToken(token);
                    case "EOR":
                        return new EORToken(token);
                    case "INC":
                        return new INCToken(token);
                    case "INX":
                        return new INXToken(token);
                    case "INY":
                        return new INYToken(token);
                    case "JMP":
                        return new JMPToken(token);
                    case "JSR":
                        return new JSRToken(token);
                    case "LDA":
                        return new LDAToken(token);
                    case "LDX":
                        return new LDXToken(token);
                    case "LDY":
                        return new LDYToken(token);
                    case "LSR":
                        return new LSRToken(token);
                    case "NOP":
                        return new NOPToken(token);
                    case "ORA":
                        return new ORAToken(token);
                    case "PHA":
                        return new PHAToken(token);
                    case "PHP":
                        return new PHPToken(token);
                    case "PLA":
                        return new PLAToken(token);
                    case "PLP":
                        return new PLPToken(token);
                    case "ROL":
                        return new ROLToken(token);
                    case "ROR":
                        return new RORToken(token);
                    case "RTI":
                        return new RTIToken(token);
                    case "RTS":
                        return new RTSToken(token);
                    case "SBC":
                        return new SBCToken(token);
                    case "SEC":
                        return new SECToken(token);
                    case "SED":
                        return new SEDToken(token);
                    case "SEI":
                        return new SEIToken(token);
                    case "STA":
                        return new STAToken(token);
                    case "STX":
                        return new STXToken(token);
                    case "STY":
                        return new STYToken(token);
                    case "TAX":
                        return new TAXToken(token);
                    case "TAY":
                        return new TAYToken(token);
                    case "TSX":
                        return new TSXToken(token);
                    case "TXA":
                        return new TXAToken(token);
                    case "TXS":
                        return new TXSToken(token);
                    case "TYA":
                        return new TYAToken(token);
                    default:
                        return token;
                }
            }
            else
            {
                return token;
            }
        }
    }

}