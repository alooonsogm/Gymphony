using Gymphony.Data;
using Gymphony.Helpers;
using Gymphony.Models;
using Microsoft.AspNetCore.Http;
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

        public async Task<List<Usuario>> GetUsuariosPorSesionAsync(int idSesion)
        {
            var consulta = from reserva in this.context.ReservaSesiones
                           join user in this.context.Usuarios
                           on reserva.ClienteId equals user.IdUsuario
                           where reserva.SesionId == idSesion
                           select user;

            return await consulta.ToListAsync();
        }

        public async Task<List<DatosSesion>> GetTodasSesionesAsync()
        {
            var consulta = from datos in this.context.DatosSesion select datos;
            return await consulta.ToListAsync();
        }

        public async Task<List<Clases>> GetTodasClasesAsync()
        {
            var consulta = from datos in this.context.Clases select datos;
            return await consulta.ToListAsync();
        }

        public async Task<List<Salas>> GetTodasSalasAsync()
        {
            var consulta = from datos in this.context.Salas select datos;
            return await consulta.ToListAsync();
        }

        public async Task<List<Usuario>> GetTodosEntrenadoresAsync()
        {
            var consulta = from datos in this.context.Usuarios where datos.RoleId == 3 select datos;
            return await consulta.ToListAsync();
        }

        private async Task<int> GetMaxIdSalaAsync()
        {
            if (this.context.Salas.Count() == 0)
            {
                return 1;
            }
            else
            {
                return await this.context.Salas.MaxAsync(z => z.IdSalas) + 1;
            }
        }

        public async Task CreateSalaAsync(string nombre, int capacidadMax)
        {
            Salas sala = new Salas();
            sala.IdSalas = await GetMaxIdSalaAsync();
            sala.Nombre = nombre;
            sala.CapacidadMaxima = capacidadMax;
            await this.context.Salas.AddAsync(sala);
            await this.context.SaveChangesAsync();
        }

        public async Task<Salas> FindSalasAsync(int idSala)
        {
            var consulta = from datos in this.context.Salas where datos.IdSalas == idSala select datos;
            return await consulta.FirstOrDefaultAsync();
        }

        public async Task DeleteSalasAsync(int id)
        {
            Salas sala = await this.FindSalasAsync(id);
            if (sala != null)
            {
                this.context.Salas.Remove(sala);
                await this.context.SaveChangesAsync();
            }
        }

        public async Task UpdateSalaAsync(int idSalas, string nombre, int capacidadMax)
        {
            Salas sala = await this.FindSalasAsync(idSalas);
            if (sala != null)
            {
                sala.Nombre = nombre;
                sala.CapacidadMaxima = capacidadMax;
                await this.context.SaveChangesAsync();
            }
        }

        private async Task<int> GetMaxIdClasesAsync()
        {
            if (this.context.Clases.Count() == 0)
            {
                return 1;
            }
            else
            {
                return await this.context.Clases.MaxAsync(z => z.IdClases) + 1;
            }
        }

        public async Task CreateClasesAsync(string nombre, string descripcion)
        {
            Clases clase = new Clases();
            clase.IdClases = await GetMaxIdClasesAsync();
            clase.Nombre = nombre;
            clase.Descripcion = descripcion;
            await this.context.Clases.AddAsync(clase);
            await this.context.SaveChangesAsync();
        }

        public async Task<Clases> FindClasesAsync(int idClase)
        {
            var consulta = from datos in this.context.Clases where datos.IdClases == idClase select datos;
            return await consulta.FirstOrDefaultAsync();
        }

        public async Task DeleteClasesAsync(int id)
        {
            Clases clase = await this.FindClasesAsync(id);
            if (clase != null)
            {
                this.context.Clases.Remove(clase);
                await this.context.SaveChangesAsync();
            }
        }

        public async Task UpdateClasesAsync(int idClase, string nombre, string descripcion)
        {
            Clases clase = await this.FindClasesAsync(idClase);
            if (clase != null)
            {
                clase.Nombre = nombre;
                clase.Descripcion = descripcion;
                await this.context.SaveChangesAsync();
            }
        }

        public async Task<string> ValidarSesionAsync(DateOnly fecha, TimeOnly horaInicio, TimeOnly horaFin, int idEntrenador, int idSala, int? idSesionActual = null)
        {
            if (horaInicio >= horaFin)
            {
                return "La hora de fin debe ser posterior a la hora de inicio.";
            }

            int diaForm = (int)fecha.DayOfWeek;
            int diaConsulta;

            if(diaForm == 0)
            {
                diaConsulta = 7;
            }else
            {
                diaConsulta = diaForm;
            }

            // Buscamos si el entrenador trabaja ese día
            var turnosDia = await this.context.HorarioEmpleados
                    .Where(h => h.UsuarioId == idEntrenador && h.DiaSemana == diaConsulta)
                    .ToListAsync();

            if (!turnosDia.Any())
            {
                return "El entrenador seleccionado no trabaja en este día de la semana.";
            }

            // Comprobamos si la sesión encaja dentro de ALGUNO de sus turnos de ese día
            bool cuadraEnTurno = turnosDia.Any(turno => horaInicio >= turno.HoraInicio && horaFin <= turno.HoraFin);

            if (!cuadraEnTurno)
            {
                var turnoMostrado = turnosDia.First();
                return $"La sesión está fuera del horario del entrenador. Su turno es de {turnoMostrado.HoraInicio.ToString("HH:mm")} a {turnoMostrado.HoraFin.ToString("HH:mm")}.";
            }

            // Comprobamos si el entrenador ya tiene OTRA sesión a esa misma hora
            bool solapeEntrenador = await this.context.Sesion
                .Where(s => s.EntrenadorId == idEntrenador && s.Fecha == fecha)
                .Where(s => idSesionActual == null || s.IdSesion != idSesionActual) // Ignoramos la sesión actual si es un Update
                .AnyAsync(s => horaInicio < s.HoraFin && horaFin > s.HoraInicio); 

            if (solapeEntrenador)
            {
                return "El entrenador ya imparte otra sesión que se cruza con este horario.";
            }

            // Comprobamos si la sala ya está ocupada a esa misma hora
            bool solapeSala = await this.context.Sesion
                .Where(s => s.SalaId == idSala && s.Fecha == fecha)
                .Where(s => idSesionActual == null || s.IdSesion != idSesionActual)
                .AnyAsync(s => horaInicio < s.HoraFin && horaFin > s.HoraInicio);

            if (solapeSala)
            {
                return "La sala seleccionada ya está ocupada en este horario por otra clase.";
            }

            return "OK";
        }

        private async Task<int> GetMaxIdSesionesAsync()
        {
            if (this.context.Sesion.Count() == 0)
            {
                return 1;
            }
            else
            {
                return await this.context.Sesion.MaxAsync(z => z.IdSesion) + 1;
            }
        }

        public async Task CreateSesionesAsync(int idClase, int idEntrenador, int idSala, DateOnly fecha, TimeOnly horaInicio, TimeOnly horaFin)
        {
            Sesion sesion = new Sesion();
            sesion.IdSesion = await GetMaxIdSesionesAsync();
            sesion.ClaseId = idClase;
            sesion.EntrenadorId = idEntrenador;
            sesion.SalaId = idSala;
            sesion.Fecha = fecha;
            sesion.HoraInicio = horaInicio;
            sesion.HoraFin = horaFin;
            await this.context.Sesion.AddAsync(sesion);
            await this.context.SaveChangesAsync();
        }

        public async Task<Sesion> FindSesionAsync(int idSesion)
        {
            var consulta = from datos in this.context.Sesion where datos.IdSesion == idSesion select datos;
            return await consulta.FirstOrDefaultAsync();
        }

        public async Task DeleteSesionAsync(int id)
        {
            Sesion sesion = await this.FindSesionAsync(id);
            if (sesion != null)
            {
                this.context.Sesion.Remove(sesion);
                await this.context.SaveChangesAsync();
            }
        }

        public async Task UpdateSesionAsync(int idSesion, int idClase, int idEntrenador, int idSala, DateOnly fecha, TimeOnly horaInicio, TimeOnly horaFin)
        {
            Sesion sesion = await this.FindSesionAsync(idSesion);
            if (sesion != null)
            {
                sesion.ClaseId = idClase;
                sesion.EntrenadorId = idEntrenador;
                sesion.SalaId = idSala;
                sesion.Fecha = fecha;
                sesion.HoraInicio = horaInicio;
                sesion.HoraFin = horaFin;
                await this.context.SaveChangesAsync();
            }
        }

        public async Task<List<Usuario>> GetUsuariosPorRolAsync(int idRol)
        {
            var consulta = from datos in this.context.Usuarios where datos.RoleId == idRol select datos;
            return await consulta.ToListAsync();
        }

        private async Task<int> GetMaxIdUsuarioAsync()
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
        private async Task<int> GetMaxIdUsuarioSeguridadAsync()
        {
            if (this.context.SeguridadUsuarios.Count() == 0)
            {
                return 1;
            }
            else
            {
                return await this.context.SeguridadUsuarios.MaxAsync(z => z.Id) + 1;
            }
        }

        public async Task RegistroSocioAsync(string email, string password, string nombre, string apellidos, string telefono, DateOnly fechaNacimiento, string dni, string rutaFoto)
        {
            Usuario user = new Usuario();
            int idUser = await this.GetMaxIdUsuarioAsync();
            user.IdUsuario = idUser;
            user.RoleId = 2;
            user.Email = email;
            user.Password = password;
            user.Nombre = nombre;
            user.Apellidos = apellidos;
            user.Telefono = telefono;
            user.FechaNacimiento = fechaNacimiento;
            user.Dni = dni;
            user.RutaFoto = rutaFoto;
            await this.context.Usuarios.AddAsync(user);
            await this.context.SaveChangesAsync();

            SeguridadUsuarios userSeguridad = new SeguridadUsuarios();
            userSeguridad.Id = await this.GetMaxIdUsuarioSeguridadAsync();
            userSeguridad.UsuarioId = idUser;
            userSeguridad.Salt = HelperTools.GenerateSalt();
            userSeguridad.PasswordHash = HelperCryptography.EncryptPassword(password, userSeguridad.Salt);
            await this.context.SeguridadUsuarios.AddAsync(userSeguridad);
            await this.context.SaveChangesAsync();
        }
    }
}
