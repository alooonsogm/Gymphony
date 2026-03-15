using Gymphony.Extensions;
using Gymphony.Models;
using Gymphony.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Gymphony.Controllers
{
    public class ManagedController : Controller
    {
        private RepositoryGymphony repo;

        public ManagedController(RepositoryGymphony repo)
        {
            this.repo = repo;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            ValidacionUsuario user = await this.repo.LogInUserAsync(email, password);

            if (user == null)
            {
                ViewData["MENSAJE"] = "Credenciales incorrectas";
                return View();
            }
            else
            {
                VistaUsuario usuario = await this.repo.FindVistaUsuarioAsync(user.Id);
                if (usuario.NombreRol == "Socio")
                {
                    bool isActive = await this.repo.IsSocioActivoAsync(usuario.IdUsuario);
                    if (isActive == false)
                    {
                        ViewData["MENSAJE"] = "Tu membresía está dada de baja. Por favor, contacta con administración.";
                        return View();
                    }
                }

                ClaimsIdentity identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                Claim claimName = new Claim(ClaimTypes.Name, usuario.Nombre);
                identity.AddClaim(claimName);
                Claim claimIdUsuario = new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString());
                identity.AddClaim(claimIdUsuario);
                Claim claimRol = new Claim(ClaimTypes.Role, usuario.NombreRol);
                identity.AddClaim(claimRol);
                ClaimsPrincipal userPrincipal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal);
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Managed");
        }
    }
}
