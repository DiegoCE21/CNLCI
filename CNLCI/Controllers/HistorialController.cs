using CNLCI.Models;
using CNLCI.Models.ViewModel;
using Rotativa;
using System.Data;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web.Mvc;
using System.Security.Claims; // Asegúrate de incluir esta directiva en la parte superior
using System;
using Rotativa.Options;
using System.Collections.Generic;

namespace CNLCI.Controllers
{
    [Authorize]
    public class HistorialController : Controller
    {
        public ActionResult PaseS()
        {
            using (DB db = new DB())
            {
                // Obtener el correo del usuario autenticado
                var identity = User.Identity as ClaimsIdentity; // Convertir la identidad del usuario a ClaimsIdentity
                var emailClaim = identity?.FindFirst(ClaimTypes.Email); // Buscar el claim de email
                var correoUsuario = emailClaim?.Value; // Obtener el valor del claim de email


                // Extraer la matrícula del correo del usuario
                var inicioMatricula = 1; // Comenzar después del primer carácter 'l'
                var finMatricula = correoUsuario.IndexOf("@matehuala.tecnm.mx"); // Encontrar la posición del dominio
                var matriculaUsuario = correoUsuario.Substring(inicioMatricula, finMatricula - inicioMatricula);

                // Buscar el usuario por correo para obtener su tipo de acceso
                var usuario = db.Usuarios.FirstOrDefault(u => u.Correo == correoUsuario);

                // Verificar si el usuario es un alumno
                if (usuario != null && usuario.tipoAcceso == "Alumno")
                {
                    // Si es alumno, filtrar los justificantes por su matrícula
                    var Datos = db.Pase_de_salida
                        .Where(j => j.Matricula == matriculaUsuario)
                        .OrderByDescending(j => j.No__pase)
                        .ToList();

                    return View(Datos);
                }
                else
                {
                    var Datos = db.Pase_de_salida.OrderByDescending(j => j.No__pase).ToList();

                    return View(Datos);
                }
            }
        }
        /*public ActionResult Justificante()
        {
            using (DB db = new DB())
            {
                var Datos = db.Justificante.OrderByDescending(j => j.No__justificante).ToList();

                return View(Datos);
            }

        }*/
        public ActionResult Justificante()
        {
            using (DB db = new DB())
            {
                // Obtener el correo del usuario autenticado
                var identity = User.Identity as ClaimsIdentity; // Convertir la identidad del usuario a ClaimsIdentity
                var emailClaim = identity?.FindFirst(ClaimTypes.Email); // Buscar el claim de email
                var correoUsuario = emailClaim?.Value; // Obtener el valor del claim de email


                // Extraer la matrícula del correo del usuario
                var inicioMatricula = 1; // Comenzar después del primer carácter 'l'
                var finMatricula = correoUsuario.IndexOf("@matehuala.tecnm.mx"); // Encontrar la posición del dominio
                var matriculaUsuario = correoUsuario.Substring(inicioMatricula, finMatricula - inicioMatricula);

                // Buscar el usuario por correo para obtener su tipo de acceso
                var usuario = db.Usuarios.FirstOrDefault(u => u.Correo == correoUsuario);

                // Verificar si el usuario es un alumno
                /*if (usuario != null && usuario.tipoAcceso == "Alumno")
                {
                    // Consulta original para obtener los justificantes
                    var Datos = db.Justificante
                        .Where(j => j.Matricula == matriculaUsuario)
                        .OrderByDescending(j => j.No__justificante)
                        .ToList();

                    // Diccionario para almacenar la información de los docentes que no han visto los justificantes
                    var docentesNoVistos = new Dictionary<int, List<int>>();

                    foreach (var justificante in Datos)
                    {
                        var docentes = db.JustificantesVistos
                            .Where(jv => jv.JustificanteID == justificante.No__justificante && jv.Visto == false && jv.FechaVisto == null)
                            .Select(jv => jv.DocenteID)
                            .ToList();

                        if (docentes.Any())
                        {
                            docentesNoVistos.Add(justificante.No__justificante, docentes);
                        }
                    }

                    // Pasar la información a la vista mediante ViewBag
                    ViewBag.DocentesNoVistos = docentesNoVistos;

                    return View(Datos);
                }*/
                if (usuario != null && usuario.tipoAcceso == "Alumno")
                {
                    var Datos = db.Justificante
                        .Where(j => j.Matricula == matriculaUsuario)
                        .OrderByDescending(j => j.No__justificante)
                        .ToList();

                    var docentesNoVistosNombres = new Dictionary<int, List<string>>();

                    foreach (var justificante in Datos)
                    {
                        var docentesNombres = db.JustificantesVistos
                            .Where(jv => jv.JustificanteID == justificante.No__justificante && jv.Visto == false && jv.FechaVisto == null)
                            .Join(db.Docente, // Tabla a unir
                                  jv => jv.DocenteID, // Clave foránea de JustificantesVistos
                                  d => d.DocenteID, // Clave primaria de Docente
                                  (jv, d) => new { d.nombre }) // Selector para obtener el nombre
                            .Select(n => n.nombre)
                            .ToList();

                        if (docentesNombres.Any())
                        {
                            docentesNoVistosNombres.Add(justificante.No__justificante, docentesNombres);
                        }
                    }

                    ViewBag.DocentesNoVistosNombres = docentesNoVistosNombres;

                    return View(Datos);
                }



                else if (usuario != null && usuario.tipoAcceso == "Docente")
                {
                    var docenteId = db.Docente
                                      .Where(d => d.CorreoDocente == correoUsuario)
                                      .Select(d => d.DocenteID)
                                      .FirstOrDefault();

                    var gruposDelDocente = db.Materias
                                             .Where(m => m.DocenteID == docenteId)
                                             .Select(m => m.Grupo)
                                             .Distinct()
                                             .ToList();

                    var justificantes = db.Justificante
                                          .Where(j => gruposDelDocente.Contains(j.Grupo_Referente))
                                          .ToList();

                    var justificantesVistos = db.JustificantesVistos
                            .Where(jv => jv.DocenteID == docenteId && (jv.Visto ?? false))
                            .ToList();
                    var justificantesRequierenAtencion = new Dictionary<int, bool>();

                    foreach (var justificante in justificantes)
                    {
                        var visto = justificantesVistos.FirstOrDefault(jv => jv.JustificanteID == justificante.No__justificante);
                        // Si 'visto' es null, significa que el justificante no ha sido visto.
                        // Si 'visto' no es null pero 'FechaVisto' es null y 'Visto' es false (equivalente a 0 en tu base de datos), también requiere atención.
                        bool requiereAtencion = visto == null || (visto.FechaVisto == null && visto.Visto == false);
                        justificantesRequierenAtencion.Add(justificante.No__justificante, requiereAtencion);
                    }

                    ViewBag.JustificantesRequierenAtencion = justificantesRequierenAtencion;

                    return View(justificantes);
                }



                else
                {
                    // Si no es alumno, o no se encuentra el usuario, mostrar todos los justificantes
                    var Datos = db.Justificante.OrderByDescending(j => j.No__justificante).ToList();
                    return View(Datos);
                }
            }
        }

        public ActionResult GenerarPDFP(int id)
        {

            using (DB db = new DB())
            {
                Pase_de_salida modelo = db.Pase_de_salida.OrderByDescending(x => x.No__pase == id).FirstOrDefault();
                P modelof = new P
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

                return new ViewAsPdf("PDFPase", modelof)
                {
                    PageSize = Rotativa.Options.Size.Letter,
                    PageMargins = new Rotativa.Options.Margins(13, 13, 13, 13)
                };
            }

        }




        /*public ActionResult GenerarPDFJ(int id)
        {

            using (DB db = new DB())
            {

                Justificante modelo = db.Justificante.SingleOrDefault(i => i.No__justificante == id);
                J modelof = new J
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

                return new ViewAsPdf("PDFJustificante", modelof)
                {
                    PageSize = Rotativa.Options.Size.Letter,
                    PageMargins = new Rotativa.Options.Margins(13, 13, 13, 13)
                };
            }

        }*/
        public ActionResult GenerarPDFJ(int id)
        {
            using (DB db = new DB()) // Asegúrate de que 'DB' es el nombre correcto de tu contexto de base de datos
            {
                // Obtener el justificante por ID
                Justificante modelo = db.Justificante.SingleOrDefault(i => i.No__justificante == id);
                if (modelo == null)
                {
                    // Manejar el caso en que no se encuentre el justificante
                    return HttpNotFound();
                }

                // Crear el modelo para la vista PDF
                J modelof = new J
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

                var identity = User.Identity as ClaimsIdentity;
                var emailClaim = identity?.FindFirst(ClaimTypes.Email);
                var correoUsuario = emailClaim?.Value;

                // Buscar el docente basado en el correo
                var usuario = db.Usuarios.FirstOrDefault(u => u.Correo == correoUsuario);

                // Verificar si el usuario es un alumno
                if (usuario != null && usuario.tipoAcceso == "Docente")
                {
                    var docente = db.Docente.SingleOrDefault(d => d.CorreoDocente == correoUsuario);
                    if (docente != null)
                    {
                        // Obtener todos los registros que coincidan con el JustificanteID y DocenteID y que no han sido marcados como vistos
                        var vistas = db.JustificantesVistos
                                        .Where(v => v.JustificanteID == id &&
                                                    v.DocenteID == docente.DocenteID &&
                                                    v.FechaVisto == null &&
                                                    v.Visto == false)
                                        .ToList();

                        // Verificar si hay registros existentes para actualizar
                        if (vistas.Any())
                        {
                            foreach (var vista in vistas)
                            {
                                // Actualizar la fecha y marcar como visto
                                vista.FechaVisto = DateTime.Now;
                                vista.Visto = true;
                            }

                            db.SaveChanges();
                        }
                    }
                }

                // Retornar la vista PDF
                return new ViewAsPdf("PDFJustificante", modelof)
                {
                    PageSize = Size.Letter,
                    PageMargins = new Margins(13, 13, 13, 13)
                };
            }
        }


        public ActionResult buscadorJ(string matricula)
        {

            using (DB db = new DB())
            {

                var resultado = db.Justificante
    .Where(j => j.Matricula.StartsWith(matricula))
    .Select(j => new
    {
        j.No__justificante,
        j.Matricula,
        j.Grupo_Referente,
        j.Motivo,
        Fecha_del_dia = SqlFunctions.DatePart("dd", j.Fecha_del_dia) + "/" + SqlFunctions.DatePart("mm", j.Fecha_del_dia) + "/" + SqlFunctions.DatePart("yyyy", j.Fecha_del_dia),
        Fecha_al_dia = SqlFunctions.DatePart("dd", j.Fecha_al_dia) + "/" + SqlFunctions.DatePart("mm", j.Fecha_al_dia) + "/" + SqlFunctions.DatePart("yyyy", j.Fecha_al_dia)

    })
    .OrderByDescending(x => x.No__justificante)
    .ToList();



                return Json(resultado, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult buscadorP(string matricula)
        {

            using (DB db = new DB())
            {

                var resultado = db.Pase_de_salida
    .Where(j => j.Matricula.StartsWith(matricula))
    .Select(j => new
    {
        j.No__pase,
        j.Matricula,
        j.Grupo_Referente,
        j.Motivo,
        Fecha = SqlFunctions.DatePart("dd", j.Fecha) + "/" + SqlFunctions.DatePart("mm", j.Fecha) + "/" + SqlFunctions.DatePart("yyyy", j.Fecha),

    })
    .OrderByDescending(x => x.No__pase)
    .ToList();



                return Json(resultado, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult p(int id)
        {

            using (DB db = new DB())
            {
                var pase = db.Pase_de_salida.FirstOrDefault(j => j.No__pase == id);

                if (pase != null && pase.Comprobante != null && pase.Comprobante.Length > 0)
                {
                    return File(pase.Comprobante, "application/octet-stream", "Comprobande de pase de salida.PDF");
                }
                else
                {
                    return HttpNotFound("El archivo no existe.");
                }
            }
        }
        public ActionResult j(int id)
        {

            using (DB db = new DB())
            {
                var Justificante = db.Justificante.FirstOrDefault(j => j.No__justificante == id);

                if (Justificante != null && Justificante.Comprobante != null && Justificante.Comprobante.Length > 0)
                {
                    return File(Justificante.Comprobante, "application/octet-stream", "Comprobande de justificante.PDF");
                }
                else
                {
                    return HttpNotFound("El archivo no existe.");
                }



            }
        }

    }
}

