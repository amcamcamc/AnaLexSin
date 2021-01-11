using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;

namespace ProyectoFinalAmaury
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MenuPrincipal : Window
    {
        AnaLex analex = new AnaLex();
        AnaSin anasin = new AnaSin();
        OpenFileDialog ofd = new OpenFileDialog();
        OutputLogger Logger = new OutputLogger();

        public MenuPrincipal()
        {
            InitializeComponent();
        }

        private void ScrollCambiado(object sender, ScrollChangedEventArgs e)
        {
            if (sender == ScrollCajaCodigo)
            {
                ScrollLineador.ScrollToVerticalOffset(e.VerticalOffset);
                ScrollLineador.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
            else
            {
                ScrollCajaCodigo.ScrollToVerticalOffset(e.VerticalOffset);
                ScrollCajaCodigo.ScrollToHorizontalOffset(e.HorizontalOffset);
            }
        }

        private void ActualizarNumeroLineas(object sender, TextChangedEventArgs e)
        {
            if (Lineador != null)
            {
                var numeroLineas = CajaCodigo.LineCount;
                Lineador.Clear();
                for (int i = 1; i < numeroLineas; i++)
                {
                    Lineador.Text += i + Environment.NewLine;
                }
            }
        }

        //funcionamiento general

        private void ImportarArchivo(object sender, RoutedEventArgs e)
        {
            ofd.Filter = "MIO File (*.mio)|*.mio|Text files (*.txt)|*.txt|All files (*.*)|*.*";
            ofd.ShowDialog();
            if (ofd.FileName != string.Empty)
            {
                CajaCodigo.Text = File.ReadAllText(ofd.FileName, Encoding.Default);
            }
            Logger.Init(CajaLexico, CajaSintactico);
            ANALISIS_LEXICO_BOTON.IsEnabled = true;
        }

        private void AnalizarLexico(object sender, RoutedEventArgs e)
        {
            if (ofd.FileName != string.Empty)
            {
                analex.Analizar(ofd.FileName, Logger);
            }
            ANALISIS_SINTACTICO_BOTON.IsEnabled = true;
        }

        private void AnalizarSintaxis(object sender, RoutedEventArgs e)
        {
            if (File.Exists(analex.lexArchivoPath) && File.Exists(analex.simArchivoPath))
            {
                anasin.Analizar(analex.lexArchivoPath, analex.simArchivoPath, analex.tablaLineas, Logger);
            }
            else
            {
                Logger.LogSintactico("NO EXISTEN LOS ARCHIVOS .LEX Y .SIM");
            }
        }
    }
}
