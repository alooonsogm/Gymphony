using Gymphony.Extensions;
using Gymphony.Filters;
using Gymphony.Models;
using Gymphony.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Gymphony.Controllers
{
    [AuthorizeUsuarios]
    public class AdminClasesController : Controller
    {
        private RepositoryGymphony repo;

        public AdminClasesController(RepositoryGymphony repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> PanelControl(string seccion)
        {
            if (User.IsInRole("Administrador") == false)
            {
                TempData["MENSAJERESERVA"] = "Acceso denegado: Solo el administrador puede acceder.";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                if (seccion == null)
                {
                    seccion = "clases";
                }
                ViewData["SECCIONACTIVA"] = seccion;

                AdminGestionClases model = new AdminGestionClases();
                model.Clases = await this.repo.GetTodasClasesAsync();
                model.Salas = await this.repo.GetTodasSalasAsync();
                model.Sesiones = await this.repo.GetTodasSesionesAsync();
                model.Entrenadores = await this.repo.GetTodosEntrenadoresAsync();
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearSala([FromBody] Salas nuevaSala)
        {
            if (User.IsInRole("Administrador") == false)
            {
                return Json(new { success = false, message = "Acceso denegado: Acción no autorizada." });
            }

            try
            {
                await this.repo.CreateSalaAsync(nuevaSala.Nombre, nuevaSala.CapacidadMaxima);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al crear la sala: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EliminarSala(int id)
        {
            if (User.IsInRole("Administrador") == false)
            {
                return Json(new { success = false, message = "Acceso denegado: Acción no autorizada." });
            }

            try
            {
                await this.repo.DeleteSalasAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "No puedes borrar esta sala porque ya tiene sesiones programadas. Borra primero las sesiones."
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditarSala([FromBody] Salas salaEditada)
        {
            if (User.IsInRole("Administrador") == false)
            {
                return Json(new { success = false, message = "Acceso denegado: Acción no autorizada." });
            }

            try
            {
                await this.repo.UpdateSalaAsync(salaEditada.IdSalas, salaEditada.Nombre, salaEditada.CapacidadMaxima);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al actualizar la sala: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearClase([FromBody] Clases nuevaClase)
        {
            if (User.IsInRole("Administrador") == false)
            {
                return Json(new { success = false, message = "Acceso denegado: Acción no autorizada." });
            }

            try
            {
                await this.repo.CreateClasesAsync(nuevaClase.Nombre, nuevaClase.Descripcion);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al crear la clase: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EliminarClase(int id)
        {
            if (User.IsInRole("Administrador") == false)
            {
                return Json(new { success = false, message = "Acceso denegado: Acción no autorizada." });
            }

            try
            {
                await this.repo.DeleteClasesAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "No puedes borrar esta clase porque ya tiene sesiones programadas. Borra primero las sesiones." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditarClase([FromBody] Clases claseEditada)
        {
            if (User.IsInRole("Administrador") == false)
            {
                return Json(new { success = false, message = "Acceso denegado: Acción no autorizada." });
            }

            try
            {
                await this.repo.UpdateClasesAsync(claseEditada.IdClases, claseEditada.Nombre, claseEditada.Descripcion);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al actualizar la clase: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CrearSesion([FromBody] Sesion nuevaSesion)
        {
            if (User.IsInRole("Administrador") == false)
            {
                return Json(new { success = false, message = "Acceso denegado: Acción no autorizada." });
            }

            try
            {
                string resultadoValidacion = await this.repo.ValidarSesionAsync(nuevaSesion.Fecha, nuevaSesion.HoraInicio, nuevaSesion.HoraFin, nuevaSesion.EntrenadorId, nuevaSesion.SalaId);

                if (resultadoValidacion != "OK")
                {
                    return Json(new { success = false, message = resultadoValidacion });
                }

                await this.repo.CreateSesionesAsync(nuevaSesion.ClaseId, nuevaSesion.EntrenadorId, nuevaSesion.SalaId, nuevaSesion.Fecha, nuevaSesion.HoraInicio, nuevaSesion.HoraFin);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error inesperado al crear la sesión: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditarSesion([FromBody] Sesion sesionEditada)
        {
            if (User.IsInRole("Administrador") == false)
            {
                return Json(new { success = false, message = "Acceso denegado: Acción no autorizada." });
            }

            try
            {
                string resultadoValidacion = await this.repo.ValidarSesionAsync(sesionEditada.Fecha, sesionEditada.HoraInicio, sesionEditada.HoraFin, sesionEditada.EntrenadorId, sesionEditada.SalaId, sesionEditada.IdSesion);

                if (resultadoValidacion != "OK")
                {
                    return Json(new { success = false, message = resultadoValidacion });
                }

                await this.repo.UpdateSesionAsync(sesionEditada.IdSesion, sesionEditada.ClaseId, sesionEditada.EntrenadorId, sesionEditada.SalaId, sesionEditada.Fecha, sesionEditada.HoraInicio, sesionEditada.HoraFin);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error inesperado al actualizar: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EliminarSesion(int id)
        {
            if (User.IsInRole("Administrador") == false)
            {
                return Json(new { success = false, message = "Acceso denegado: Acción no autorizada." });
            }

            try
            {
                await this.repo.DeleteSesionAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }
        }
    }
}
