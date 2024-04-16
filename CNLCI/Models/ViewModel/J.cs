using System;

namespace CNLCI.Models.ViewModel
{
    public class J
    {
        public int No__justificante { get; set; }
        public string Matricula { get; set; }
        public string Nombre { get; set; }
        public string Primer_apellido { get; set; }
        public string Segundo_apellido { get; set; }
        public string Plan_de_Estudios { get; set; }
        public string Grupo_Referente { get; set; }
        public string Motivo { get; set; }
        public DateTime? Fecha_del_dia { get; set; }
        public DateTime? Fecha_al_dia { get; set; }

        public string ElaboradoPor { get; set; }
    }
}