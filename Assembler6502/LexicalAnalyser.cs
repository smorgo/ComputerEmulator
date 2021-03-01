using System;
using System.Collections.Generic;
using System.Text;

namespace Assembler6502
{
    public class LexicalAnalyser
    {
        public EventHandler<Token> TokenParsed {get; set;}
        private StringBuilder _buffer = new StringBuilder();
        private Token _currentToken;
        private State _currentState;
        private char _currentChar;
        private int _lineNumber;
        private int _lineOffset;
        private Dictionary<State, Dictionary<Event, Action>> _stateMachine;
        private enum State
        {
            Initial,
            Identifier,
            Comment,
            HexNumber,
            Number,
            StringLiteral
        }

        private enum Event 
        {
            DecimalDigit,
            Letter,
            Hash,
            Dollar,
            Colon,
            Comma,
            OpenParen,
            CloseParen,
            Quote,
            Equals,
            Whitespace,
            LineEnd,
            Semicolon,
            HexDigit,
            Other,
            Anything
        }
        private void BuildStateMachine()
        {
            _stateMachine = new Dictionary<State, Dictionary<Event, Action>>() {
                { 
                    State.Initial,
                        new Dictionary<Event, Action>()
                        {
                            { Event.DecimalDigit, StartNumber },
                            { Event.Letter, StartIdentifier },
                            { Event.Hash, YieldImmediate },
                            { Event.Dollar, StartHex },
                            { Event.Comma, YieldIndexerDelimiter },
                            { Event.OpenParen, YieldIndirectStart },
                            { Event.CloseParen, YieldIndirectEnd },
                            { Event.Quote, StartStringLiteral },
                            { Event.Equals, YieldAssignmentOperator },
                            { Event.LineEnd, YieldLineEnd},
                            { Event.Semicolon, StartComment },
                            { Event.Whitespace, Ignore }
                        }
                },
                {
                    State.Comment,
                        new Dictionary<Event, Action>()
                        {
                            { Event.LineEnd, YieldCurrent },
                            { Event.Anything, Append }
                        }
                },
                {
                    State.Number,
                        new Dictionary<Event, Action>() 
                        {
                            { Event.DecimalDigit, Append },
                            { Event.Anything, YieldCurrent }
                        }
                },
                {
                    State.Identifier,
                        new Dictionary<Event, Action>()
                        {
                            { Event.Letter, Append },
                            { Event.DecimalDigit, Append },
                            { Event.Anything, YieldCurrent }
                        }
                },
                {
                    State.HexNumber,
                        new Dictionary<Event, Action>()
                        {
                            { Event.HexDigit, Append },
                            { Event.Anything, YieldCurrent }
                        }
                },
                {
                    State.StringLiteral,
                        new Dictionary<Event, Action>()
                        {
                            { Event.Quote, YieldCurrentAndConsume },
                            { Event.LineEnd, ErrorUnterminatedStringLiteral },
                            { Event.Anything, Append }
                        }
                }

            };
        }

        public void Parse(string lines)
        {
            BuildStateMachine();
            
            var ix = 0;
            var size = lines.Length;
            _currentChar = '\0';

            while(ix < size)
            {
                if(_currentChar == '\0')
                {
                    _currentChar = lines[ix];
                    ix++;
                }

                Run();
            }

        }

        private void Run()
        {
            var ev = GetPrimaryEvent();

            if(_stateMachine[_currentState].ContainsKey(ev))
            {
                // We have a transition handler for this character
                _stateMachine[_currentState][ev]();
                return;
            } 

            ev = GetSecondaryEvent();

            if(_stateMachine[_currentState].ContainsKey(ev))
            {
                // We have a transition handler for this character
                _stateMachine[_currentState][ev]();
                return;
            } 

            if(_stateMachine[_currentState].ContainsKey(Event.Anything))
            {
                // We have a transition handler for this character
                _stateMachine[_currentState][Event.Anything]();
                return;
            } 
            
            Yield(new SyntaxErrorToken(_lineNumber, _lineOffset, _currentChar.ToString(), "Unexpected token"));
            ConsumeCurrentCharacter();
        }

        private Event GetPrimaryEvent()
        {
            if(_currentChar == ':') return Event.Colon;
            if(_currentChar == ',') return Event.Comma;
            if(_currentChar == '=') return Event.Equals;
            if(_currentChar == '$') return Event.Dollar;
            if(_currentChar == '"') return Event.Quote;
            if(_currentChar == '(') return Event.OpenParen;
            if(_currentChar == ')') return Event.CloseParen;
            if(_currentChar == '#') return Event.Hash;
            if(_currentChar == ';') return Event.Semicolon;
            if(char.IsDigit(_currentChar)) return Event.DecimalDigit;
            if(char.IsLetter(_currentChar)) return Event.Letter;
            if(_currentChar == Environment.NewLine[0]) return Event.LineEnd;
            if(char.IsWhiteSpace(_currentChar)) return Event.Whitespace;
            return Event.Other;
        }

        private Event GetSecondaryEvent()
        {
            const string HEXCHARS = "0123456789ABCDEFabcdef";
            if(HEXCHARS.Contains(_currentChar)) return Event.HexDigit;
            return Event.Other;
        }

        private void ErrorUnterminatedStringLiteral()
        {   
            _currentToken = new SyntaxErrorToken(
                _currentToken.LineNumber, 
                _currentToken.LineOffset,
                _buffer.ToString(),
                "Unterminated string literal");

            Yield();
            _currentState = State.Initial;
        }

        private void Yield()
        {
            if(_currentToken != null)
            {
                TokenParsed?.Invoke(this, _currentToken);
            }

            _currentToken = null;
            _buffer.Clear();
        }

        private void Yield(Token newToken)
        {
            Yield();
            _currentToken = newToken;
            Yield();
        }

        private void Yield(string value)
        {
            if(_currentToken != null)
            {
                _currentToken.Value = value;
                Yield();
            }
        }

        private void Ignore()
        {
            ConsumeCurrentCharacter();
        }

        private void Append()
        {
            _buffer.Append(_currentChar);
            ConsumeCurrentCharacter();
        }
        private void StartNumber()
        {   
            Yield();
            _currentState = State.Number;
            _currentToken = new DecimalNumberToken(_lineNumber, _lineOffset);
        }

        private void StartIdentifier()
        {
            Yield();
            _currentState = State.Identifier;
            _currentToken = new IdentifierToken(_lineNumber, _lineOffset);
        }

        private void YieldImmediate()
        {
            Yield(new ImmediateValueToken(_lineNumber, _lineOffset));
            ConsumeCurrentCharacter();
        }
        private void YieldIndirectStart()
        {
            Yield(new IndirectStartToken(_lineNumber, _lineOffset));
            ConsumeCurrentCharacter();
        }
        private void YieldIndirectEnd()
        {
            Yield(new IndirectEndToken(_lineNumber, _lineOffset));
            ConsumeCurrentCharacter();
        }

        private void StartHex()
        {
            Yield();
            _currentState = State.HexNumber;
            _currentToken = new HexadecimalNumberToken(_lineNumber, _lineOffset);
            ConsumeCurrentCharacter();
        }
        private void StartComment()
        {
            Yield();
            _currentState = State.Comment;
            _currentToken = new CommentToken(_lineNumber, _lineOffset);
            ConsumeCurrentCharacter();
        }
        private void YieldIndexerDelimiter()
        {
            Yield(new IndexerDelimiterToken(_lineNumber, _lineOffset));
            ConsumeCurrentCharacter();
        }
        private void StartStringLiteral()
        {
            Yield();
            _currentState = State.StringLiteral;
            ConsumeCurrentCharacter();
        }
        private void YieldAssignmentOperator()
        {
            Yield(new AssignmentOperatorToken(_lineNumber, _lineOffset));
            ConsumeCurrentCharacter();
        }

        private void YieldLineEnd()
        {
            Yield(new LineEndToken(_lineNumber, _lineOffset));
            ConsumeCurrentCharacter();
            _lineNumber++;
            _lineOffset = 1;
        }

        private void YieldCurrent()
        {
            Yield(_buffer.ToString());
            _currentState = State.Initial;
        }

        private void YieldCurrentAndConsume()
        {
            YieldCurrent();
            ConsumeCurrentCharacter();
        }

        private void ConsumeCurrentCharacter()
        {
            _currentChar = '\0';
        }
    }
}