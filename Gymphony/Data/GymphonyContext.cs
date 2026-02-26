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
    }
}
