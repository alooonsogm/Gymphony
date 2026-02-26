using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gymphony.Models
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        [Column("Id")]
        public int IdUsuario { get; set; }
        [Column("RoleId")]
        public int RoleId { get; set; }
        [Column("Email")]
        public string Email { get; set; }
        [Column("Password")]
        public string Password { get; set; }
        [Column("Nombre")]
        public string Nombre { get; set; }
        [Column("Apellidos")]
        public string Apellidos { get; set; }
        [Column("Telefono")]
        public string Telefono { get; set; }
        [Column("FechaNacimiento")]
        public DateOnly FechaNacimiento { get; set; }
        [Column("DNI")]
        public string Dni { get; set; }
        [Column("RutaFoto")]
        public string? RutaFoto { get; set; }
    }
}
