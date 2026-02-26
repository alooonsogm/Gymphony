using Gymphony.Extensions;
using Gymphony.Models;
using Gymphony.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Gymphony.Controllers
{
    public class LoginController : Controller
    {
        private RepositoryGymphony repo;

        public LoginController(RepositoryGymphony repo)
        {
            this.repo = repo;
        }

        public IActionResult Index()
        {
            HttpContext.Session.SetObjetc("IDUSUARIO", 0);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string email, string password)
        {
            ValidacionUsuario user = await this.repo.LogInUserAsync(email, password);
            if (user == null)
            {
                ViewData["MENSAJE"] = "Credenciales incorrectas";
                return View();
            }
            else
            {
                Usuario usuario = await this.repo.FindUsuarioAsync(user.Id);
                HttpContext.Session.SetObjetc("IDUSUARIO", usuario.IdUsuario);
                HttpContext.Session.SetObjetc("NOMBREUSUARIO", usuario.Nombre);
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
