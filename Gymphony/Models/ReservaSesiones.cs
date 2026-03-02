using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gymphony.Models
{
    [Table("ReservaSesiones")]
    public class ReservaSesiones
    {
        [Key]
        [Column("Id")]
        public int IdReservaSesion { get; set; }
        [Column("SesionId")]
        public int SesionId { get; set; }
        [Column("ClienteId")]
        public int ClienteId { get; set; }
        [Column("HoraReserva")]
        public DateTime FechaHoraReserva { get; set; }
    }
}
