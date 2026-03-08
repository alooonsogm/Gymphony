using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gymphony.Models
{
    [Table("Sesiones")]
    public class Sesion
    {
        [Key]
        [Column("Id")]
        public int IdSesion { get; set; }
        [Column("ClaseId")]
        public int ClaseId { get; set; }
        [Column("EntrenadorId")]
        public int EntrenadorId { get; set; }
        [Column("SalaId")]
        public int SalaId { get; set; }
        [Column("Fecha")]
        public DateOnly Fecha { get; set; }
        [Column("HoraInicio")]
        public TimeOnly HoraInicio { get; set; }
        [Column("HoraFin")]
        public TimeOnly HoraFin { get; set; }
    }
}
