using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Para compresión 
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Threading;

namespace SimpleZip.Core
{
    public static class Compresor
    {
        public static string Compress(string originalFolder, string destinationFolder, string folderName, CancellationToken cancellationToken = default)
        {
            // Verificación y normalización de rutas
            if (string.IsNullOrWhiteSpace(originalFolder) || string.IsNullOrWhiteSpace(destinationFolder)) 
                throw new InvalidOperationException("No se puede operar sobre una carpeta sin nombre");

            if (!Directory.Exists(originalFolder)) 
                throw new InvalidOperationException("La carpeta ingresada no existe o no se puede operar sobre ella");

            if (!Directory.EnumerateFileSystemEntries(originalFolder).Any())
                throw new InvalidOperationException("La carpeta esta vacía, agregue al menos 1 archivo para comprimir");

            string originFolderNorm = Path.GetFullPath(originalFolder).TrimEnd('\\') + '\\';
            string destinationFolderNorm = Path.GetFullPath(destinationFolder).TrimEnd('\\') + '\\';

            if (destinationFolderNorm.StartsWith(originFolderNorm, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("No se puede guardar el Zip dentro de la misma carpeta que se comprime");

            CleanTmpFiles(destinationFolderNorm);

            // Preparación de rutas para operar
            string baseRoute = Path.Combine(destinationFolderNorm, folderName);
            string finalRoute = baseRoute + ".zip";
            string tmpRoute = finalRoute + ".tmp";

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                // Creación del .zip
                ZipFile.CreateFromDirectory(originFolderNorm, tmpRoute, CompressionLevel.Optimal, true);
                cancellationToken.ThrowIfCancellationRequested();

                if (File.Exists(finalRoute))
                {
                    File.Delete(finalRoute);
                }

                // Mover el zip final
                File.Move(tmpRoute, finalRoute);

                // Retornar la nueva ruta
                return finalRoute;
            }
            catch(OperationCanceledException)
            {
                throw;
            }
            catch (UnauthorizedAccessException) {
                throw new InvalidOperationException("No tenemos permiso para acceder a algunos archivos, asegúrese que no estan siendo usados por otro programa");
            }
            catch (PathTooLongException)
            {
                throw new InvalidOperationException("La ruta de la carpeta es demasiado larga, intenta mover la carpeta a un lugar más cercano al disco (C:)");
            }
            catch(IOException ioEx)
            {
                throw new InvalidOperationException("No se pudo crear el archivo. Es posible que otro programa esté usando la carpeta o que el disco esté lleno.", ioEx);
            }
            finally
            {
                if (File.Exists(tmpRoute))
                {
                    try
                    {
                        File.Delete(tmpRoute);
                    }
                    catch { }
                }
            }            
        }

        private static void CleanTmpFiles(string destinationFolderNorm)
        {
            try
            {
                string[] tmpFiles = Directory.GetFiles(destinationFolderNorm, "*.tmp");

                foreach (string tmpFile in tmpFiles)
                {
                    File.Delete(tmpFile);
                }
            }
            catch { }
        }
    }
}
