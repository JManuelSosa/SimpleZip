using System.IO;
using System.Windows;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartCompressBtn(object sender, RoutedEventArgs e)
        {

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

        }

        private void ProcessRoute(string route)
        {
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
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
