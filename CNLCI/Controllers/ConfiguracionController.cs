using CNLCI.Models;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CNLCI.Controllers
{
    [Authorize]
    public class ConfiguracionController : Controller
    {
        public ActionResult Importar()
        { return View(); }


        [HttpPost]
        public ActionResult AgregarUsuario(string correo)
        {
            if (string.IsNullOrEmpty(correo))
            {
                ModelState.AddModelError("correo", "El correo electrónico es obligatorio");
                return View("Usuarios"); // Asegúrate de regresar a la vista "Usuarios" si hay un error.
            }

            try
            {
                using (DB db = new DB()) // Asegura que 'DB' es tu contexto de EF correcto.
                {
                    var nuevoUsuario = new Usuarios { Correo = correo };
                    db.Usuarios.Add(nuevoUsuario); // Verifica que 'Usuario' sea el nombre correcto de tu DbSet.
                    db.SaveChanges();
                }
                return RedirectToAction("Index", "Home"); // Redirige a la página de inicio u otra página de confirmación.
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Se produjo un error al agregar el usuario: " + ex.Message);
                return View("Usuarios"); // Regresa a la vista de Usuarios con el mensaje de error.
            }
        }

        // Asegúrate de tener un método GET para Usuarios que pueda regresar la vista correctamente.
        [HttpGet]
        /*public ActionResult Usuarios()
        {
            return View();
        }*/
        public ActionResult Usuarios()
        {
            using (DB db = new DB()) // Asume que 'DB' es tu contexto de EF.
            {
                var listaUsuarios = db.Usuarios.Where(u => u.tipoAcceso == "Administrador").ToList();
                return View(listaUsuarios); // Pasa la lista filtrada de usuarios a la vista.
            }
        }



        [HttpPost]
        public ActionResult Importar(HttpPostedFileBase archivo)
        {
            string mensaje = "";

            if (archivo != null && archivo.ContentLength > 0)
            {
                string RutaArchivo = Server.MapPath("~/ArchivosPI/" + Path.GetFileName(archivo.FileName));
                archivo.SaveAs(RutaArchivo);

                using (DB db = new DB())
                {
                    try
                    {
                        db.Database.ExecuteSqlCommand("EXEC importar @RutaArchivo", new SqlParameter("@RutaArchivo", RutaArchivo));
                        mensaje = "Los registros se han subido correctamente.";
                    }
                    catch (Exception ex)
                    {
                        mensaje = "Error al subir los registros: " + ex.Message;
                    }
                }
            }
            else
            {
                mensaje = "No se ha seleccionado ningún archivo o el archivo está vacío.";
            }



            return Json(mensaje, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CopiaS()
        {
            string connectionString = "data source=WIN-HTKAANTQI7R\\CONALEPCHECKIN;initial catalog=CONALEPcheckin;persist security info=True;;Initial Catalog=CONALEPcheckin;user id=sa;password=Chec-123";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var nombre = "N'C:/Users/Administrador/Downloads/CONALEPcheckin"+ DateTime.Now.ToString().Substring(0, 2)+ DateTime.Now.ToString().Substring(3,2) + DateTime.Now.ToString().Substring(6, 4) + ".bak'";

                using (var command = new SqlCommand("BACKUP DATABASE [CONALEPcheckin] TO DISK = "+nombre+" WITH NOFORMAT, NOINIT, NAME = N'CONALEPcheckin-Full Database Backup', SKIP, NOREWIND, NOUNLOAD, STATS = 10", connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            return Json("La copia de seguridad se ha guardado con éxito en Descargas", JsonRequestBehavior.AllowGet);
        }








        [HttpGet]
        public ActionResult EditarUsuario(int id)
        {
            using (DB db = new DB())
            {
                var usuario = db.Usuarios.Find(id);
                if (usuario == null)
                {
                    return HttpNotFound();
                }
                return View(usuario);
            }
        }

        [HttpPost]
        public ActionResult EditarUsuario(Usuarios usuario)
        {
            if (ModelState.IsValid)
            {
                using (DB db = new DB())
                {
                    db.Entry(usuario).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Usuarios");
                }
            }
            return View(usuario);
        }

        [HttpGet] // Opcional, si quieres una página de confirmación para eliminar.
        public ActionResult EliminarUsuarioConfirmacion(int id)
        {
            using (DB db = new DB())
            {
                var usuario = db.Usuarios.Find(id);
                if (usuario == null)
                {
                    return HttpNotFound();
                }
                return View(usuario);
            }
        }

        public ActionResult MiVista() // Este sería el método del controlador que carga tu vista
        {
            string emailUsuarioActual = User.Identity.Name; // Obtener el correo del usuario actual
            string tipoAcceso = "Alumno"; // Valor por defecto o manejo de no encontrado

            using (DB db = new DB()) // Asume que 'DB' es tu contexto de EF
            {
                var usuario = db.Usuarios.FirstOrDefault(u => u.Correo == emailUsuarioActual);
                if (usuario != null)
                {
                    tipoAcceso = usuario.tipoAcceso;
                }
            }

            ViewBag.TipoAcceso = tipoAcceso; // Pasar el tipo de acceso a la vista
            return View();
        }


        [HttpPost]
        public ActionResult EliminarUsuario(int id)
        {
            using (DB db = new DB())
            {
                var usuario = db.Usuarios.Find(id);
                if (usuario != null)
                {
                    db.Usuarios.Remove(usuario);
                    db.SaveChanges();
                }
                return RedirectToAction("Usuarios");
            }
        }
    }
}