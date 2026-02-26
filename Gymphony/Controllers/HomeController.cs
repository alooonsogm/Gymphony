using Gymphony.Extensions;
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

        public HomeController(RepositoryGymphony repo)
        {
            this.repo = repo;
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
                return View(user);
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
                return View(user);
            }
        }
    }
}
