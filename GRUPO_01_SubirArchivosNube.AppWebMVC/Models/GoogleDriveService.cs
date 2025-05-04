using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google;

namespace GRUPO_01_SubirArchivosNube.AppWebMVC.Models
{
    public class GoogleDriveService
    {

        #region metodos para archivos
        //Service
        //Servicioo principal dde Google drive para  realizar las operaciones
        private readonly DriveService _driveService;

        // Constructor: Autenticación con OAuth 2.0 utilizando credenciales de Google
        public GoogleDriveService(string clientId, string clientSecret)
        {
            var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                new[] { DriveService.Scope.Drive }, // Permisos de acceso a Drive
                "user",
                CancellationToken.None,
                new FileDataStore("Drive.Auth.Store")).Result;

            // Inicializa el servicio de Google Drive con las credenciales obtenidas
            _driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential
            });
        }

        // Método para subir un archivo a Google Drive
        public string SubirArchivo(Stream fileStream, string nombreArchivo)
        {
            // Validar el stream del archivo antes de procesarlo para ver que el archivo no este vacío
            if (fileStream == null || fileStream.Length == 0)
            {
                throw new Exception("El archivo está vacío o no se pudo acceder al stream.");
            }

            //Crear la metadata del archivo
            var archivoMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = nombreArchivo
            };

            //Configurar y ejecutar la solicitud de subida
            var solicitud = _driveService.Files.Create(
                archivoMetadata, fileStream, "application/octet-stream");
            solicitud.Fields = "id";
            solicitud.Upload();

            // Validación de la respuesta
            if (solicitud.ResponseBody == null)
            {
                throw new Exception("La respuesta del servidor es nula. Verifica la configuración de la solicitud.");
            }


            return solicitud.ResponseBody.Id; // Devuelve el ID del archivo subido
        }

       

        // Método auxiliar para obtener una descripción simplificada del tipo de archivo
        private string ObtenerTipoSimple(string mimeType)
        {
            if (string.IsNullOrEmpty(mimeType)) return "Desconocido";

            if (mimeType.Contains("image")) return "Imagen";
            if (mimeType.Contains("pdf")) return "PDF";
            if (mimeType.Contains("video")) return "Video";
            if (mimeType.Contains("spreadsheet")) return "Hoja de cálculo";
            if (mimeType.Contains("document")) return "Documento";
            if (mimeType.Contains("folder")) return "Carpeta";
            return "Archivo";
        }

        // Método para descargar un archivo desde Google Drive
        public (MemoryStream Stream, string FileName) DescargarArchivo(string fileId)
        {
            try
            {
                // Obtiene la información del archivo desde Google Drive y prepara la solicitud de la descarga
                var file = _driveService.Files.Get(fileId).Execute();
                var request = _driveService.Files.Get(fileId);

                var stream = new MemoryStream();
                request.Download(stream);
                stream.Seek(0, SeekOrigin.Begin); // Reiniciamos la posición del stream
                // Retorna el stream del archivo y su nombre
                return (stream, file.Name);
            }
            catch (GoogleApiException ex)
            {
                throw new Exception($"Error al descargar el archivo: {ex.Message}");
            }
        }

        // Método para eliminar un archivo de Google Drive
        public bool EliminarArchivo(string fileId)
        {
            try
            {
                // Ejecuta la solicitud para eliminar el archivo por su ID
                _driveService.Files.Delete(fileId).Execute();
                return true;// Retorna verdadero si la eliminación fue exitosa
            }
            catch (GoogleApiException ex)
            {
                // Captura errores de la API y los muestra en consola
                Console.WriteLine($"Error al eliminar archivo: {ex.Message}");
                return false; // Retorna falso si la eliminación falla

            }
        }

        #endregion

        #region metodos para carpetas
        // Método para crear una carpeta en Google Drive
        public string CrearCarpeta(string nombreCarpeta)
        {
            var carpetaMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = nombreCarpeta,
                MimeType = "application/vnd.google-apps.folder" // Define que es una carpeta
            };

            // Crea la carpeta en Google Drive
            var solicitud = _driveService.Files.Create(carpetaMetadata);
            solicitud.Fields = "id"; // Solo obtiene el ID de la carpeta
            var carpeta = solicitud.Execute();

            return carpeta.Id; // Devuelve el ID de la carpeta creada
        }

        // Método para listar todas las carpetas en Drive
        public IList<Google.Apis.Drive.v3.Data.File> ListarCarpetas()
        {
            try
            {
                var solicitud = _driveService.Files.List();
                solicitud.Fields = "files(id, name, mimeType)";
                solicitud.Q = "mimeType='application/vnd.google-apps.folder'"; // Filtro solo para carpetas
                var carpetas = solicitud.Execute();
                return carpetas.Files; // Retorna solo las carpetas
            }
            catch (GoogleApiException ex)
            {
                throw new Exception($"Error al listar carpetas: {ex.Message}");
            }
        }

        // Método para subir un archivo dentro de una carpeta específica
        public string SubirArchivoEnCarpeta(Stream fileStream, string nombreArchivo, string carpetaId)
        {
            var archivoMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = nombreArchivo,
                Parents = new List<string> { carpetaId } // Asigna el archivo a la carpeta
            };

            var solicitud = _driveService.Files.Create(archivoMetadata, fileStream, "application/octet-stream");
            solicitud.Fields = "id";
            solicitud.Upload();

            // Validación de la respuesta
            if (solicitud.ResponseBody == null)
            {
                throw new Exception("La respuesta del servidor es nula. Verifica la configuración.");
            }

            return solicitud.ResponseBody.Id; // Devuelve el ID del archivo subido
        }

        // Método para eliminar una carpeta de Google Drive
        public bool EliminarCarpeta(string carpetaId)
        {

            try
            {
                // Ejecuta la solicitud de eliminación
                var resultado = _driveService.Files.Delete(carpetaId).Execute();

                // Si no lanza excepción, se considera exitosa
                return true;
            }
            catch (GoogleApiException ex)
            {
                // Loguear el error específico de la API y lanzar excepción para manejo externo
                Console.WriteLine($"Error específico de Google API: {ex.Message}");
                throw new Exception($"Error al eliminar la carpeta: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Loguear otros errores posibles
                Console.WriteLine($"Error general: {ex.Message}");
                throw new Exception($"Error inesperado: {ex.Message}");
            }

        }

        // Método para listar archivos dentro de una carpeta específica
        public IList<GoogleDrive> ListarArchivosEnCarpeta(string carpetaId)
        {
            try
            {
                var solicitud = _driveService.Files.List();
                solicitud.Fields = "files(id, name, mimeType, size, createdTime, modifiedTime, webViewLink)";
                solicitud.Q = $"'{carpetaId}' in parents"; // Filtra archivos dentro de la carpeta

                var archivos = solicitud.Execute();

                return archivos.Files?.Select(f => new GoogleDrive
                {
                    Id = f.Id,
                    Nombre = f.Name,
                    Tipo = ObtenerTipoSimple(f.MimeType),
                    Tamano = f.Size,
                    FechaCreacion = f.CreatedTime,
                    FechaModificacion = f.ModifiedTime,
                    Url = f.WebViewLink
                }).ToList() ?? new List<GoogleDrive>();
            }
            catch (GoogleApiException ex)
            {
                throw new Exception($"Error al listar archivos en la carpeta: {ex.Message}");
            }
        }

        public IList<GoogleDrive> ListarArchivos()
        {
            try
            {
                var solicitud = _driveService.Files.List();
                solicitud.Fields = "files(id, name, mimeType, size, createdTime, modifiedTime, webViewLink)";
                var archivos = solicitud.Execute();

                return archivos.Files?.Select(f => new GoogleDrive
                {
                    Id = f.Id, // Ahora compatible (string)
                    Nombre = f.Name,
                    Tipo = ObtenerTipoSimple(f.MimeType), // Función auxiliar
                    Tamano = f.Size,
                    FechaCreacion = f.CreatedTime,
                    FechaModificacion = f.ModifiedTime,
                    Url = f.WebViewLink
                }).ToList() ?? new List<GoogleDrive>();
            }
            catch (GoogleApiException ex)
            {
                Console.WriteLine($"Error al listar archivos: {ex.Message}");
                throw new Exception("No se pudieron listar los archivos. Verifica los permisos.");
            }
        }
        #endregion
    }
}
