using System;

namespace CNLCI.Models.ViewModel
{
    public class P
    {
        public int No__pase { get; set; }
        public string Matricula { get; set; }
        public string Nombre { get; set; }
        public string Primer_apellido { get; set; }
        public string Segundo_apellido { get; set; }
        public string Plan_de_Estudios { get; set; }
        public string Grupo_Referente { get; set; }
        public string Motivo { get; set; }
        public string Autoriza { get; set; }
        public string Parentezco { get; set; }
        public string Medio_de_Autorizacion { get; set; }
        //  public byte[] Comprobante { get; set; }
        public Nullable<System.DateTime> Fecha { get; set; }

        public string ElaboradoPor { get; set; }
    }
}