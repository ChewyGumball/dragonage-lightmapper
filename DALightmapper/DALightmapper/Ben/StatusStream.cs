using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Ben
{
    public enum Verbosity { Warnings = 0, Low = 1, Medium = 2, High = 3, TESTING = 4 }
    public class StatusStream
    {
        private Boolean outputToConsole = true;    //If this stream should output to the console or not
        private List<TextBox> textBoxes;            //The list of text boxes to output too
        private List<ProgressBar> progressBars;
  
        public Verbosity verbosity { get; set; }    //The minimum verbosity level needed to output to this stream
        public int indent { get; set; }             //The indent to added to output messages

        private delegate void AppendTextDelegate(Verbosity verb, String text, TextBox b);   //The delegate definition used to invoke textbox updates
        private delegate void UpdateProgressDelegate(int delta, ProgressBar p); //The delegate definition used to invoke progressbar updates

        //-- Constructors --//
        public StatusStream(TextBox statusBox)
            : this()
        {
            textBoxes.Add(statusBox);
        }
        public StatusStream(ProgressBar progressBar)
            : this()
        {
            progressBars.Add(progressBar);
        }
        public StatusStream()
        {
            textBoxes = new List<TextBox>();
            progressBars = new List<ProgressBar>();
            verbosity = Verbosity.Low;
        }


        //-- Attach/Detatch the console, textboxes, or progressBars to/from this stream --//
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
        public void attachProgressBar(ProgressBar bar)
        {
            if (!progressBars.Contains(bar))
                progressBars.Add(bar);
        }
        public void detatchProgressBar(ProgressBar bar)
        {
            progressBars.Remove(bar);
        }

        //-- Set maximum size for attached progress bars --//
        public void SetProgressBarMaximum(int max)
        {
            foreach (ProgressBar p in progressBars)
            {
                //If this thread is not the textbox's thread, invoke the update method
                if (p.InvokeRequired)
                    p.Invoke(new UpdateProgressDelegate(this.SetMaximumInvoker), max, p);
                else
                    SetMaximumInvoker(max, p);
            }
        }
        private void SetMaximumInvoker(int max, ProgressBar p)
        {
            p.Value = 0;
            p.Maximum = max;
        }

        //-- Update progress bars by delta --//
        public void UpdateProgress()
        {
            UpdateProgress(1);
        }
        public void UpdateProgress(int delta)
        {
            foreach (ProgressBar p in progressBars)
            {
                //If this thread is not the textbox's thread, invoke the update method
                if (p.InvokeRequired)
                    p.Invoke(new UpdateProgressDelegate(this.UpdateProgressInvoker), delta, p);
                else
                    UpdateProgressInvoker(delta, p);
            }
        }
        private void UpdateProgressInvoker(int delta, ProgressBar p)
        {
            p.Increment(delta);
        }

        //-- Append methods with formatting using the lowest verbosity --// 
        //--    Can't use default parameter for verbosity because of ambiguity between and int type first format parameter --//
        //--    with no verbosity and Verbosity but no parameters --//
        public void AppendFormatLine(String format, params object[] args)
        {
            AppendFormatText(format + "\n", args);
        }
        public void AppendFormatText(String format, params object[] args)
        {
            AppendFormatText(Verbosity.Warnings, format, args);
        }

        //-- Append methods with formatting using the input verbosity --//
        public void AppendFormatLine(Verbosity verb, String format, params object[] args)
        {
            AppendFormatText(verb, format + "\n", args);
        }
        public void AppendFormatText(Verbosity verb, String format, params object[] args)
        {
            AppendText(verb, String.Format(format, args));
        }

        //-- Append methods without formatting - default verbosity is the lowest --//
        public void AppendLine(String text)
        {
            AppendText(text + "\n");
        }
        public void AppendLine()
        {
            AppendText("\n");
        }
        public void AppendLine(Verbosity verb)
        {
            AppendText(verb, "\n");
        }
        public void AppendLine(Verbosity verb, String text)
        {
            AppendText(verb, text + "\n");
        }
        public void AppendText(String text)
        {
            AppendText(Verbosity.Warnings, text);
        }
        public void AppendText(Verbosity verb, String text)
        {
            if (outputToConsole && verbosity >= verb)
                System.Console.Write(text);

            foreach (TextBox t in textBoxes)
            {
                //If this thread is not the textbox's thread, invoke the update method
                if (t.InvokeRequired)
                    t.Invoke(new AppendTextDelegate(this.AppendTextInvoker), verb, text, t);
                else
                    AppendTextInvoker(verb, text, t);
            }
        }

        //-- Write 'text' to textbox b if 'verb' is equal to or lower than this streams verbosity --//
        private void AppendTextInvoker(Verbosity verb, String text, TextBox b)
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
