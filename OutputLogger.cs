using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ProyectoFinalAmaury
{
    class OutputLogger
    {
        TextBox OutputLex;
        TextBox OutputSin;

        public void Init(TextBox lexLog, TextBox sinLog)
        {
            OutputLex = lexLog;
            OutputSin = sinLog;
            OutputLex.Text = "";
            OutputSin.Text = "";
        }

        public void LogLexico(string texto)
        {
            OutputLex.Text += texto + Environment.NewLine;
        }

        public void LogSintactico(string texto)
        {
            OutputSin.Text += texto + Environment.NewLine;
        }
    }
}
