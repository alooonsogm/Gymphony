using Gymphony.Extensions;
using Gymphony.Helpers;
using Gymphony.Models;
using Gymphony.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Gymphony.Controllers
{
    public class AdminUsuariosController : Controller
    {
        private RepositoryGymphony repo;
        private HelperPath helper;

        public AdminUsuariosController(RepositoryGymphony repo, HelperPath helper)
        {
            this.repo = repo;
            this.helper = helper;
        }

        public async Task<IActionResult> PanelUsuarios(string seccion)
        {
            int idUser = HttpContext.Session.GetObject<int>("IDUSUARIO");
            int idRol = HttpContext.Session.GetObject<int>("IDROLUSUARIO");

            if (idUser == 0)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                if (idRol != 1)
                {
                    TempData["MENSAJERESERVA"] = "Acceso denegado: Solo el administrador puede acceder.";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    if (seccion == null)
                    {
                        seccion = "socios";
                    }
                    ViewData["SECCIONACTIVA"] = seccion;

                    AdminGestionUsuarios model = new AdminGestionUsuarios();
                    model.Socios = await this.repo.GetSociosConEstadoAsync();
                    model.Entrenadores = await this.repo.GetUsuariosPorRolAsync(3);
                    return View(model);
                }
            }
        }

        public IActionResult CrearSocio()
        {
            int idUser = HttpContext.Session.GetObject<int>("IDUSUARIO");
            int idRol = HttpContext.Session.GetObject<int>("IDROLUSUARIO");

            if (idUser == 0)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                if (idRol != 1)
                {
                    TempData["MENSAJERESERVA"] = "Acceso denegado: Solo el administrador puede acceder.";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return View();
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearSocio(string email, string password, string nombre, string apellidos, string telefono, DateOnly fechaNacimiento, string dni, IFormFile foto)
        {
            int idUser = HttpContext.Session.GetObject<int>("IDUSUARIO");
            int idRol = HttpContext.Session.GetObject<int>("IDROLUSUARIO");

            if (idUser == 0)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                if (idRol != 1)
                {
                    TempData["MENSAJERESERVA"] = "Acceso denegado: Solo el administrador puede acceder.";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    string fileName = foto.FileName;
                    string path = this.helper.MapPath(fileName, Folders.Usuarios);
                    using (Stream stream = new FileStream(path, FileMode.Create))
                    {
                        await foto.CopyToAsync(stream);
                    }
                    await this.repo.RegistroSocioAsync(email, password, nombre, apellidos, telefono, fechaNacimiento, dni, fileName);
                    TempData["MENSAJE_EXITO"] = $"El socio {nombre} {apellidos} ha sido dado de alta correctamente.";
                    return RedirectToAction("PanelUsuarios", new { seccion = "socios" });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> EliminarSocio(int id)
        {
            try
            {
                await this.repo.DeleteSocioAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar el usuario: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DarBajaSocio(int id)
        {
            try
            {
                await this.repo.DarDeBajaSocioAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al dar de baja: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DarDeAltaSocio(int id)
        {
            try
            {
                await this.repo.DarDeAltaSocioAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al reactivar socio: " + ex.Message });
            }
        }

        public IActionResult CrearEntrenador()
        {
            int idUser = HttpContext.Session.GetObject<int>("IDUSUARIO");
            int idRol = HttpContext.Session.GetObject<int>("IDROLUSUARIO");

            if (idUser == 0)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                if (idRol != 1)
                {
                    TempData["MENSAJERESERVA"] = "Acceso denegado: Solo el administrador puede acceder.";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return View();
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearEntrenador(string email, string password, string nombre, string apellidos, string telefono, DateOnly fechaNacimiento, string dni, IFormFile foto, List<int> diasSemana, List<TimeOnly> horasInicio, List<TimeOnly> horasFin)
        {
            int idUser = HttpContext.Session.GetObject<int>("IDUSUARIO");
            int idRol = HttpContext.Session.GetObject<int>("IDROLUSUARIO");

            if (idUser == 0)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                if (idRol != 1)
                {
                    TempData["MENSAJERESERVA"] = "Acceso denegado: Solo el administrador puede acceder.";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    string fileName = foto.FileName;
                    string path = this.helper.MapPath(fileName, Folders.Usuarios);
                    using (Stream stream = new FileStream(path, FileMode.Create))
                    {
                        await foto.CopyToAsync(stream);
                    }
                    await this.repo.RegistroEntrenadorAsync(email, password, nombre, apellidos, telefono, fechaNacimiento, dni, fileName, diasSemana, horasInicio, horasFin);
                    TempData["MENSAJE_EXITO"] = $"El entrenador {nombre} {apellidos} ha sido configurado correctamente.";
                    return RedirectToAction("PanelUsuarios", new { seccion = "entrenadores" });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerHorariosEntrenador(int id)
        {
            try
            {
                var horarios = await this.repo.GetHorariosEntrenadorAsync(id);
                var datosFormateados = horarios.Select(h => new {
                    diaSemana = h.DiaSemana,
                    horaInicio = h.HoraInicio.ToString("HH:mm"),
                    horaFin = h.HoraFin.ToString("HH:mm")
                });

                return Json(new { success = true, data = datosFormateados });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al obtener horarios: " + ex.Message });
            }
        }

        public async Task<IActionResult> ValidarBorradoEntrenador(int id)
        {
            try
            {
                bool tieneSesiones = await this.repo.EntrenadorTieneSesionesAsync(id);
                var sustitutos = await this.repo.GetEntrenadoresSustitutosAsync(id);

                var listaSustitutos = sustitutos.Select(s => new {
                    id = s.IdUsuario,
                    nombre = s.Nombre + " " + s.Apellidos
                }).ToList();

                return Json(new { success = true, hasSessions = tieneSesiones, sustitutos = listaSustitutos });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al validar: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EliminarEntrenador(int idBorrar, int? idSustituto)
        {
            try
            {
                await this.repo.DeleteEntrenadorSustituyendoAsync(idBorrar, idSustituto);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al eliminar: " + ex.Message });
            }
        }
    }
}
