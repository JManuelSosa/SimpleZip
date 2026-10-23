using System;
using System.IO;
using System.Runtime.InteropServices;
using SHDocVw;

namespace SimpleZip.Services
{
    public static class WindowHelper
    {
        // Constante para restaurar la ventana a su tamaño normal si está minimizada
        private const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// Comprueba si la ruta ya está abierta en el Explorador.
        /// Si existe, la restaura y la pone en primer plano.
        /// </summary>
        /// <returns>True si ya estaba abierta; False si no lo estaba.</returns>
        public static bool FocusIfAlreadyOpen(string targetPath)
        {
            // Normalizamos la ruta para que no fallen comparaciones por barras finales
            string normalizedTarget = Path.GetFullPath(targetPath).TrimEnd('\\');

            var shellWindows = new ShellWindows();

            foreach (InternetExplorer window in shellWindows)
            {
                if (window == null) continue;

                // Filtramos únicamente las ventanas de explorer.exe
                string processName = Path.GetFileNameWithoutExtension(window.FullName);
                if (string.Equals(processName, "explorer", StringComparison.OrdinalIgnoreCase))
                {
                    // LocationURL devuelve la ruta en formato URI (ej: "file:///C:/Users/...")
                    if (Uri.TryCreate(window.LocationURL, UriKind.Absolute, out Uri currentUri))
                    {
                        string localPath = currentUri.LocalPath.TrimEnd('\\');

                        if (string.Equals(localPath, normalizedTarget, StringComparison.OrdinalIgnoreCase))
                        {
                            IntPtr hwnd = (IntPtr)window.HWND;

                            // 1. Restaurar la ventana por si está minimizada
                            ShowWindowAsync(hwnd, SW_RESTORE);

                            // 2. Traerla al frente
                            SetForegroundWindow(hwnd);

                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
