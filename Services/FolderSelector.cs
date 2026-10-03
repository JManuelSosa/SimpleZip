using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinForms = System.Windows.Forms;

namespace SimpleZip.Services
{
    public static class FolderSelector
    {
        public static string GetFolderRoute()
        {
            using (var dialog = new WinForms.FolderBrowserDialog())
            {
                dialog.Description = "Selecciona la carpeta que deseas comprimir";
                dialog.ShowNewFolderButton = false;

                if(dialog.ShowDialog() == WinForms.DialogResult.OK)
                {
                    return dialog.SelectedPath;
                }

                return null;
            }
        }
    }
}
