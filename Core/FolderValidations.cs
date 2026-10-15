using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace SimpleZip.Core
{
    public static class FolderValidations
    {
        public static string ValidateFolderName(string folderName)
        {
            string errMsg = string.Empty;

            if (string.IsNullOrWhiteSpace(folderName))
            {
                errMsg = "Por favor, escribe un nombre para tu archivo comprimido.";
                return errMsg;
            }

            if(folderName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                errMsg = "El nombre de la carpeta no puede contener estos caracteres: \\ / : * ? \" < > |";
                return errMsg;
            }

            string[] reservedNames = { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };

            if (reservedNames.Contains(Path.GetFileNameWithoutExtension(folderName).ToUpperInvariant()))
            {
                errMsg = "El nombre elegido no es válido en Windows, favor de cambiarlo";
                return errMsg;
            }

            return errMsg;
        }
    }
}
