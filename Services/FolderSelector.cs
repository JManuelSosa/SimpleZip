using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace SimpleZip.Services
{
    public static class FolderSelector
    {
        // Opciones del diálogo nativo
        private const uint FOS_PICKFOLDERS = 0x00000020;
        private const uint FOS_FORCEFILESYSTEM = 0x00000040;

        /// <summary>
        /// Abre el explorador moderno de Windows configurado solo para seleccionar carpetas.
        /// </summary>
        public static string GetFolderRoute()
        {
            IntPtr hwndOwner = IntPtr.Zero;

            // Intentamos obtener el handle de la ventana principal de WPF para que el diálogo sea modal
            if (Application.Current?.MainWindow != null)
            {
                hwndOwner = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            }

            // Instanciamos el diálogo nativo de Windows
            var dialog = (IFileOpenDialog)new FileOpenDialog();

            try
            {
                // Configuramos para que solo permita seleccionar carpetas
                dialog.GetOptions(out uint options);
                dialog.SetOptions(options | FOS_PICKFOLDERS | FOS_FORCEFILESYSTEM);

                // Mostramos el diálogo
                int hr = dialog.Show(hwndOwner);

                // Si el usuario cancela (hr == 0x800704C7), retornamos null
                if (hr != 0) return null;

                // Obtenemos el resultado seleccionado
                dialog.GetResult(out IShellItem shellItem);
                shellItem.GetDisplayName(SIGDN.SIGDN_FILESYSPATH, out string path);

                return path;
            }
            catch
            {
                return null;
            }
            finally
            {
                // Liberamos la memoria COM
                if (dialog != null) Marshal.ReleaseComObject(dialog);
            }
        }

        #region COM Interop Definitions (API Nativa de Windows)

        [ComImport, Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")]
        private class FileOpenDialog { }

        [ComImport, Guid("d57c7288-d4ad-4768-be02-9d969532d960"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IFileOpenDialog
        {
            [PreserveSig] int Show(IntPtr parent);
            void SetFileTypes(uint cFileTypes, IntPtr rgFilterSpec);
            void SetFileTypeIndex(uint iFileType);
            void GetFileTypeIndex(out uint piFileType);
            void Advise(IntPtr pfde, out uint pdwCookie);
            void Unadvise(uint dwCookie);
            void SetOptions(uint fos);
            void GetOptions(out uint fos);
            void SetDefaultFolder(IShellItem psi);
            void SetFolder(IShellItem psi);
            void GetFolder(out IShellItem ppsi);
            void GetCurrentSelection(out IShellItem ppsi);
            void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);
            void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
            void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);
            void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
            void GetResult(out IShellItem ppsi);
            void AddPlace(IShellItem psi, uint alignment);
            void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
            void Close(int hr);
            void SetClientGuid(ref Guid guid);
            void ClearClientData();
            void SetFilter(IntPtr pFilter);
            void GetResults(out IntPtr ppenum);
            void GetSelectedItems(out IntPtr ppsai);
        }

        [ComImport, Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItem
        {
            void BindToHandler(IntPtr pbc, ref Guid bhid, ref Guid riid, out IntPtr ppv);
            void GetParent(out IShellItem ppsi);
            void GetDisplayName(SIGDN sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);
            void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);
            void Compare(IShellItem psi, uint hint, out int piOrder);
        }

        private enum SIGDN : uint
        {
            SIGDN_FILESYSPATH = 0x80058000
        }

        #endregion
    }
}