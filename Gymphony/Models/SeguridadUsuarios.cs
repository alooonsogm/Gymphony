using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gymphony.Models
{
    [Table("SeguridadUsuarios")]
    public class SeguridadUsuarios
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }
        [Column("UsuarioId")]
        public int UsuarioId { get; set; }
        [Column("PasswordHash")]
        public byte[] PasswordHash { get; set; }
        [Column("Salt")]
        public string Salt { get; set; }
    }
}
