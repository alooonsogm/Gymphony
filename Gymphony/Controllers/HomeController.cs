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
                Usuario user = await this.repo.FindUsuarioAsync(idUser);
                ViewData["NOMBREUSER"] = user.Nombre;

                List<DatosSesion> sesiones = await this.repo.GetSesionesAsync();
                return View(sesiones);
            }
        }

        public async Task<IActionResult> Perfil()
        {
            int idUser = HttpContext.Session.GetObject<int>("IDUSUARIO");
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
                return View(user);
            }
        }
    }
}
