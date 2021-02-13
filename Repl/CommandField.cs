using System;
using Terminal.Gui;

namespace Repl
{
    public class CommandField : TextField
    {
        public EventHandler<string> CommandEntered {get; set;}

		public override bool ProcessKey (KeyEvent kb)
        {
   			switch (ShortcutHelper.GetModifiersKey (kb)) 
            {
			    case Key.Enter:
                    CommandEntered?.Invoke(this, Text.ToString());
                    Text = string.Empty;
                    return true;
                default:
                    return base.ProcessKey(kb);
            }
        }
    }
}