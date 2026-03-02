using Gymphony.Extensions;
using Gymphony.Helpers;
using Gymphony.Models;
using Gymphony.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Gymphony.Controllers
{
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
            int idUser = HttpContext.Session.GetObject<int>("IDUSUARIO");
            if(idUser == 0)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                List<int> sesionesReservadas = await this.repo.GetSesionesReservadasClienteAsync(idUser);
                ViewData["SESIONES_RESERVADAS"] = sesionesReservadas;

                List<DatosSesion> sesiones = await this.repo.GetSesionesNuevasAsync();
                return View(sesiones);
            }
        }

        public async Task<IActionResult> ReservarSesiones(int idSesion)
        {
            int idUser = HttpContext.Session.GetObject<int>("IDUSUARIO");
            int idRol = HttpContext.Session.GetObject<int>("IDROLUSUARIO");
            if (idUser == 0)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                if (idRol != 2)
                {
                    TempData["MENSAJERESERVA"] = "Acceso denegado: Solo los clientes pueden reservar clases.";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["MENSAJERESERVA"] = await this.repo.ReservarPlazaAsync(idSesion, idUser);
                    return RedirectToAction("Index", "Home");
                }
            }
        }

        public async Task<IActionResult> AnularSesiones(int idSesion)
        {
            int idUser = HttpContext.Session.GetObject<int>("IDUSUARIO");
            int idRol = HttpContext.Session.GetObject<int>("IDROLUSUARIO");
            if (idUser == 0)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                if (idRol != 2)
                {
                    TempData["MENSAJERESERVA"] = "Acceso denegado: Los empleados no pueden anular reservas.";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["MENSAJERESERVA"] = await this.repo.AnularReservaAsync(idSesion, idUser);
                    return RedirectToAction("Index", "Home");
                }
            }
        }

        public async Task<IActionResult> Perfil()
        {
            int idUser = HttpContext.Session.GetObject<int>("IDUSUARIO");
            int idRol = HttpContext.Session.GetObject<int>("IDROLUSUARIO");
            if (idUser == 0)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                Usuario user = await this.repo.FindUsuarioAsync(idUser);
                Rol rol = await this.repo.FindRolPorIdRolAsync(user.RoleId);
                ViewData["ROL"] = rol.NombreRol;
                ViewData["PATHFOTO"] = this.helper.MapUrlPath(user.RutaFoto, Folders.Usuarios);

                if (idRol == 2)
                {
                    List<DatosSesion> misReservas = await this.repo.GetMisSesionesCompletasAsync(idUser);
                    ViewData["MIS_RESERVAS"] = misReservas;
                }
                else if (idRol == 3)
                {
                    List<HorarioEmpleados> misHorarios = await this.repo.GetHorarioUsuarioPorIdAsync(idUser);
                    ViewData["MIS_HORARIOS"] = misHorarios;
                }

                return View(user);
            }
        }
    }
}
