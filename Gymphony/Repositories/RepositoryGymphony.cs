using Gymphony.Data;
using Gymphony.Helpers;
using Gymphony.Models;
using Microsoft.EntityFrameworkCore;

namespace Gymphony.Repositories
{
    public class RepositoryGymphony
    {
        private GymphonyContext context;

        public RepositoryGymphony(GymphonyContext context)
        {
            this.context = context;
        }

        private async Task<int> GetMaxIdUsuariosAsync()
        {
            if (this.context.Usuarios.Count() == 0)
            {
                return 1;
            }
            else
            {
                return await this.context.Usuarios.MaxAsync(z => z.IdUsuario) + 1;
            }
        }

        public async Task GeneratePasswordHashAsync()
        {
            var consulta = from datos in this.context.Usuarios select datos;
            List<Usuario> usuarios = await consulta.ToListAsync();
            int idHash = 1;

            foreach (Usuario user in usuarios)
            {
                int usuarioId = user.IdUsuario;
                string password = user.Password;
                string salt = HelperTools.GenerateSalt();

                SeguridadUsuarios seguridad = new SeguridadUsuarios();
                seguridad.Id = idHash;
                seguridad.UsuarioId = usuarioId;
                seguridad.PasswordHash = HelperCryptography.EncryptPassword(password, salt);
                seguridad.Salt = salt;
                await this.context.SeguridadUsuarios.AddAsync(seguridad);
                await this.context.SaveChangesAsync();
                idHash++;
            }
        }

        public async Task<ValidacionUsuario> LogInUserAsync(string email, string password)
        {
            var consulta = from datos in this.context.ValidacionUsuario where datos.Email == email select datos;
            ValidacionUsuario user = await consulta.FirstOrDefaultAsync();
            if (user == null)
            {
                return null;
            }
            else
            {
                string salt = user.Salt;
                byte[] temp = HelperCryptography.EncryptPassword(password, salt);
                byte[] passByte = user.PasswordHash;
                bool response = HelperTools.CompareArrays(temp, passByte);
                if (response == true)
                {
                    return user;
                }
                else
                {
                    return null;
                }
            }
        }

        public async Task<Usuario> FindUsuarioAsync(int idUsuario)
        {
            var consulta = from datos in this.context.Usuarios where datos.IdUsuario == idUsuario select datos;
            return await consulta.FirstOrDefaultAsync();
        }

        public async Task<Rol> FindRolPorIdRolAsync(int idRol)
        {
            var consulta = from datos in this.context.Rol where datos.IdRol == idRol select datos;
            return await consulta.FirstOrDefaultAsync();
        }

        public async Task<List<DatosSesion>> GetSesionesAsync()
        {
            var consulta = from datos in this.context.DatosSesion select datos;
            return await consulta.ToListAsync();
        }
    }
}
