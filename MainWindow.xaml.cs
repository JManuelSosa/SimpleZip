using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using SimpleZip.Core;
using SimpleZip.Services;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using Path = System.IO.Path;


namespace SimpleZip
{
    public partial class MainWindow : Window
    {
        private string _routeOriginFolder;
        private const string FolderZips = "SimpleZip - Carpetas comprimidas";
        private CancellationTokenSource _cancellationTokenSource;
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
                ShowCustomMessage(errMsg, "Atención");
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

            HandleStartCompressUI(finalName);

            _cancellationTokenSource = new CancellationTokenSource();

            // Inicializamos compresión
            try
            {
                // Comprensión en segundo plano
                string finalRoute = await Task.Run(() => 
                {
                    return SimpleZip.Core.Compresor.Compress(_routeOriginFolder, zipFolderPath, finalName, _cancellationTokenSource.Token);
                }, _cancellationTokenSource.Token);

                ShowCustomMessage($"¡Listo! Tu archivo quedó en la carpeta '{FolderZips}' del Escritorio: \n\n{finalRoute}", "Compresión exitosa");
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{finalRoute}\"");

            }
            catch(OperationCanceledException)
            {
                ShowCustomMessage("La compresión fue cancelada existosamente", "Cancelar");
                return;
            }
            catch(Exception ex) when (ex is InvalidOperationException || ex is IOException || ex is UnauthorizedAccessException)
            {
                ShowCustomMessage(ex.Message, "Error");
            }
            catch (Exception)
            {
                ShowCustomMessage("Ocurrió un error inesperado. Cierre los documentos de esa carpeta e inténtelo de nuevo", "Error");
            }
            finally
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;

                HandleFinalizeProcessUI();
                ResetUI();
            }

        }
        
        private void DropZoneDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                DropZoneBorder.Background = ColorUtilities.BackgroundDropZoneColor(UIColorStates.Hover);
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void DropZoneDragLeave(object sender, DragEventArgs e)
        {
            DropZoneBorder.Background = ColorUtilities.BackgroundDropZoneColor(UIColorStates.Default);
        }

        private void DropZoneDrop(object sender, DragEventArgs e)
        {
            DropZoneBorder.Background = ColorUtilities.BackgroundDropZoneColor(UIColorStates.Default);

            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                ShowCustomMessage("El elemento agregado no es válido, por favor verifique", "Atención");
                return;
            }

            string[] routes = (string[])e.Data.GetData(DataFormats.FileDrop);

            if(routes.Length > 1)
            {
                ShowCustomMessage("Por favor, arrastra solo una carpeta a la vez", "Atención");
                return;
            }

            string route = routes[0];

            ProcessFolderRoute(route);
        }

        private void SelectFolderBtn(object sender, RoutedEventArgs e)
        {
            string route = FolderSelector.GetFolderRoute();
            if(!string.IsNullOrEmpty(route)) ProcessFolderRoute(route);
        }

        private void ExploreDirectoryBtn(object sender, RoutedEventArgs e)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string zipFolderPath = Path.Combine(desktopPath, FolderZips);

            try
            {
                Directory.CreateDirectory(zipFolderPath);

                if (!WindowHelper.FocusIfAlreadyOpen(zipFolderPath))
                {
                    System.Diagnostics.Process.Start("explorer.exe",zipFolderPath);
                }
            }
            catch (Exception)
            {
                ShowCustomMessage("No se pudo abrir la carpeta de destino.", "Error");
            }
        }

        private void RetryBtn(object sender, RoutedEventArgs e)
        {
            ResetUI();
            SelectFolderBtn(sender, e);
        }

        private void CancelCompressBtn(object sender, RoutedEventArgs e)
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();

                // UI de cancelación
                BtnCancel.IsEnabled = false;
                BtnCancel.Background = ColorUtilities.BtnCancelColor(UIColorStates.Hover);

                TxtProcesingCompress.Text = "Cancelando comprensión, por favor espere ...";
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                string msg = "Hay una compresión en proceso. Si cierras ahora, el archivo no se completará. \n\n" +
                "¿Deseas cerrar de todos modos?";

                var result = ShowCustomMessage(msg, "¿Estas seguro que quieres salir?", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                _cancellationTokenSource.Cancel();
            }

            base.OnClosing(e);
        }

        private string ResolveZipFileName(string destinationPath, string filename)
        {
            string baseName = FileService.RemoveExtension(filename, ".zip");
            string finalZipPath = Path.Combine(destinationPath, $"{baseName}.zip");

            if (!File.Exists(finalZipPath)) return baseName;

            string message = $"Ya existe un archivo llamado '{Path.GetFileName(finalZipPath)}' en la carpeta '{FolderZips}' del Escritorio.\n\n" +
                     "¿Desea reemplazar la carpeta? \n\n" +
                     "Sí: Reemplazar el archivo existente.\n" +
                     "No: Guardar una copia con un número correlativo.\n";

            var result = ShowCustomMessage(message, "Archivo existente", MessageBoxButton.YesNoCancel);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    return baseName;

                case MessageBoxResult.No:
                    return FileService.GetNextAvailableNumberedZip(destinationPath, baseName);

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

        private void ProcessFolderRoute(string route)
        {
            string error = FolderValidations.ValidateFolder(route);

            if (!string.IsNullOrEmpty(error))
            {
                ShowCustomMessage(error, "Atención");
                return;
            }

            LoadSelectedFolder(route);
        }

        private void LoadSelectedFolder(string folderPath)
        {
            _routeOriginFolder = folderPath;
            string cleanRoute = folderPath.TrimEnd(Path.DirectorySeparatorChar);
            string folderName = Path.GetFileName(cleanRoute);

            TxtFolderName.Text = folderName;
            StateEmpty.Visibility = Visibility.Collapsed;
            StateLoaded.Visibility = Visibility.Visible;

            TxtFolderName.SelectAll();
            TxtFolderName.Focus();
        }

        private void HandleStartCompressUI(string textFolderName)
        {
            // Nombre de carpeta
            TxtFolderName.Text = textFolderName;
            TxtFolderName.IsEnabled = false;

            // Botón compresión
            BtnCompress.IsEnabled = false;
            BtnCompress.Visibility = Visibility.Collapsed;

            // Grid de boton de cancelar y boton de comprimir
            GridUIBtns.Margin = new Thickness(GridUIBtns.Margin.Left, 8, GridUIBtns.Margin.Right, GridUIBtns.Margin.Bottom);

            // Texto de mensaje de compresión
            TxtProcesingCompress.Text = "Comprimiendo carpeta por favor espere ...";
            TxtProcesingCompress.Visibility = Visibility.Visible;

            // Botón de cancelación
            BtnCancel.IsEnabled = true;
            BtnCancel.Visibility = Visibility.Visible;

            // Botón de seleccionar otra carpeta
            BtnRetry.IsEnabled = false;
            BtnRetry.Background = ColorUtilities.BtnRetryColor(UIColorStates.Hover);
        }

        private void HandleFinalizeProcessUI()
        {
            // Nombre carpeta
            TxtFolderName.IsEnabled = true;

            // Botón compresión
            BtnCompress.IsEnabled = true;
            BtnCompress.Visibility = Visibility.Visible;

            // Grid de boton de cancelar y boton de comprimir
            GridUIBtns.Margin = new Thickness(GridUIBtns.Margin.Left, 20, GridUIBtns.Margin.Right, GridUIBtns.Margin.Bottom);

            //Texto de mensaje de compresión
            TxtProcesingCompress.Visibility = Visibility.Collapsed;

            // Botón de cancelación
            BtnCancel.IsEnabled = false;
            BtnCancel.Background = ColorUtilities.BtnCancelColor(UIColorStates.Default);
            BtnCancel.Visibility = Visibility.Collapsed;

            // Botón de seleccionar otra carpeta
            BtnRetry.IsEnabled = true;
            BtnRetry.Background = ColorUtilities.BtnRetryColor(UIColorStates.Default);
        }

        private MessageBoxResult ShowCustomMessage(string message, string title = "SimpleZip", MessageBoxButton buttons = MessageBoxButton.OK)
        {
            var msgBox = new CustomMessageBox(message, title, buttons);
            msgBox.Owner = this;
            msgBox.ShowDialog();
            return msgBox.Result;
        }

    }
}
