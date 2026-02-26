using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gymphony.Models
{
    [Table("Roles")]
    public class Rol
    {
        [Key]
        [Column("Id")]
        public int IdRol { get; set; }
        [Column("Nombre")]
        public string NombreRol { get; set; }
    }
}
