using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gymphony.Models
{
    [Table("HorarioEmpleados")]
    public class HorarioEmpleados
    {
        [Key]
        [Column("Id")]
        public int IdHorarioEmpleados { get; set; }
        [Column("UsuarioId")]
        public int UsuarioId { get; set; }
        [Column("DiaSemana")]
        public int DiaSemana { get; set; }
        [Column("HoraInicio")]
        public TimeOnly HoraInicio { get; set; }
        [Column("HoraFin")]
        public TimeOnly HoraFin { get; set; }
    }
}
