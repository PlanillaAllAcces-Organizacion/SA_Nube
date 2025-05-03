using System.ComponentModel;

namespace GRUPO_01_SubirArchivosNube.AppWebMVC.Models
{
    public class GoogleDrive
    {
        // Identificador único del archivo en Google Drive
        public string Id { get; set; }

        // Nombre del archivo almacenado en Drive
        public string Nombre { get; set; }

        // Tipo de archivo (ejemplo: imagen, documento, PDF, etc.)
        // Se usa DisplayName para mejorar la presentación en interfaces gráficas
        [DisplayName("Tipo de Archivo")]
        public string Tipo { get; set; }

        // Tamaño del archivo en bytes (puede ser nulo si la API no proporciona la información)
        public long? Tamano { get; set; }

        // Fecha y hora en la que el archivo fue creado en Google Drive
        // Se usa DisplayName para personalizar la etiqueta en interfaces gráficas
        [DisplayName("Fecha de Creación")]
        public DateTime? FechaCreacion { get; set; }

        // Última fecha y hora de modificación del archivo
        [DisplayName("Fecha de Modificación")]
        public DateTime? FechaModificacion { get; set; }

        // Enlace público para acceder al archivo desde Google Drive
        public string Url { get; set; }
    }
}
