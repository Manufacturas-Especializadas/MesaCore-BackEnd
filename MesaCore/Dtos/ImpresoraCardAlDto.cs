namespace MesaCore.Dtos
{
    public class ImpresoraCardAlDto
    {
        public int Id { get; set; }

        public string NParte { get; set; }

        public string NombreDelProyecto { get; set; }

        public string EstatusNombre { get; set; }

        public string Solicitante { get; set; }

        public string NDibujo { get; set; }

        public string Revision { get; set; }

        public DateTime? FechaSolicitud { get; set; }
    }
}