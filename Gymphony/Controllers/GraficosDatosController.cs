using Gymphony.Filters;
using Gymphony.Models;
using Gymphony.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gymphony.Controllers
{
    [AuthorizeUsuarios]
    public class GraficosDatosController : Controller
    {
        private RepositoryGymphony repo;

        public GraficosDatosController(RepositoryGymphony repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> PanelEstadisticas(string seccion)
        {
            if (User.IsInRole("Socio") == true)
            {
                TempData["MENSAJERESERVA"] = "Acceso denegado: Área exclusiva para la plantilla del gimnasio.";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                if (seccion == null)
                {
                    seccion = "evolucion";
                }
                ViewData["SECCIONACTIVA"] = seccion;

                if (seccion == "evolucion")
                {
                    ViewData["EVOLUCION"] = await this.repo.GetEvolucionSociosAsync();
                }
                else if (seccion == "horas")
                {
                    ViewData["HORASPICO"] = await this.repo.GetHorasPicoAsync();
                }
                return View();
            }

        }

        public async Task<IActionResult> AsistenciaSocio()
        {
            if (User.IsInRole("Socio") == false)
            {
                TempData["MENSAJERESERVA"] = "Acceso denegado: Esta vista es exclusiva para el seguimiento de socios.";
                return RedirectToAction("Index", "Home");
            }

            int idUser = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            List<string> diasAsistencia = await this.repo.GetDiasAsistenciaSocioAsync(idUser);
            ViewData["ASISTENCIA"] = diasAsistencia;
            return View();
        }
    }
}
