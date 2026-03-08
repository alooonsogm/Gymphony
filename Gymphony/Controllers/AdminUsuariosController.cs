using Gymphony.Extensions;
using Gymphony.Models;
using Gymphony.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Gymphony.Controllers
{
    public class AdminUsuariosController : Controller
    {
        private RepositoryGymphony repo;

        public AdminUsuariosController(RepositoryGymphony repo)
        {
            this.repo = repo;
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
                    model.Socios = await this.repo.GetUsuariosPorRolAsync(2);
                    model.Entrenadores = await this.repo.GetUsuariosPorRolAsync(3);
                    return View(model);
                }
            }
        }
    }
}
