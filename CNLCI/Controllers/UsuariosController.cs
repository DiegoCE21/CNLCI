using CNLCI.Models;
using System;
using System.Web.Mvc;
namespace CNLCI.Controllers { 

public class UsuariosController : Controller
{
    [HttpPost]
    public ActionResult AgregarUsuario(string correo)
    {
        if (!string.IsNullOrEmpty(correo))
        {
            try
            {
                using (DB db = new DB()) 
                {
                    var nuevoUsuario = new Usuarios { Correo = correo };

                    db.Usuarios.Add(nuevoUsuario);
                    db.SaveChanges();
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Se produjo un error al agregar el usuario: " + ex.Message);
                return View("AddUsers");
            }
        }
        else
        {
            ModelState.AddModelError("correo", "El correo electrónico es obligatorio");
            return View("AddUsers");
        }
    }
}
}