using Gymphony.Filters;
using Gymphony.Helpers;
using Gymphony.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using QRCoder;

namespace Gymphony.Controllers
{
    [AuthorizeUsuarios]
    public class AccesosController : Controller
    {
        private RepositoryGymphony repo;

        public AccesosController(RepositoryGymphony repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> AforoActual()
        {
            int aforo = await this.repo.GetAforoActualAsync();
            ViewData["AFORO"] = aforo;
            return View();
        }

        public IActionResult QRAcceso()
        {
            if (User.IsInRole("Socio") == false)
            {
                TempData["MENSAJERESERVA"] = "Acceso denegado: Solo los socios tienen acceso al QR.";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                int idUser = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(idUser.ToString(), QRCodeGenerator.ECCLevel.H);
                Base64QRCode qrCode = new Base64QRCode(qrCodeData);
                string qrBase64 = qrCode.GetGraphic(20);
                ViewData["QR"] = qrBase64;
                return View();
            }
        }

        public IActionResult LectorCodigoQR()
        {
            if (User.IsInRole("Administrador") == false)
            {
                TempData["MENSAJERESERVA"] = "Acceso denegado: Solo el Administrador tiene acceso.";
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ControlAccesoQR(int idSocio)
        {
            if (User.IsInRole("Administrador") == false)
            {
                return Json(new { success = false, message = "Acceso denegado." });
            }

            try
            {
                string accion = await this.repo.RegistrarAccesoAsync(idSocio);
                return Json(new { success = true, operacion = accion });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error de lectura: " + ex.Message });
            }
        }
    }
}
