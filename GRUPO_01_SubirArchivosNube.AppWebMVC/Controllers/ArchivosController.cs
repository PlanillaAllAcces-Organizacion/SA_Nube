using GRUPO_01_SubirArchivosNube.AppWebMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace GRUPO_01_SubirArchivosNube.AppWebMVC.Controllers
{
    public class ArchivosController : Controller
    {
        // Servicio para interactuar con Google Drive
        private readonly GoogleDriveService _driveService;

        // Constructor: Inicializa el servicio de Google Drive con credenciales desde la configuración
        public ArchivosController(IConfiguration configuration)
        {
            _driveService = new GoogleDriveService(
                configuration["GoogleDrive:ClientId"],
                configuration["GoogleDrive:ClientSecret"]);
        }

        [HttpGet]
        public IActionResult Subir()
        {
            // Renderiza la vista para subir archivos
            return View();
        }

        // Método para subir un archivo a Google Drive
        [HttpPost]
        public IActionResult Subir(IFormFile archivo)
        {
            if (archivo != null && archivo.Length > 0)
            {
                using (var stream = archivo.OpenReadStream())
                {
                    try
                    {
                        var id = _driveService.SubirArchivo(stream, archivo.FileName);
                        TempData["Mensaje"] = $"Archivo subido correctamente. ID: {id}";

                        // Redirige a la acción ListarArchivos después de subir
                        return RedirectToAction("ListarArchivos");
                    }
                    catch (Exception ex)
                    {
                        TempData["MensajeError"] = $"Error al subir el archivo: {ex.Message}";
                        return RedirectToAction("ListarArchivos");
                    }
                }
            }
            else
            {
                TempData["MensajeError"] = "Por favor, selecciona un archivo.";
                return RedirectToAction("ListarArchivos");
            }
        }

        // Método para mostrar la vista de creación de carpetas con la lista de carpetas existentes
        [HttpGet]
        public IActionResult CrearCarpeta()
        {
            // Recupera la lista de carpetas existentes
            var carpetas = _driveService.ListarCarpetas(); // Necesitamos un método para listar solo carpetas
            return View(carpetas); // Pasa la lista de carpetas a la vista

        }

        // Método para crear una carpeta en Google Drive
        [HttpPost]
        public IActionResult CrearCarpeta(string nombreCarpeta)
        {

            if (!string.IsNullOrEmpty(nombreCarpeta))
            {
                try
                {
                    // Crea la carpeta y guarda su ID
                    var idCarpeta = _driveService.CrearCarpeta(nombreCarpeta);
                    ViewBag.Mensaje = $"Carpeta creada correctamente. ID: {idCarpeta}";
                }
                catch (Exception ex)
                {
                    ViewBag.Mensaje = $"Error al crear la carpeta: {ex.Message}";
                }
            }
            else
            {
                ViewBag.Mensaje = "Por favor, ingresa un nombre para la carpeta.";
            }

            // Recupera la lista de carpetas existentes después de intentar crear una nueva
            var carpetas = _driveService.ListarCarpetas();
            return View(carpetas); // Pasa la lista de carpetas a la vista

        }

        // Método para subir un archivo dentro de una carpeta específica
        [HttpPost]
        public IActionResult SubirArchivoACarpeta(IFormFile archivo, string carpetaId)
        {
            if (archivo != null && archivo.Length > 0)
            {
                using (var stream = archivo.OpenReadStream())
                {
                    try
                    {
                        // Sube el archivo a la carpeta especificada
                        var idArchivo = _driveService.SubirArchivoEnCarpeta(stream, archivo.FileName, carpetaId);
                        ViewBag.Mensaje = $"Archivo subido correctamente a la carpeta. ID del archivo: {idArchivo}";
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Mensaje = $"Error al subir el archivo: {ex.Message}";
                    }
                }
            }
            else
            {
                ViewBag.Mensaje = "Por favor, selecciona un archivo válido.";
            }

            // Recupera nuevamente las carpetas para renderizarlas en la vista
            var carpetas = _driveService.ListarCarpetas();
            return View("CrearCarpeta", carpetas); // Redirige a la vista de CrearCarpeta
        }

        // Método para eliminar una carpeta de Google Drive
        [HttpPost]
        public IActionResult EliminarCarpeta(string carpetaId)
        {

            try
            {
                // Intentar eliminar la carpeta y verificar si se realizó correctamente
                bool exito = _driveService.EliminarCarpeta(carpetaId);
                if (exito)
                {
                    ViewBag.Mensaje = "Carpeta eliminada correctamente.";
                }
                else
                {
                    ViewBag.Mensaje = "No se pudo eliminar la carpeta.";
                }
            }
            catch (Exception ex)
            {
                // Capturar errores y mostrar mensaje
                TempData["Mensaje"] = $"Error al eliminar la carpeta: {ex.Message}";
            }

            // Recupera nuevamente las carpetas para renderizarlas en la vista
            var carpetas = _driveService.ListarCarpetas();

            return View("CrearCarpeta", carpetas);// Redirige a la vista de CrearCarpeta

        }

        // Método para mostrar los archivos dentro de una carpeta
        [HttpGet]
        public IActionResult VerCarpeta(string carpetaId)
        {
            try
            {
                //Verifica si se encuentra  la carpetaa
                if (string.IsNullOrEmpty(carpetaId))
                {
                    TempData["MensajeError"] = "ID de carpeta no proporcionado.";
                    return RedirectToAction("CrearCarpeta");
                }

                var archivos = _driveService.ListarArchivosEnCarpeta(carpetaId);

                ViewBag.CarpetaId = carpetaId; // Guardar el ID para la vista
                return View(archivos);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = $"Error al obtener archivos de la carpeta: {ex.Message}";
                return RedirectToAction("CrearCarpeta");
            }
        }

        // Método para listar todos los archivos de Google Drive
        [HttpGet]
        public async Task<IActionResult> ListarArchivos()
        {
            try
            {
                // Obtener todos los archivos desde el servicio de Google Drive
                var archivos = _driveService.ListarArchivos();

                // Ordenar los archivos por fecha de creación (más recientes primero)
                var archivosOrdenados = archivos.OrderByDescending(a => a.FechaCreacion).ToList();

                // Retornar la vista con la lista de archivos ordenados
                return View(archivosOrdenados);
            }
            catch (Exception ex)
            {
                // En caso de error, almacenar mensaje para mostrar al usuario
                TempData["MensajeError"] = $"Error al listar archivos: {ex.Message}";

                // Retornar vista con lista vacía para evitar errores
                return View(new List<GoogleDrive>());
            }
        }

        // Método para eliminar un archivo de Google Drive
        [HttpPost]
        public IActionResult EliminarArchivo(string fileId)
        {
            //Evalúa si la acción de eliminar el archivo se realizó
            try
            {
                bool resultado = _driveService.EliminarArchivo(fileId);
                if (resultado)
                {
                    TempData["Mensaje"] = "Archivo eliminado correctamente.";
                }
                else
                {
                    TempData["MensajeError"] = "No se pudo eliminar el archivo.";
                }
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = $"Error al eliminar el archivo: {ex.Message}";
            }

            //Redirecciona al listado para verificar la eliminación
            return RedirectToAction("ListarArchivos");
        }

        // Método para descargar un archivo desde Google Drive
        [HttpGet]
        public IActionResult DescargarArchivo(string fileId)
        {
            try
            {
                var (stream, fileName) = _driveService.DescargarArchivo(fileId);

                // Obtener el tipo MIME basado en la extensión del archivo
                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(fileName, out var contentType))
                {
                    contentType = "application/octet-stream";
                }

                return File(stream, contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = $"Error al descargar el archivo: {ex.Message}";
                return RedirectToAction("ListarArchivos");
            }

        }

    public IActionResult Index()
        {
            return View();
        }
    }
}
