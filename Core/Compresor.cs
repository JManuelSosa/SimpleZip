using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Para compresión 
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace SimpleZip.Core
{
    public static class Compresor
    {
        public static string Compress(string originalFolder, string destinationFolder, string folderName)
        {
            // Verificación y normalización de rutas
            if (string.IsNullOrWhiteSpace(originalFolder) || string.IsNullOrWhiteSpace(destinationFolder)) 
                throw new InvalidOperationException("No se puede operar sobre una carpeta sin nombre");

            if (!Directory.Exists(originalFolder)) 
                throw new InvalidOperationException("La carpeta ingresada no existe o no se puede operar sobre ella");
            
            string originFolderNorm = Path.GetFullPath(originalFolder).TrimEnd('\\') + '\\';
            string destinationFolderNorm = Path.GetFullPath(destinationFolder).TrimEnd('\\') + '\\';

            if (destinationFolderNorm.StartsWith(originFolderNorm, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("No se puede guardar el Zip dentro de la misma carpeta que se comprime");

            // Preparación de rutas para operar
            string baseRoute = Path.Combine(destinationFolderNorm, folderName);
            string finalRoute = baseRoute + ".zip";
            string tmpRoute = finalRoute + ".tmp";

            // Creación del .zip
            ZipFile.CreateFromDirectory(originFolderNorm, tmpRoute, CompressionLevel.Optimal, true);

            // Comprobación de existencia y manejo de duplicados
            if (File.Exists(finalRoute))
            {
                string filename = Path.GetFileNameWithoutExtension(finalRoute);
                string extension = Path.GetExtension(finalRoute);
                string directory = Path.GetDirectoryName(finalRoute);

                bool limitFound = false;
                int counterCopies = 1;

                while(!limitFound)
                {
                    string filenameFind = Path.Combine(directory, $"{filename} ({counterCopies}){extension}");

                    if (!File.Exists(filenameFind))
                    {
                        finalRoute = filenameFind;
                        limitFound = true;
                    }

                    counterCopies++;
                }
            }

            // Mover el zip final
            File.Move(tmpRoute, finalRoute);

            // Retornar la nueva ruta
            return finalRoute;
        }

    }
}
