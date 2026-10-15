using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using SimpleZip.Core;
using SimpleZip.Services;
using Path = System.IO.Path;

namespace SimpleZip
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string _routeOriginFolder;
        private const string FolderZips = "SimpleZip - Carpetas comprimidas";
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void StartCompressBtn(object sender, RoutedEventArgs e)
        {
            // Validar carpeta ingresada
            string folderName = TxtFolderName.Text.Trim();
            string errMsg = FolderValidations.ValidateFolderName(folderName);

            if(!string.IsNullOrEmpty(errMsg))
            {
                ShowErrorMessage(errMsg);
                return;
            }

            // Definir ruta en escritorio
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string zipFolderPath = Path.Combine(desktopPath, FolderZips);

            // Crear carpeta (En caso de no existir)
            Directory.CreateDirectory(zipFolderPath);

            // Resolver el nombre final
            string finalName = ResolveZipFileName(zipFolderPath, folderName);
            if (finalName == string.Empty) return;

            // Preparar UI para procesamiento
            TxtFolderName.Text = finalName;
            TxtFolderName.IsEnabled = false;
            BtnCompress.IsEnabled = false;
            BtnCompress.Content = "Procesando ...";

            // Inicializamos compresión
            try
            {
                // Comprensión en segundo plano
                string finalRoute = await Task.Run(() => 
                {
                    return SimpleZip.Core.Compresor.Compress(_routeOriginFolder, zipFolderPath, finalName);
                });

                MessageBox.Show($"¡Listo! Tu archivo quedó en la carpeta '{FolderZips}' del Escritorio: \n\n{finalRoute}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{finalRoute}\"");

            }
            catch(InvalidOperationException ex)
            {
                ShowErrorMessage(ex.Message);
            }
            catch (Exception)
            {
                ShowErrorMessage("Ocurrió un error inesperado. Cierre los documentos de esa carpeta e inténtelo de nuevo");
            }
            finally
            {
                BtnCompress.IsEnabled = true;
                BtnCompress.Content = "Comprimir";
                TxtFolderName.IsEnabled = true;

                ResetUI();
            }

        }
        
        private void DropZoneDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                DropZoneBorder.Background = ColorUtilities.GetColorBrush(BackgroundStates.Hover);
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void DropZoneDragLeave(object sender, DragEventArgs e)
        {
            DropZoneBorder.Background = ColorUtilities.GetColorBrush(BackgroundStates.Default);
        }

        private void DropZoneDrop(object sender, DragEventArgs e)
        {
            DropZoneBorder.Background = ColorUtilities.GetColorBrush(BackgroundStates.Default);

            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                ShowErrorMessage("El elemento agregado no es válido, por favor verifique");
                return;
            }

            string[] routes = (string[])e.Data.GetData(DataFormats.FileDrop);

            if(routes.Length > 1)
            {
                ShowErrorMessage("Por favor, arrastra solo una carpeta a la vez");
                return;
            }

            string route = routes[0];

            ProcessRoute(route);
        }

        private void SelectFolderBtn(object sender, RoutedEventArgs e)
        {
            string route = FolderSelector.GetFolderRoute();

            if(!string.IsNullOrEmpty(route)) ProcessRoute(route);
        }

        private void ExploreDirectoryBtn(object sender, RoutedEventArgs e)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string zipFolderPath = Path.Combine(desktopPath, FolderZips);

            try
            {
                Directory.CreateDirectory(zipFolderPath);
                System.Diagnostics.Process.Start("explorer.exe", zipFolderPath);
            }
            catch (Exception)
            {
                ShowErrorMessage("No se pudo abrir la carpeta de destino.");
            }
        }

        private void RetryBtn(object sender, RoutedEventArgs e)
        {
            ResetUI();
            SelectFolderBtn(sender, e);
        }

        private void ProcessRoute(string route)
        {
            if (File.Exists(route))
            {
                ShowErrorMessage("Has seleccionado un archivo suelto. Por favor, selecciona o arrastra una carpeta completa");
                return;
            }

            if (!Directory.Exists(route))
            {
                ShowErrorMessage("El elemento agregado no es una carpeta, por favor verifique");
                return;
            }

            _routeOriginFolder = route;
            string cleanRoute = route.TrimEnd(Path.DirectorySeparatorChar);
            string folderName = Path.GetFileName(cleanRoute);

            TxtFolderName.Text = folderName;
            StateEmpty.Visibility = Visibility.Collapsed;
            StateLoaded.Visibility = Visibility.Visible;

            TxtFolderName.SelectAll();
            TxtFolderName.Focus();
        }

        private string ResolveZipFileName(string destinationPath, string filename)
        {
            string baseName = filename.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) ? filename.Substring(0, filename.Length - 4) : filename;
            string finalZipPath = Path.Combine(destinationPath, $"{baseName}.zip");

            if (!File.Exists(finalZipPath))
            {
                return baseName;
            }

            string message = $"Ya existe un archivo llamado '{Path.GetFileName(finalZipPath)}' en la carpeta '{FolderZips}' del Escritorio.\n\n" +
                     "• [Sí]: Reemplazar el archivo existente.\n" +
                     "• [No]: Guardar una copia con un número correlativo.\n" +
                     "• [Cancelar]: Detener la operación.";

            var result = MessageBox.Show(message, "Archivo existente", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    return baseName;

                case MessageBoxResult.No:
                    int counter = 1;
                    
                    while(File.Exists(Path.Combine(destinationPath, $"{baseName} ({counter}).zip")))
                    {
                        counter++;
                    }
                    return $"{baseName} ({counter})";

                case MessageBoxResult.Cancel:
                default:
                    return string.Empty;
            }
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void ResetUI()
        {
            _routeOriginFolder = null;
            TxtFolderName.Text = "";
            StateEmpty.Visibility = Visibility.Visible;
            StateLoaded.Visibility = Visibility.Collapsed;
        }
    }
}
