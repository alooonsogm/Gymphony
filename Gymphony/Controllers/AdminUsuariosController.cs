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

        public IActionResult AltaSocio()
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
        public async Task<IActionResult> AltaSocio(string email, string password, string nombre, string apellidos, string telefono, DateOnly fechaNacimiento, string dni, IFormFile foto)
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
                    return RedirectToAction("PanelUsuarios", new { seccion = "socios" });
                }
            }
        }
    }
}
