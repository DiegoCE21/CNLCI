//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CNLCI.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Materias
    {
        public int MateriasID { get; set; }
        public string Clave { get; set; }
        public string Alumno { get; set; }
        public string Grupo { get; set; }
        public string Periodo { get; set; }
        public Nullable<int> DocenteID { get; set; }
        public string RFC { get; set; }
    
        public virtual Docente Docente1 { get; set; }
    }
}
