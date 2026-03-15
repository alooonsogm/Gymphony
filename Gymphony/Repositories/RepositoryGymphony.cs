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
            var consulta = from datos in this.context.DatosSesion orderby datos.IdSesion descending select datos;
            return await consulta.ToListAsync();
        }

        public async Task<List<Clases>> GetTodasClasesAsync()
        {
            var consulta = from datos in this.context.Clases orderby datos.IdClases descending select datos;
            return await consulta.ToListAsync();
        }

        public async Task<List<Salas>> GetTodasSalasAsync()
        {
            var consulta = from datos in this.context.Salas orderby datos.IdSalas descending select datos;
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
            if (fecha < DateOnly.FromDateTime(DateTime.Today))
            {
                return "La fecha debe ser posterior o el día de hoy.";
            }

            if (fecha == DateOnly.FromDateTime(DateTime.Today) && horaInicio <= TimeOnly.FromDateTime(DateTime.Now))
            {
                return "Para las sesiones de hoy, la hora de inicio debe ser posterior a la hora actual.";
            }

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

        public async Task<List<VistaUsuario>> GetUsuariosPorRolAsync(string rol)
        {
            var consulta = from datos in this.context.VistaUsuario where datos.NombreRol == rol select datos;
            return await consulta.ToListAsync();
        }

        public async Task<List<VistaSocio>> GetSociosConEstadoAsync()
        {
            var usuariosSocio = await this.context.VistaUsuario.Where(u => u.NombreRol == "Socio").ToListAsync();
            var listaSocios = new List<VistaSocio>();

            foreach (var user in usuariosSocio)
            {
                var ultimaAfiliacion = await this.context.Afiliaciones.Where(a => a.ClienteId == user.IdUsuario).OrderByDescending(a => a.Id).FirstOrDefaultAsync();

                bool activo = false;
                if (ultimaAfiliacion != null && ultimaAfiliacion.FechaBaja == null)
                {
                    activo = true;
                }

                listaSocios.Add(new VistaSocio
                {
                    IdSocio = user.IdUsuario,
                    Nombre = user.Nombre,
                    Apellidos = user.Apellidos,
                    Email = user.Email,
                    Telefono = user.Telefono,
                    FechaNacimiento = user.FechaNacimiento,
                    DNI = user.Dni,
                    esActivo = activo
                });
            }
            return listaSocios;
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

        private async Task<int> GetMaxIdAfiliacionAsync()
        {
            if (this.context.Afiliaciones.Count() == 0)
            {
                return 1;
            }
            else
            {
                return await this.context.Afiliaciones.MaxAsync(z => z.Id) + 1;
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

            Afiliaciones nuevaAfiliacion = new Afiliaciones();
            nuevaAfiliacion.Id = await this.GetMaxIdAfiliacionAsync();
            nuevaAfiliacion.ClienteId = idUser;
            nuevaAfiliacion.FechaAlta = DateOnly.FromDateTime(DateTime.Now);
            nuevaAfiliacion.FechaBaja = null;
            await this.context.Afiliaciones.AddAsync(nuevaAfiliacion);
            await this.context.SaveChangesAsync();
        }

        public async Task DeleteSocioAsync(int idSocio)
        {
            Usuario user = await FindUsuarioAsync(idSocio);
            if (user != null)
            {
                this.context.Usuarios.Remove(user);
                await this.context.SaveChangesAsync();
            }
        }

        public async Task DarDeBajaSocioAsync(int idSocio)
        {
            Afiliaciones afiliacionActiva = await this.context.Afiliaciones.Where(a => a.ClienteId == idSocio && a.FechaBaja == null).OrderByDescending(a => a.FechaAlta).FirstOrDefaultAsync();

            if (afiliacionActiva != null)
            {
                afiliacionActiva.FechaBaja = DateOnly.FromDateTime(DateTime.Now);
                await this.context.SaveChangesAsync();
            }
        }

        public async Task DarDeAltaSocioAsync(int idSocio)
        {
            Afiliaciones nuevaAfiliacion = new Afiliaciones();
            nuevaAfiliacion.Id = await this.GetMaxIdAfiliacionAsync();
            nuevaAfiliacion.ClienteId = idSocio;
            nuevaAfiliacion.FechaAlta = DateOnly.FromDateTime(DateTime.Now);
            nuevaAfiliacion.FechaBaja = null;

            await this.context.Afiliaciones.AddAsync(nuevaAfiliacion);
            await this.context.SaveChangesAsync();
        }

        public async Task<bool> IsSocioActivoAsync(int idUsuario)
        {
            var ultimaAfiliacion = await this.context.Afiliaciones.Where(a => a.ClienteId == idUsuario).OrderByDescending(a => a.Id).FirstOrDefaultAsync();
            if (ultimaAfiliacion == null || ultimaAfiliacion.FechaBaja != null)
            {
                return false;
            }
            return true;
        }

        private async Task<int> GetMaxIdHorarioEmpleadoAsync()
        {
            if (this.context.HorarioEmpleados.Count() == 0)
            {
                return 1;
            }
            else
            {
                return await this.context.HorarioEmpleados.MaxAsync(z => z.IdHorarioEmpleados) + 1;
            }
        }

        public async Task RegistroEntrenadorAsync(string email, string password, string nombre, string apellidos, string telefono, DateOnly fechaNacimiento, string dni, string rutaFoto, List<int> diasSemana, List<TimeOnly> horasInicio, List<TimeOnly> horasFin)
        {
            Usuario user = new Usuario();
            int idUser = await this.GetMaxIdUsuarioAsync();
            user.IdUsuario = idUser;
            user.RoleId = 3;
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

            if (diasSemana != null && diasSemana.Count > 0)
            {
                for (int i = 0; i < diasSemana.Count; i++)
                {
                    HorarioEmpleados nuevoHorario = new HorarioEmpleados();
                    nuevoHorario.IdHorarioEmpleados = await this.GetMaxIdHorarioEmpleadoAsync();
                    nuevoHorario.UsuarioId = idUser;
                    nuevoHorario.DiaSemana = diasSemana[i];
                    nuevoHorario.HoraInicio = horasInicio[i];
                    nuevoHorario.HoraFin = horasFin[i];
                    await this.context.HorarioEmpleados.AddAsync(nuevoHorario);
                    await this.context.SaveChangesAsync();
                }
            }
        }

        public async Task<List<HorarioEmpleados>> GetHorariosEntrenadorAsync(int idEntrenador)
        {
            return await this.context.HorarioEmpleados.Where(h => h.UsuarioId == idEntrenador).OrderBy(h => h.DiaSemana).ThenBy(h => h.HoraInicio).ToListAsync();
        }

        public async Task<bool> EntrenadorTieneSesionesAsync(int idEntrenador)
        {
            return await this.context.Sesion.AnyAsync(s => s.EntrenadorId == idEntrenador);
        }

        public async Task<List<Usuario>> GetEntrenadoresSustitutosAsync(int idEntrenadorExcluir)
        {
            return await this.context.Usuarios.Where(u => u.RoleId == 3 && u.IdUsuario != idEntrenadorExcluir).ToListAsync();
        }

        public async Task DeleteEntrenadorSustituyendoAsync(int idEntrenadorABorrar, int? idEntrenadorSustituto)
        {
            if (idEntrenadorSustituto != null && idEntrenadorSustituto > 0)
            {
                var sesiones = await this.context.Sesion.Where(s => s.EntrenadorId == idEntrenadorABorrar).ToListAsync();

                foreach (var sesion in sesiones)
                {
                    sesion.EntrenadorId = idEntrenadorSustituto.Value;
                }

                await this.context.SaveChangesAsync();
            }

            Usuario user = await FindUsuarioAsync(idEntrenadorABorrar);
            if (user != null)
            {
                this.context.Usuarios.Remove(user);
                await this.context.SaveChangesAsync();
            }
        }

        public async Task<VistaUsuario> FindVistaUsuarioAsync(int idUsuario)
        {
            var consulta = from datos in this.context.VistaUsuario where datos.IdUsuario == idUsuario select datos;
            return await consulta.FirstOrDefaultAsync();
        }
    }
}
