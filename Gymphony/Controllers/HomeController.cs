using Gymphony.Extensions;
using Gymphony.Filters;
using Gymphony.Helpers;
using Gymphony.Models;
using Gymphony.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Gymphony.Controllers
{
    [AuthorizeUsuarios]
    public class HomeController : Controller
    {
        private RepositoryGymphony repo;
        private HelperPath helper;

        public HomeController(RepositoryGymphony repo, HelperPath helper)
        {
            this.repo = repo;
            this.helper = helper;
        }

        public async Task<IActionResult> Index()
        {
            int idUser = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            List<int> sesionesReservadas = await this.repo.GetSesionesReservadasClienteAsync(idUser);
            ViewData["SESIONES_RESERVADAS"] = sesionesReservadas;

            List<DatosSesion> sesiones = await this.repo.GetSesionesNuevasAsync();
            return View(sesiones);
        }

        public async Task<IActionResult> ReservarSesiones(int idSesion)
        {
            int idUser = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (User.IsInRole("Socio") == false)
            {
                TempData["MENSAJERESERVA"] = "Acceso denegado: Solo los socios pueden reservar clases.";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["MENSAJERESERVA"] = await this.repo.ReservarPlazaAsync(idSesion, idUser);
                DatosSesion sesion = await this.repo.FindDatosSesionAsync(idSesion);
                TempData["NOMBRECLASE"] = sesion.NombreClase;
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> AnularSesiones(int idSesion)
        {
            int idUser = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (User.IsInRole("Socio") == false)
            {
                TempData["MENSAJERESERVA"] = "Acceso denegado: Solo los socios pueden anular reservas.";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["MENSAJERESERVA"] = await this.repo.AnularReservaAsync(idSesion, idUser);
                DatosSesion sesion = await this.repo.FindDatosSesionAsync(idSesion);
                TempData["NOMBRECLASE"] = sesion.NombreClase;
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> GetSociosSesion(int idSesion)
        {
            int idUser = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (User.IsInRole("Socio") == true)
            {
                TempData["MENSAJERESERVA"] = "Acceso denegado: Los socios no pueden acceder a esta información.";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                List<Usuario> socios = await this.repo.GetUsuariosPorSesionAsync(idSesion);
                var resultado = socios.Select(s => new {nombreCompleto = s.Nombre + " " + s.Apellidos, email = s.Email}).ToList();
                return Json(resultado);
            }
        }

        public async Task<IActionResult> Perfil()
        {
            int idUser = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            string rol = HttpContext.User.FindFirst(ClaimTypes.Role).Value;

            Usuario user = await this.repo.FindUsuarioAsync(idUser);
            ViewData["ROL"] = rol;
            ViewData["PATHFOTO"] = this.helper.MapUrlPath(user.RutaFoto, Folders.Usuarios);

            if (rol == "Socio")
            {
                List<DatosSesion> misReservas = await this.repo.GetMisSesionesCompletasAsync(idUser);
                ViewData["MIS_RESERVAS"] = misReservas;
            }
            else if (rol == "Entrenador")
            {
                List<HorarioEmpleados> misHorarios = await this.repo.GetHorarioUsuarioPorIdAsync(idUser);
                ViewData["MIS_HORARIOS"] = misHorarios;
            }

            return View(user);
        }
    }
}
