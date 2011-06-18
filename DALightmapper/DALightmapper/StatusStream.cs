using System;
using System.Windows.Forms;

namespace Ben
{
    public enum Verbosity { Sparse = 0, Low = 1, Medium = 2, High = 3, TESTING = 4 }
    enum StatusStreamType { Console = 0, TextBox = 1 }
    public class StatusStream
    {
        StatusStreamType type;
        TextBox _statusBox;
        Verbosity _verbosity;   //The minimum verbosity a message needs to be displayed

        int _indent;

        public Verbosity verbosity
        {
            get { return _verbosity; }
            set { _verbosity = value; }
        }
        public int indent
        {
            get { return _indent; }
            set { _indent = value; }
        }

        public StatusStream(TextBox statusBox)
        {
            type = StatusStreamType.TextBox;
            _statusBox = statusBox;
        }
        public StatusStream()
        {
            type = StatusStreamType.Console;
        }

        public delegate void AppendTextDelegate(String text, Verbosity verb);

        //Adds text to the status stream
        public void AppentLine(String text, Verbosity verb)
        {
            AppendText(text + "\n", verb);
        }
        public void AppendText(String text, Verbosity verb)
        {
            if (type != StatusStreamType.Console && _statusBox.InvokeRequired)
            {
                _statusBox.Invoke(new AppendTextDelegate(this.AppendText), text, verb);
            }
            else
            {
                if (verbosity >= verb)
                {
                    //Apply Indent
                    for (int i = 0; i < indent; i++)
                        text = "    " + text;

                    //Output text
                    switch (type)
                    {
                        case StatusStreamType.TextBox:
                            if (_statusBox.TextLength > _statusBox.MaxLength - 20)
                                _statusBox.Clear();
                            _statusBox.AppendText(text);
                            break;
                        case StatusStreamType.Console:
                            Console.Write(text);
                            break;
                        default:
                            Console.WriteLine("COULD NOT FIND THE TYPE OF THIS STATUS STREAM :O");
                            break;
                    }
                }
            }
        }

        //Clears the status stream of all text
        public void clear()
        {
            switch (type)
            {
                case StatusStreamType.TextBox:
                    _statusBox.Clear();
                    break;
                case StatusStreamType.Console:
                    Console.Clear();
                    break;
                default:
                    Console.WriteLine("COULD NOT FIND THE TYPE OF THIS STATUS STREAM :O");
                    break;
            }
        }
    }
}
