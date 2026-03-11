using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gymphony.Models
{
    [Table("Afiliaciones")]
    public class Afiliaciones
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }
        [Column("ClienteId")]
        public int ClienteId { get; set; }
        [Column("FechaAlta")]
        public DateOnly FechaAlta { get; set; }
        [Column("FechaBaja")]
        public DateOnly? FechaBaja { get; set; }
    }
}
