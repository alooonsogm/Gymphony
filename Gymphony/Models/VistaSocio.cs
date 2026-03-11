namespace Gymphony.Models
{
    public class VistaSocio
    {
        public int IdSocio { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public DateOnly FechaNacimiento { get; set; }
        public string DNI { get; set; }
        public bool esActivo { get; set; }
    }
}
