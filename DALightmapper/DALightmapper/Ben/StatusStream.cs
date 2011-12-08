using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Ben
{
    public enum Verbosity { Warnings = 0, Low = 1, Medium = 2, High = 3, TESTING = 4 }
    public class StatusStream
    {
        private Boolean outputToConsole = false;    //If this stream should output to the console or not
        private List<TextBox> textBoxes;            //The list of text boxes to output too
  
        public Verbosity verbosity { get; set; }    //The minimum verbosity level needed to output to this stream
        public int indent { get; set; }             //The indent to added to output messages

        private delegate void AppendTextDelegate(String text, Verbosity verb, TextBox b);   //The delegate definition use to invoke textbox updates

        //-- Constructors --//
        public StatusStream(TextBox statusBox)
            : this()
        {
            textBoxes.Add(statusBox);
        }
        public StatusStream()
        {
            textBoxes = new List<TextBox>();
            verbosity = Verbosity.Low;
        }


        //-- Attach/Detatch the console or textboxes to/from this tream --//
        public void attachToConsole()
        {
            outputToConsole = true;
        }
        public void detatchFromConsole()
        {
            outputToConsole = false;
        }
        public void attachTextBox(TextBox box)
        {
            if (!textBoxes.Contains(box))
                textBoxes.Add(box);
        }
        public void detatchTextBox(TextBox box)
        {
            textBoxes.Remove(box);
        }


        //-- Append methods with formatting using the lowest verbosity --// 
        //--    Can't use default parameter for verbosity because of ambiguity between and int type first format parameter 
        //--    with no verbosity and Verbosity but no parameters --//
        public void AppendFormatLine(String format, params object[] args)
        {
            AppendFormatLine(format, Verbosity.Warnings, args);
        }
        public void AppendFormat(String format, params object[] args)
        {
            AppendFormat(format, Verbosity.Warnings, args);
        }

        //-- Append methods with formatting using the input verbosity --//
        public void AppendFormatLine(String format, Verbosity verb, params object[] args)
        {
            AppendFormat(format + "\n", verb, args);
        }
        public void AppendFormat(String format, Verbosity verb, params object[] args)
        {
            AppendText(String.Format(format, args), verb);
        }

        //-- Append methods without formatting - default verbosity is the lowest --//
        public void AppendLine(String text, Verbosity verb = Verbosity.Warnings)
        {
            AppendText(text + "\n", verb);
        }
        public void AppendText(String text, Verbosity verb = Verbosity.Warnings)
        {
            if (outputToConsole)
                System.Console.Write(text);

            foreach (TextBox t in textBoxes)
            {
                //If this thread is not the textbox's thread, invoke the update method
                if (t.InvokeRequired)
                    t.Invoke(new AppendTextDelegate(this.AppendTextInvoker), text, verb, t);
                else
                    AppendTextInvoker(text, verb, t);
            }
        }

        //-- Write 'text' to textbox b if 'verb' is equal to or lower than this streams verbosity --//
        private void AppendTextInvoker(String text, Verbosity verb, TextBox b)
        {
            if (verbosity >= verb)
            {
                //Apply Indent
                for (int i = 0; i < indent; i++)
                    text = "    " + text;

                //Double the textbox's max length if this text would overflow
                if (b.TextLength + text.Length > b.MaxLength)
                {
                    b.MaxLength *= 2;
                }

                b.AppendText(text);
            }
        }

        //-- Clears all output channels of text --//
        public void clear()
        {
            if (outputToConsole)
                Console.Clear();

            foreach (TextBox t in textBoxes)
            {
                t.Clear();
            }
        }
    }
}
