using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gymphony.Models
{
    [Table("V_DatosSesiones")]
    public class DatosSesion
    {
        [Key]
        [Column("Id")]
        public int IdSesion { get; set; }
        [Column("NombreClase")]
        public string NombreClase { get; set; }
        [Column("Descripcion")]
        public string Descripcion { get; set; }
        [Column("NombreEntrenador")]
        public string NombreEntrenador { get; set; }
        [Column("ApellidoEntrenador")]
        public string ApellidoEntrenador { get; set; }
        [Column("NombreSala")]
        public string NombreSala { get; set; }
        [Column("CapacidadMaxima")]
        public int CapacidadMaxima { get; set; }
        [Column("Fecha")]
        public DateOnly Fecha { get; set; }
        [Column("HoraInicio")]
        public TimeOnly HoraInicio { get; set; }
        [Column("HoraFin")]
        public TimeOnly HoraFin { get; set; }
    }
}
