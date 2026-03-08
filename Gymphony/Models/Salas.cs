using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gymphony.Models
{
    [Table("salas")]
    public class Salas
    {
        [Key]
        [Column("Id")]
        public int IdSalas { get; set; }
        [Column("Nombre")]
        public string Nombre { get; set; }
        [Column("CapacidadMaxima")]
        public int CapacidadMaxima { get; set; }
    }
}
