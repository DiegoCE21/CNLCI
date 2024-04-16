using CNLCI.Models;
using CNLCI.Models.ViewModel;
using Rotativa;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.Graph;
using System.Windows.Forms;

namespace CNLCI.Controllers
{

    [Authorize]
    public class JustificantesController : Controller
    {
        public ActionResult Justificante()
        {

            using (DB db = new DB())
            {
                string matricula = "Matrícula";
                var datos = db.Alumno.Where(a => a.Matricula == matricula).ToList();

                return View(datos);
            }
        }
        public ActionResult JGrupal()
        {
            return View();
        }
        public string ObtenerCorreoAlumno(string matricula)
        {
            using (var db = new DB())
            {
                var alumno = db.Alumno.FirstOrDefault(a => a.Matricula == matricula);
                return alumno?.Email_institucional;
            }
        }

        public int justificantesAlumno(string matricula)
        {
            using (var db = new DB())
            {
                var justificantes = db.Justificante.Count(j => j.Matricula == matricula);
                return justificantes;
            }
        }



        public List<string> ObtenerCorreoDocente(int numeroJustificante)
        {
            using (var db = new DB())
            {
                var correosDocente = (from d in db.Docente
                                      join m in db.Materias on d.DocenteID equals m.DocenteID
                                      join qa in db.Justificante on m.Alumno equals qa.Matricula
                                      where db.Justificante.Any(j => j.No__justificante == numeroJustificante)
                                      select d.CorreoDocente).Distinct().ToList();
                return correosDocente;
            }
        }


        public JsonResult ObtenerGruposPorCarrera(string planDeEstudiosCve)
        {
            using (DB db = new DB())
            {
                var grupos = db.Grupo
                    .Where(g => g.Carrera.Plan_de_Estudios == planDeEstudiosCve)
                    .Select(g => new { Nombre = g.Grupo_Referente })
                    .ToList();

                return Json(grupos, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult PaseDeSalida(string ruta = null)
        {
            ViewBag.Ruta = ruta;
            string matricula = "Matrícula";
            using (DB db = new DB())
            {
                var planesEstudio = db.Carrera.ToList();
                ViewBag.PlanesEstudio = new SelectList(planesEstudio, "Plan_de_Estudios_Cve", "Plan_de_Estudios");
                var datos = (from Alumno in db.Alumno
                             join Tutor in db.Tutor on Alumno.Matricula equals Tutor.Matricula
                             where matricula == Alumno.Matricula
                             select new MJ
                             {
                                 Matricula = Alumno.Matricula,
                                 Nombre = Alumno.Nombre,
                                 Primer_apellido = Alumno.Primer_apellido,
                                 Segundo_apellido = Alumno.Segundo_apellido,
                                 Tutor_Nombre = Tutor.Tutor_Nombre

                             }).ToList();
                return View("PaseDeSalida", datos);
            }

        }
        [HttpPost]
        public ActionResult GenerarP(string Nombre, string Primer_apellido, string Segundo_Apellido,
        string Matricula, string Carrera, string Grupo, string Motivo, string Medio, string autorizo,
        HttpPostedFileBase archivo, string TutorN, string parentezco, string nombreA, string Observaciones)
        {
            var nombre = ((System.Security.Claims.ClaimsIdentity)User.Identity).FindFirst("preferred_username");
            var n = nombre?.Value ?? "No tiene nombre registrado";
            string par, TN;
            byte[] data;
            if (archivo != null)
            {
                string Extension = Path.GetExtension(archivo.FileName);
                MemoryStream ms = new MemoryStream();
                archivo.InputStream.CopyTo(ms);
                data = ms.ToArray();

            }
            else { data = null; }

            using (DB db = new DB())
            {

                if (autorizo == "Tutor")
                {
                    TN = TutorN;
                    par = "TUTOR";
                }
                else
                {
                    par = parentezco;
                    TN = nombreA;
                }


                Pase_de_salida p = new Pase_de_salida
                {
                    Matricula = Matricula,
                    Nombre = Nombre,
                    Primer_apellido = Primer_apellido,
                    Segundo_apellido = Segundo_Apellido,
                    Plan_de_Estudios = Carrera,
                    Grupo_Referente = Grupo,
                    Motivo = Motivo,
                    Autoriza = TN,
                    Parentezco = par,
                    Medio_de_Autorizacion = Medio,
                    Comprobante = data,
                    Activo = true,
                    Valor = 1,
                    Fecha = System.DateTime.Now,
                    Observaciones = Observaciones,
                    ElaboradoPor = User.Identity.Name


                };
                db.Pase_de_salida.Add(p);
                db.SaveChanges();

                int ultimoNoPase = db.Pase_de_salida.Max(up => up.No__pase);
                bitacora b = new bitacora
                {
                    accion = "Insercion",
                    realizada_por = n,
                    fecha = System.DateTime.Now,
                    tabla = "Pase de salida",
                    folio = ultimoNoPase
                };

                db.bitacora.Add(b);
                db.SaveChanges();

                Pase_de_salida modelo = db.Pase_de_salida.OrderByDescending(x => x.No__pase).FirstOrDefault();
                P ultimoRegistro = new P
                {
                    No__pase = modelo.No__pase,
                    Matricula = modelo.Matricula,
                    Nombre = modelo.Nombre,
                    Primer_apellido = modelo.Primer_apellido,
                    Segundo_apellido = modelo.Segundo_apellido,
                    Plan_de_Estudios = modelo.Plan_de_Estudios,
                    Grupo_Referente = modelo.Grupo_Referente,
                    Motivo = modelo.Motivo,
                    Autoriza = modelo.Autoriza,
                    Parentezco = modelo.Parentezco,
                    Medio_de_Autorizacion = modelo.Medio_de_Autorizacion,
                    Fecha = modelo.Fecha,
                    ElaboradoPor = modelo.ElaboradoPor

                };

                if (ultimoRegistro != null)
                {
                    return RedirectToAction("GenerarPDF", ultimoRegistro);
                }
            }

            return RedirectToAction("PaseDeSalida");

        }
        public ActionResult GenerarPdfJ(J modelo)
        {
            // Asumiendo que ya tienes el código para generar el PDF y guardar la ruta en rutaArchivo
            string nombreArchivo = "Justificante" + ".pdf";
            var viewAsPdf = new ViewAsPdf("PDFJustificante", modelo); // Asegúrate de tener la vista correcta
            viewAsPdf.PageSize = Rotativa.Options.Size.Letter;
            viewAsPdf.PageMargins = new Rotativa.Options.Margins(13, 13, 13, 13);
            byte[] pdfBytes = viewAsPdf.BuildFile(ControllerContext);
            string rutaArchivo = Path.Combine(Server.MapPath("~/ArchivosPDF"), nombreArchivo);
            System.IO.File.WriteAllBytes(rutaArchivo, pdfBytes);

            // Nuevo código para enviar el correo

            string correoAlumno = ObtenerCorreoAlumno(modelo.Matricula);
            using (var db = new DB())
            {
                var correosDocente = (from d in db.Docente
                                      join m in db.Materias on d.DocenteID equals m.DocenteID
                                      join qa in db.Justificante on m.Alumno equals qa.Matricula
                                      where db.Justificante.Any(j => j.No__justificante == modelo.No__justificante)
                                      select d.CorreoDocente).Distinct().ToList();

                foreach (var correo in correosDocente)
                {
                    EnviarCorreoConPDFDocente(correo, rutaArchivo);
                }
            }
            EnviarCorreoConPDF(correoAlumno, rutaArchivo);


            string nombreArchivoF = Request.Url.GetLeftPart(UriPartial.Authority) + "/ArchivosPDF/" + nombreArchivo;
            return Json(nombreArchivoF, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _Carrerajg()
        {
            var identity = User.Identity as ClaimsIdentity;
            var emailClaim = identity?.FindFirst(ClaimTypes.Email);
            var correoUsuario = emailClaim?.Value;

            using (DB db = new DB())
            {
                var usuario = db.Usuarios.FirstOrDefault(u => u.Correo == correoUsuario);
                bool esAdministrador = usuario != null && usuario.tipoAcceso.Contains("Administrador");

                if (usuario != null)
                {
                    if (esAdministrador)
                    {
                        var planes = db.Grupo.Select(g => g.Plan_de_Estudios_Cve).Distinct().ToList();
                        ViewBag.PlanesDeEstudio = new SelectList(planes.Select(x => new { Value = x, Text = x }).ToList(), "Value", "Text");
                    }
                    else
                    {
                        string[] palabrasClave = { "COP", "ICI", "ISI", "IND", "IGE" };
                        var palabraClaveEncontrada = palabrasClave.FirstOrDefault(palabra => usuario.tipoAcceso.Contains(palabra));

                        if (!string.IsNullOrEmpty(palabraClaveEncontrada))
                        {
                            ViewBag.PlanDeEstudioSeleccionado = palabraClaveEncontrada;
                            var grupos = db.Grupo.Where(g => g.Plan_de_Estudios_Cve == palabraClaveEncontrada).Select(g => g.Grupo_Referente).Distinct().ToList();
                            ViewBag.Grupos = grupos;
                        }
                    }
                }
            }

            if (ViewBag.PlanesDeEstudio == null && ViewBag.PlanDeEstudioSeleccionado == null)
            {
                ViewBag.PlanesDeEstudio = new SelectList(Enumerable.Empty<SelectListItem>());
                ViewBag.Grupos = Enumerable.Empty<string>();
            }

            List<Grupo> gruposDisponibles = new List<Grupo>();
            return PartialView(gruposDisponibles);
        }

        [HttpPost]
        public ActionResult GenerarJG(string Carrera, string Grupo, string Motivo, string fecha, string Observaciones, HttpPostedFileBase archivo, DateTime? fecha1 = null, DateTime? fechaDelDia = null, DateTime? fechaAlDia = null)
        {
            var nombre = ((System.Security.Claims.ClaimsIdentity)User.Identity).FindFirst("preferred_username");
            var n = nombre?.Value ?? "No tiene nombre registrado";

            byte[] data;
            if (archivo != null)
            {
                string Extension = Path.GetExtension(archivo.FileName);
                MemoryStream ms = new MemoryStream();
                archivo.InputStream.CopyTo(ms);
                data = ms.ToArray();
            }
            else
            {
                data = null;
            }

            // Obtener los grupos disponibles

            using (DB db = new DB())
            {
                var LAlumnos = db.Alumno.Where(a => a.Grupo_Referente == Grupo && a.Plan_de_Estudios == Carrera).ToList();

                if (fecha == "unica")
                {
                    foreach (var LA in LAlumnos)
                    {
                        Justificante justificante = new Justificante
                        {
                            Nombre = LA.Nombre,
                            Primer_apellido = LA.Primer_apellido,
                            Segundo_apellido = LA.Segundo_apellido,
                            Matricula = LA.Matricula,
                            Plan_de_Estudios = Carrera,
                            Grupo_Referente = Grupo,
                            Motivo = Motivo,
                            Fecha_del_dia = fecha1,
                            Fecha_al_dia = fecha1,
                            Comprobante = data,
                            Valor = 1,
                            Activo = true,
                            Observaciones = Observaciones,
                            ElaboradoPor = User.Identity.Name

                        };

                        db.Justificante.Add(justificante);
                        db.SaveChanges();
                    }
                }
                else
                {
                    int valor = 0;

                    for (DateTime F = (DateTime)fechaDelDia; F <= (DateTime)fechaAlDia; F = F.AddDays(1))
                    {
                        valor = valor + 1;
                    }
                    foreach (var LA in LAlumnos)
                    {
                        Justificante justificante = new Justificante
                        {
                            Nombre = LA.Nombre,
                            Primer_apellido = LA.Primer_apellido,
                            Segundo_apellido = LA.Segundo_apellido,
                            Matricula = LA.Matricula,
                            Plan_de_Estudios = Carrera,
                            Grupo_Referente = Grupo,
                            Motivo = Motivo,
                            Fecha_del_dia = fechaDelDia,
                            Fecha_al_dia = fechaAlDia,
                            Comprobante = data,
                            Valor = valor,
                            Activo = true,
                            Observaciones = Observaciones,
                            ElaboradoPor = User.Identity.Name

                        };
                        db.Justificante.Add(justificante);
                        db.SaveChanges();
                    }
                }

                int ultimoNoJus = db.Justificante.Max(up => up.No__justificante);
                bitacora b = new bitacora
                {
                    accion = "Insercion",
                    realizada_por = n,
                    fecha = DateTime.Now,
                    tabla = "Justificante",
                    folio = ultimoNoJus
                };

                db.bitacora.Add(b);
                db.SaveChanges();

                Justificante modelo = db.Justificante.OrderByDescending(x => x.No__justificante).FirstOrDefault();
                J ultimoRegistro = new J
                {
                    No__justificante = modelo.No__justificante,
                    Matricula = modelo.Matricula,
                    Nombre = modelo.Nombre,
                    Primer_apellido = modelo.Primer_apellido,
                    Segundo_apellido = modelo.Segundo_apellido,
                    Plan_de_Estudios = modelo.Plan_de_Estudios,
                    Grupo_Referente = modelo.Grupo_Referente,
                    Motivo = modelo.Motivo,
                    Fecha_del_dia = modelo.Fecha_del_dia,
                    Fecha_al_dia = modelo.Fecha_al_dia,
                    ElaboradoPor = modelo.ElaboradoPor
                };
                if (ultimoRegistro != null)
                {
                    return RedirectToAction("GenerarPDFJG", ultimoRegistro);
                }
            }

            return RedirectToAction("Justificante");
        }

        public ActionResult GenerarPDFJG(J modelo)
        {
            string nombreArchivo = "JustificanteGrupal_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf"; // Asegura un nombre de archivo único con timestamp
            var viewAsPdf = new ViewAsPdf("PDFJGrupal", modelo)
            {
                PageSize = Rotativa.Options.Size.Letter,
                PageMargins = new Rotativa.Options.Margins(13, 13, 13, 13)
            };

            byte[] pdfBytes = viewAsPdf.BuildFile(ControllerContext); // Genera el PDF
            string rutaArchivo = Path.Combine(Server.MapPath("~/ArchivosPDF"), nombreArchivo);

            System.IO.File.WriteAllBytes(rutaArchivo, pdfBytes); // Guarda el PDF en el sistema de archivos            
            using (var db = new DB())
            {
                var alumnos = db.Alumno
                    .Where(a => a.Grupo_Referente == modelo.Grupo_Referente && a.Plan_de_Estudios == modelo.Plan_de_Estudios)
                    .ToList();

                foreach (var alumno in alumnos)
                {
                    string correoAlumno = ObtenerCorreoAlumno(alumno.Matricula);
                    if (!string.IsNullOrEmpty(correoAlumno))
                    {
                        EnviarCorreoConPDF(correoAlumno, rutaArchivo); // Envía el correo con el PDF adjunto
                    }
                }
            }

            using (var db = new DB())
            {
                var correosDocente = (from d in db.Docente
                                      join m in db.Materias on d.DocenteID equals m.DocenteID
                                      join qa in db.Justificante on m.Alumno equals qa.Matricula
                                      where db.Justificante.Any(j => j.No__justificante == modelo.No__justificante)
                                      select d.CorreoDocente).Distinct().ToList();

                foreach (var correo in correosDocente)
                {
                    EnviarCorreoConPDFDocenteG(correo, rutaArchivo);
                }
            }

            string nombreArchivoF = Request.Url.GetLeftPart(UriPartial.Authority) + "CNLCI/ArchivosPDF/" + nombreArchivo;
            return Json(nombreArchivoF, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Buscar(string matricula)
        {
            var identity = User.Identity as ClaimsIdentity;
            var emailClaim = identity?.FindFirst(ClaimTypes.Email);
            var correoUsuario = emailClaim?.Value;

            using (DB db = new DB())
            {
                var usuario = db.Usuarios.FirstOrDefault(u => u.Correo == correoUsuario);

                if (usuario != null)
                {
                    // Verificar si el usuario es un Administrador
                    if (usuario.tipoAcceso.Contains("Administrador"))
                    {
                        // Consulta para Administradores, sin filtro de Plan_de_Estudios
                        var datos = db.Alumno.Where(a => a.Matricula == matricula).ToList();
                        foreach (var alumno in datos)
                        {
                            alumno.NumJustificantes = justificantesAlumno(alumno.Matricula);
                        }
                        return View("Justificante", datos);
                    }
                    else
                    {
                        // Lógica existente para otros roles
                        string[] palabrasClave = { "COP", "ICI", "ISI", "IND", "IGE" };
                        string planDeEstudios = null;

                        foreach (var palabra in palabrasClave)
                        {
                            if (usuario.tipoAcceso.Contains(palabra))
                            {
                                planDeEstudios = palabra;
                                break;
                            }
                        }

                        if (planDeEstudios != null)
                        {
                            var datos = db.Alumno.Where(a => a.Matricula == matricula && a.Plan_de_Estudios == planDeEstudios).ToList();

                            if (datos.Any())
                            {
                                foreach (var alumno in datos)
                                {
                                    alumno.NumJustificantes = justificantesAlumno(alumno.Matricula);
                                }
                                return View("Justificante", datos);
                            }
                            else
                            {
                                ViewBag.ErrorMensaje = "Este alumno no pertenece a tu carrera";
                                return View("Justificante", new List<Alumno>());
                            }
                        }
                        else
                        {
                            ViewBag.ErrorMensaje = "Este alumno no pertenece a tu carrera";
                            return View("Justificante", new List<Alumno>());
                        }
                    }
                }
                else
                {
                    ViewBag.ErrorMensaje = "No se pudo encontrar el usuario";
                    return View("Justificante", new List<Alumno>());
                }
            }



        }
        public ActionResult BuscarP(string matricula)
        {
            using (DB db = new DB())
            {
                var datos = (from alumno in db.Alumno
                             where matricula == alumno.Matricula
                             select new MJ
                             {
                                 Matricula = alumno.Matricula,
                                 Nombre = alumno.Nombre,
                                 Primer_apellido = alumno.Primer_apellido,
                                 Segundo_apellido = alumno.Segundo_apellido,
                                 Plan_de_Estudios = alumno.Plan_de_Estudios,
                                 Grupo_Referente = alumno.Grupo_Referente
                                 // Tutor_Nombre = alumno.Tutor.Tutor_Nombre // Asegúrate de que Tutor esté disponible en la entidad Alumno
                             }).ToList();

                return View("PaseDeSalida", datos.AsEnumerable());



            }

        }
        [HttpPost]
        public ActionResult GenerarJ(string Nombre, string Primer_apellido, string Segundo_Apellido,
            string Matricula, string Carrera, string Grupo, string Motivo, string fecha, string Observaciones, HttpPostedFileBase archivo, DateTime? fecha1 = null
            , DateTime? fechaDelDia = null, DateTime? fechaAlDia = null)
        {
            var nombre = ((System.Security.Claims.ClaimsIdentity)User.Identity).FindFirst("preferred_username");
            var n = nombre?.Value ?? "No tiene nombre registrado";

            byte[] data;
            if (archivo != null)
            {

                string Extension = Path.GetExtension(archivo.FileName);
                MemoryStream ms = new MemoryStream();
                archivo.InputStream.CopyTo(ms);
                data = ms.ToArray();

            }
            else { data = null; }
            using (DB db = new DB())
            {


                if (fecha == "unica")
                {

                    Justificante justificante = new Justificante
                    {
                        Nombre = Nombre,
                        Primer_apellido = Primer_apellido,
                        Segundo_apellido = Segundo_Apellido,
                        Matricula = Matricula,
                        Plan_de_Estudios = Carrera,
                        Grupo_Referente = Grupo,
                        Motivo = Motivo,
                        Fecha_del_dia = fecha1,
                        Fecha_al_dia = fecha1,
                        Comprobante = data,
                        Valor = 1,
                        Activo = true,
                        Observaciones = Observaciones,
                        ElaboradoPor = User.Identity.Name

                    };

                    db.Justificante.Add(justificante);
                    db.SaveChanges();
                }
                else
                {
                    int valor = 0;

                    for (DateTime F = (DateTime)fechaDelDia; F <= (DateTime)fechaAlDia; F = F.AddDays(1))
                    {
                        valor = valor + 1;
                    }

                    Justificante justificante = new Justificante
                    {
                        Nombre = Nombre,
                        Primer_apellido = Primer_apellido,
                        Segundo_apellido = Segundo_Apellido,
                        Matricula = Matricula,
                        Plan_de_Estudios = Carrera,
                        Grupo_Referente = Grupo,
                        Motivo = Motivo,
                        Fecha_del_dia = fechaDelDia,
                        Fecha_al_dia = fechaAlDia,
                        Comprobante = data,
                        Valor = valor,
                        Activo = true,
                        Observaciones = Observaciones,
                        ElaboradoPor = User.Identity.Name

                    };
                    db.Justificante.Add(justificante);
                    db.SaveChanges();
                }

                int ultimoNoJus = db.Justificante.Max(up => up.No__justificante);
                bitacora b = new bitacora
                {
                    accion = "Insercion",
                    realizada_por = n,
                    fecha = DateTime.Now,
                    tabla = "Justificantes",
                    folio = ultimoNoJus
                };

                db.bitacora.Add(b);
                db.SaveChanges();

                Justificante modelo = db.Justificante.OrderByDescending(x => x.No__justificante).FirstOrDefault();
                J ultimoRegistro = new J
                {
                    No__justificante = modelo.No__justificante,
                    Matricula = modelo.Matricula,
                    Nombre = modelo.Nombre,
                    Primer_apellido = modelo.Primer_apellido,
                    Segundo_apellido = modelo.Segundo_apellido,
                    Plan_de_Estudios = modelo.Plan_de_Estudios,
                    Grupo_Referente = modelo.Grupo_Referente,
                    Motivo = modelo.Motivo,
                    Fecha_del_dia = modelo.Fecha_del_dia,
                    Fecha_al_dia = modelo.Fecha_al_dia,
                    ElaboradoPor = modelo.ElaboradoPor
                };
                if (ultimoRegistro != null)
                {
                    return RedirectToAction("GenerarPdfJ", ultimoRegistro);
                }
            }

            return RedirectToAction("Justificante");
        }

        public JsonResult ObtenerDatosAlumno(string matricula)
        {
            using (DB db = new DB())
            {
                var alumno = db.Alumno.FirstOrDefault(a => a.Matricula == matricula);
                if (alumno != null)
                {
                    var resultado = new
                    {
                        PlanDeEstudios = alumno.Plan_de_Estudios,
                        Grupo = alumno.Semestre
                    };
                    return Json(resultado, JsonRequestBehavior.AllowGet);
                }
                return Json(null);
            }
        }





        public void EnviarCorreoConPDF(string emailDestinatario, string pathPDF)
        {
            try
            {
                var mensaje = new MailMessage("pruebavinculacion@matehuala.tecnm.mx", emailDestinatario)
                {
                    Subject = "Tu justificante ha sido generado",
                    Body = "Aquí está el justificante que solicitaste.",
                    IsBodyHtml = true
                };
                mensaje.Attachments.Add(new System.Net.Mail.Attachment(pathPDF));

                using (var smtp = new SmtpClient("smtp.office365.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("pruebavinculacion@matehuala.tecnm.mx", "Chingatumadre_123");
                    smtp.EnableSsl = true;
                    smtp.Send(mensaje);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void EnviarCorreoConPDFDocente(string emailDestinatario, string pathPDF)
        {
            try
            {
                var mensaje = new MailMessage("pruebavinculacion@matehuala.tecnm.mx", emailDestinatario)
                {
                    Subject = "Uno de tus alumnos tiene un justificante",
                    Body = "Se adjunta el justificante del alumno",
                    IsBodyHtml = true
                };
                mensaje.Attachments.Add(new System.Net.Mail.Attachment(pathPDF));

                using (var smtp = new SmtpClient("smtp.office365.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("pruebavinculacion@matehuala.tecnm.mx", "Chingatumadre_123");
                    smtp.EnableSsl = true;
                    smtp.Send(mensaje);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public void EnviarCorreoConPDFDocenteG(string emailDestinatario, string pathPDF)
        {
            try
            {
                var mensaje = new MailMessage("pruebavinculacion@matehuala.tecnm.mx", emailDestinatario)
                {
                    Subject = "Uno de tus grupos tiene un justificante",
                    Body = "Se adjunta el justificante de tu grupo",
                    IsBodyHtml = true
                };
                mensaje.Attachments.Add(new System.Net.Mail.Attachment(pathPDF));

                using (var smtp = new SmtpClient("smtp.office365.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("pruebavinculacion@matehuala.tecnm.mx", "Chingatumadre_123");
                    smtp.EnableSsl = true;
                    smtp.Send(mensaje);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


    }



}
