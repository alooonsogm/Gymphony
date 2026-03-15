using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gymphony.Models
{
    [Table("RegistroAforo")]
    public class RegistroAforo
    {
        [Key]
        [Column("Id")]
        public int IdRegistroAforo { get; set; }

        [Column("ClienteId")]
        public int ClienteId { get; set; }

        [Column("HoraEntrada")]
        public DateTime HoraEntrada { get; set; }

        [Column("HoraSalida")]
        public DateTime? HoraSalida { get; set; }
    }
}
