using System;
using System.Collections.Generic;
using System.IO;
using HardwareCore;

namespace Assembler6502
{
    public class AssemblyParser
    {
        private IAddressMap _map;
        private LexicalAnalyser _analyser;
        private List<Token> _tokens;
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
            _tokens = new List<Token>();
            _analyser.Parse(code);

            _tokens = FindOpcodes(_tokens);
        }

        private List<Token> FindOpcodes(List<Token> tokens)
        {
            var outputTokens = new List<Token>();

            foreach(var token in tokens)
            {
                outputTokens.Add(ResolveOpcodeToken(token));
            }

            return outputTokens;
        }

        private Token ResolveOpcodeToken(Token token)
        {
            var identifierToken = token as IdentifierToken;

            if(identifierToken != null)
            {
                switch(identifierToken.Value)
                {
                    case "LDA":
                        return new LDAToken(token);
                    case "TAY":
                        return new TAYToken(token);
                    case "STA":
                        return new STAToken(token);
                    case "INY":
                        return new INYToken(token);
                    case "DEX":
                        return new DEXToken(token);
                    case "BNE":
                        return new BNEToken(token);
                    case "RTS":
                        return new RTSToken(token);
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

    public abstract class OpcodeToken : Token
    {
        public string Opcode {get; private set;}
        public Token SourceToken {get; private set;}
        public OpcodeToken(string opcode, Token sourceToken) 
            : base(sourceToken.LineNumber, sourceToken.LineOffset)
        {
            Opcode = opcode;
            SourceToken = sourceToken;
        }
    }
    public class LDAToken : OpcodeToken
    {
        public LDAToken(Token sourceToken) : 
            base("LDA", sourceToken)
        {
        }
    }
    public class TAYToken : OpcodeToken
    {
        public TAYToken(Token sourceToken) : 
            base("TAY", sourceToken)
        {
        }
    }
    public class STAToken : OpcodeToken
    {
        public STAToken(Token sourceToken) : 
            base("STA", sourceToken)
        {
        }
    }
    public class INYToken : OpcodeToken
    {
        public INYToken(Token sourceToken) : 
            base("INY", sourceToken)
        {
        }
    }
    public class DEXToken : OpcodeToken
    {
        public DEXToken(Token sourceToken) : 
            base("DEX", sourceToken)
        {
        }
    }
    public class BNEToken : OpcodeToken
    {
        public BNEToken(Token sourceToken) : 
            base("BNE", sourceToken)
        {
        }
    }
    public class RTSToken : OpcodeToken
    {
        public RTSToken(Token sourceToken) : 
            base("RTS", sourceToken)
        {
        }
    }

}