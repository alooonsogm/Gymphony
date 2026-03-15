using Gymphony.Models;
using Microsoft.EntityFrameworkCore;

namespace Gymphony.Data
{
    public class GymphonyContext: DbContext
    {
        public GymphonyContext(DbContextOptions<GymphonyContext> options) : base(options)
        {

        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<SeguridadUsuarios> SeguridadUsuarios { get; set; }
        public DbSet<ValidacionUsuario> ValidacionUsuario { get; set; }
        public DbSet<Rol> Rol { get; set; }
        public DbSet<DatosSesion> DatosSesion { get; set; }
        public DbSet<ReservaSesiones> ReservaSesiones { get; set; }
        public DbSet<HorarioEmpleados> HorarioEmpleados { get; set; }
        public DbSet<Clases> Clases { get; set; }
        public DbSet<Salas> Salas { get; set; }
        public DbSet<Sesion> Sesion { get; set; }
        public DbSet<Afiliaciones> Afiliaciones { get; set; }
        public DbSet<VistaUsuario> VistaUsuario { get; set; }
    }
}
