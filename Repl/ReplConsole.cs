using System;
using Terminal.Gui;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using IntegratedDebugger;
using System.Text;

namespace Repl
{

    public class ReplConsole : ILogger, IEmulatorConsole
    {
        private static ReplConsole _instance;
        private MenuBar _menuBar;
        private Window _logWindow;
        private StatusBar _statusBar;
        // private Filler _logView;
        private LogView _textView;
        private ILogSink _logSink;
        private Debugger.IParser _parser;
        private ScrollBarView _scrollBar;
        private StringBuilder _buffer = new StringBuilder();
        public ReplConsole(ILogSink logSink, Debugger.IParser parser)
        {
            if(_instance != null)
            {
                throw new InvalidOperationException("More than one instance of ReplConsole");
            }
            _instance = this;

            _logSink = logSink;
            _logSink.OnWriteLog += WriteLog;
            _parser = parser;
        }
        public void Start()
        {
            Application.UseSystemConsole = true;
            Application.Init();
            Application.MainLoop.AddIdle(OnMainLoopIdle);
            var top = Application.Top;
            var topFrame = top.Frame;

            _logWindow = new Window("Debugger Log")
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill() - 3,
                CanFocus = false
            };

            // _logView = new Filler(
            //     new Rect(0, 0, 200, 200)
            // );
            _textView = new LogView () {
				X = 0,
				Y = 0,
				Width = Dim.Fill (),
				Height = Dim.Fill (),

			};

            _logWindow.Add (_textView);

            _scrollBar = new ScrollBarView(_textView, true);

			_scrollBar.ChangedPosition += () => {
				_textView.TopRow = _scrollBar.Position;
				if (_textView.TopRow != _scrollBar.Position) {
					_scrollBar.Position = _textView.TopRow;
				}
				_textView.SetNeedsDisplay ();
			};

			_scrollBar.OtherScrollBarView.ChangedPosition += () => {
				_textView.LeftColumn = _scrollBar.OtherScrollBarView.Position;
				if (_textView.LeftColumn != _scrollBar.OtherScrollBarView.Position) {
					_scrollBar.OtherScrollBarView.Position = _textView.LeftColumn;
				}
				_textView.SetNeedsDisplay ();
			};

			_textView.DrawContent += (e) => {
				_scrollBar.Size = _textView.Lines - 1;
				_scrollBar.Position = _textView.TopRow;
				_scrollBar.OtherScrollBarView.Size = _textView.Maxlength;
				_scrollBar.OtherScrollBarView.Position = _textView.LeftColumn;
				_scrollBar.LayoutSubviews ();
				_scrollBar.Refresh ();
			};

            var commandBar = new View()
            {
                X = 0,
                Y = Pos.AnchorEnd(3),
                Width = Dim.Fill(),
                Height = 2
            };

            var commandBarText = new Label("Enter debugger commands")
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(),
                Height = 1
            };

            var commandLabel = new Label(">")
            {
                X = 0,
                Y = 1,
                Width = 1,
                Height = 1
            };

            var commandText = new CommandField
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill(),
                Height = 1,
                CanFocus = true,

            };

			commandText.CommandEntered += (s,e) => {
			 	_parser?.Parse(e);
			};

            commandBar.Add(commandBarText, commandLabel, commandText);

            _menuBar = new MenuBar(
                new MenuBarItem[] {
                    new MenuBarItem ("_File", new MenuItem [] {
                        new MenuItem ("_Save", "Save program", SaveFile),
                        new MenuItem ("_Open", "Open program", OpenFile),
                        new MenuItem ("_Quit", "", () => { if (Quit ()) top.Running = false; })
                }),
                new MenuBarItem ("_Debug", new MenuItem [] {
                    new MenuItem ("_Continue", "", null),
                    new MenuItem ("_Pause", "", null),
                    new MenuItem ("_Step", "", null)
                })
            });


            _statusBar = new StatusBar()
            {
                Text = "Status Bar"
            };

            top.Add(_logWindow);
            top.Add(commandBar);
            top.Add(_menuBar);
            top.Add(_statusBar);
            Application.Run();
        }

        private bool OnMainLoopIdle()
        {
            lock(_buffer)
            {
                if(_buffer.Length > 0)
                {
                    _textView.AppendText(_buffer.ToString());
                    _buffer.Clear();
                }
            }
            return true;
        }

        public void WriteLine(string message)
        {
            // var lines = message.Split(Environment.NewLine);
            // _logView.AddMessages(lines);
            lock(_buffer)
            {
                if(_buffer.Length > 0)
                {
                    _buffer.Append(Environment.NewLine);
                }
                _buffer.Append(message);
            }
        }

        public void SaveFile()
        {

        }
        public void OpenFile()
        {

        }
        public bool Quit()
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            throw new NotImplementedException();
        }

        public void WriteLog(object sender, LogEntry entry)
        {
            WriteLine(entry.Text);
        }
    }
}