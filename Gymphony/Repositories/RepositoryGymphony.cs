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

        public async Task<List<DatosSesion>> GetSesionesNuevasAsync()
        {
            DateOnly hoy = DateOnly.FromDateTime(DateTime.Now);
            TimeOnly ahora = TimeOnly.FromDateTime(DateTime.Now);

            var consulta = from datos in this.context.DatosSesion
                           where datos.Fecha > hoy || (datos.Fecha == hoy && datos.HoraInicio > ahora)
                           orderby datos.Fecha ascending, datos.HoraInicio ascending
                           select datos;

            return await consulta.ToListAsync();
        }

        private async Task<int> GetMaxIdReservaSesionesAsync()
        {
            if (this.context.ReservaSesiones.Count() == 0)
            {
                return 1;
            }
            else
            {
                return await this.context.ReservaSesiones.MaxAsync(z => z.IdReservaSesion) + 1;
            }
        }

        public async Task<string> ReservarPlazaAsync(int idSesion, int idCliente)
        {
            var consulta = from datos in this.context.DatosSesion where datos.IdSesion == idSesion select datos;
            DatosSesion sesion = await consulta.FirstOrDefaultAsync();
            if (sesion == null)
            {
                return "Error: La sesión no existe.";
            }

            DateOnly hoy = DateOnly.FromDateTime(DateTime.Now);
            TimeOnly ahora = TimeOnly.FromDateTime(DateTime.Now);
            if (sesion.Fecha < hoy || (sesion.Fecha == hoy && sesion.HoraInicio <= ahora))
            {
                return "No puedes reservar una sesión que ya ha comenzado o ha finalizado.";
            }

            int reservasActuales = await this.context.ReservaSesiones.CountAsync(r => r.SesionId == idSesion);
            if (reservasActuales >= sesion.CapacidadMaxima)
            {
                return "Lo sentimos, la sesión está completa y no quedan plazas.";
            }

            int idReservaSesiones = await GetMaxIdReservaSesionesAsync();
            ReservaSesiones nuevaReserva = new ReservaSesiones
            {
                IdReservaSesion = idReservaSesiones,
                SesionId = idSesion,
                ClienteId = idCliente,
                FechaHoraReserva = DateTime.Now
            };
            await this.context.ReservaSesiones.AddAsync(nuevaReserva);
            await this.context.SaveChangesAsync();
            return "OK";
        }

        public async Task<DatosSesion> FindDatosSesionAsync(int idSesion)
        {
            var consulta = from datos in this.context.DatosSesion where datos.IdSesion == idSesion select datos;
            return await consulta.FirstOrDefaultAsync();
        }

        public async Task<List<int>> GetSesionesReservadasClienteAsync(int idCliente)
        {
            var consulta = from datos in this.context.ReservaSesiones
                           where datos.ClienteId == idCliente
                           select datos.SesionId;

            return await consulta.ToListAsync();
        }

        public async Task<string> AnularReservaAsync(int idSesion, int idCliente)
        {
            ReservaSesiones reserva = await this.context.ReservaSesiones.FirstOrDefaultAsync(r => r.SesionId == idSesion && r.ClienteId == idCliente);

            if (reserva == null)
            {
                return "Error: No tienes una reserva para esta sesión.";
            }

            this.context.ReservaSesiones.Remove(reserva);
            await this.context.SaveChangesAsync();

            return "OK_ANULADA";
        }

        public async Task<List<DatosSesion>> GetMisSesionesCompletasAsync(int idCliente)
        {
            List<int> idsReservas = await this.GetSesionesReservadasClienteAsync(idCliente);

            if (idsReservas.Count == 0)
            {
                return new List<DatosSesion>();
            }

            var consulta = from datos in this.context.DatosSesion
                           where idsReservas.Contains(datos.IdSesion)
                           orderby datos.Fecha ascending, datos.HoraInicio ascending
                           select datos;

            return await consulta.ToListAsync();
        }

        public async Task<List<HorarioEmpleados>> GetHorarioUsuarioPorIdAsync(int idUsuario)
        {
            var consulta = from datos in this.context.HorarioEmpleados
                           where datos.UsuarioId == idUsuario
                           orderby datos.DiaSemana ascending
                           select datos;

            return await consulta.ToListAsync();
        }
    }
}
