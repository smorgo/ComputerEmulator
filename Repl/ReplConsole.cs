using System;
using Terminal.Gui;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Repl
{

    public class ReplConsole : ILogger, IEmulatorConsole
    {
        private MenuBar _menuBar;
        private Window _logWindow;
        private StatusBar _statusBar;
        private Filler _logView;
        public void Start()
        {
            Application.Init();
            var top = Application.Top;
            var topFrame = top.Frame;

            _logWindow = new Window("Debugger Log")
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill() - 2
            };

            _logView = new Filler(
                new Rect(0, 0, 200, 200)
            );

            var scrollView = new ScrollView()
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill() - 2
            };

            scrollView.DrawContent += (r) =>
            {
                scrollView.ContentSize = _logView.GetContentSize();
            };

            scrollView.Add(_logView);

            var commandBar = new View()
            {
                X = 0,
                Y = Pos.AnchorEnd(1),
                Width = Dim.Fill(),
                Height = 1,
                ColorScheme = Colors.Dialog
            };

            var commandLabel = new Label(">")
            {
                X = 0,
                Y = 0,
                Width = 1,
                Height = 1
            };

            var commandText = new TextField
            {
                X = 1,
                Y = 0,
                Width = Dim.Fill(),
                Height = 1
            };

            commandBar.Add(commandLabel, commandText);

            _logWindow.Add(scrollView, commandBar);

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

            top.Add(_logWindow, _menuBar);
            // top.Add(_menuBar);
            top.Add(_statusBar);
            Application.Run();
        }

        public void WriteLine(string message)
        {
            var lines = message.Split(Environment.NewLine);
            _logView.AddMessages(lines);
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
    }

    class Filler : View 
    {
        private List<string> _lines = new List<string>();
        private int w, h;
        public Filler (Rect rect) : base (rect)
        {
            w = rect.Width;
            h = rect.Height;

            // Pre-fill the text
            for(var ix = 1; ix < 200; ix++)
            {
                _lines.Add($"{ix:D4} This is text line number {ix}");
            }
        }

        public void AddMessage(string message)
        {
            AddMessages(new string[] {message});
        }
        public void AddMessages(string[] messages)
        {
            _lines.AddRange(messages);
            SetNeedsDisplay();
        }
        public Size GetContentSize ()
        {
            return new Size (w, h);
        }

        public override void Redraw (Rect bounds)
        {
            Driver.SetAttribute (ColorScheme.Normal);
            var f = Frame;
            w = _lines.Max(x => x.Length);
            h = _lines.Count();

            for (int y = 0; y < h; y++) {
                Move (0, y);
                var line = _lines[y];
                for (int x = 0; x < line.Length; x++) {
                    Rune r = line[x];
                    Driver.AddRune (r);
                }
            }
        }
    }
}