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
using SimpleZip.Core;

namespace SimpleZip
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void IniciarCompresion(object sender, System.Windows.RoutedEventArgs e)
        {
            // Obtener valores de la interfaz
            string origin = txtOriginRoute.Text.Trim();
            string name = txtNameZip.Text.Trim();

            string destiny = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            try
            {
                // Limpiar mensajes anteriores
                txtResultado.Foreground = System.Windows.Media.Brushes.Black;

                // Llamar a la lógica central
                string finalRoute = Compresor.Compress(origin, destiny, name);

                // Si llega aquí, fue exitoso
                txtResultado.Text = $"¡Éxito! El archivo se guardó en:\n{finalRoute}";
                txtResultado.Foreground = System.Windows.Media.Brushes.Green;
            }
            catch (Exception ex)
            {
                txtResultado.Text = $"Error inesperado: {ex.Message}";
                txtResultado.Foreground = System.Windows.Media.Brushes.Red;
            }
            
        }
    }
}
