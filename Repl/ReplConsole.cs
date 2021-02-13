using System;
using Terminal.Gui;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using IntegratedDebugger;
using System.Text;
using HardwareCore;

namespace Repl
{

    public class ReplConsole : IEmulatorConsole
    {
        private static ReplConsole _instance;
        private MenuBar _menuBar;
        private Window _logWindow;
        private StatusBar _statusBar;
        // private Filler _logView;
        private LogView _textView;
        private ILogSink _logSink;
        private IRegisterTracker _tracker;
        private Debugger.IParser _parser;
        private ScrollBarView _scrollBar;
        private StringBuilder _buffer = new StringBuilder();
        private StatusItem _runMode = new StatusItem(Key.CharMask, "Mode: Paused  ", null);
        private StatusItem _A = new StatusItem(Key.CharMask, "A:??", null);
        private StatusItem _X = new StatusItem(Key.CharMask, "X:??", null);
        private StatusItem _Y = new StatusItem(Key.CharMask, "Y:??", null);
        private StatusItem _PC = new StatusItem(Key.CharMask, "PC:????", null);
        private StatusItem _SP = new StatusItem(Key.CharMask, "SP:??", null);
        private StatusItem _C = new StatusItem(Key.CharMask, "C:?", null);
        private StatusItem _Z = new StatusItem(Key.CharMask, "Z:?", null);
        private StatusItem _I = new StatusItem(Key.CharMask, "I:?", null);
        private StatusItem _D = new StatusItem(Key.CharMask, "D:?", null);
        private StatusItem _B = new StatusItem(Key.CharMask, "B:?", null);
        private StatusItem _B2 = new StatusItem(Key.CharMask, "B2:?", null);
        private StatusItem _V = new StatusItem(Key.CharMask, "V:?", null);
        private StatusItem _N = new StatusItem(Key.CharMask, "N:?", null);
        public ReplConsole(ILogSink logSink, Debugger.IParser parser, IRegisterTracker tracker)
        {
            if(_instance != null)
            {
                throw new InvalidOperationException("More than one instance of ReplConsole");
            }
            _instance = this;

            _logSink = logSink;
            _logSink.OnWriteLog += WriteLog;
            _parser = parser;
            _tracker = tracker;
            _tracker.RegisterUpdated += RegisterUpdated;
        }

        private void RegisterUpdated(object sender, RegisterUpdatedEventArgs e)
        {
            switch(e.Register)
            {
                case "PC":
                    _PC.Title = $"PC:{e.Value:X4}";
                    break;
                case "SP":
                    _SP.Title = $"SP:{e.Value:X2}";
                    break;
                case "A":
                    _A.Title = $"A:{e.Value:X2}";
                    break;
                case "X":
                    _X.Title = $"X:{e.Value:X2}";
                    break;
                case "Y":
                    _Y.Title = $"Y:{e.Value:X2}";
                    break;
                case "C":
                    _C.Title = $"C:{e.Value:X1}";
                    break;
                case "Z":
                    _Z.Title = $"Z:{e.Value:X1}";
                    break;
                case "I":
                    _I.Title = $"I:{e.Value:X1}";
                    break;
                case "D":
                    _D.Title = $"D:{e.Value:X1}";
                    break;
                case "B":
                    _B.Title = $"B:{e.Value:X1}";
                    break;
                case "B2":
                    _B2.Title = $"B2:{e.Value:X1}";
                    break;
                case "V":
                    _V.Title = $"V:{e.Value:X1}";
                    break;
                case "N":
                    _N.Title = $"N:{e.Value:X1}";
                    break;
                case "MODE":
                    var mode = 
                        ((e.Value == 1) ? "Paused  " :
                         (e.Value == 2) ? "Running " :
                         (e.Value == 3) ? "Stepping" :
                                          "Unknown ");
                    _runMode.Title = "Mode: " + mode;
                        
                    break;
                default:
                    // Unexpected register
                    break;
            }

            _statusBar.SetNeedsDisplay();
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
                CanFocus = false
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
                TabIndex = 0,
            };

			commandText.CommandEntered += (s,e) => {
			 	_parser?.Parse(e);
                commandBar.SetFocus();
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
                ColorScheme = Colors.TopLevel,
				Visible = true,
                Items = new StatusItem[] {
                    _runMode,
                    _A,
                    _X,
                    _Y,
                    _PC,
                    _SP,
                    _C,
                    _Z,
                    _I,
                    _D,
                    _B,
                    _B2,
                    _V,
                    _N
                }
			};

            top.Add(_logWindow);
            top.Add(commandBar);
            top.Add(_menuBar);
            top.Add(_statusBar);

            Application.Run();

            commandBar.SetFocus();
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

        public void WriteLog(object sender, LogEntry entry)
        {
            WriteLine(entry.Text);
        }


    }
}