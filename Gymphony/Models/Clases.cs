using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gymphony.Models
{
    [Table("Clases")]
    public class Clases
    {
        [Key]
        [Column("Id")]
        public int IdClases { get; set; }
        [Column("Nombre")]
        public string Nombre { get; set; }
        [Column("Description")]
        public string Descripcion { get; set; }
    }
}
