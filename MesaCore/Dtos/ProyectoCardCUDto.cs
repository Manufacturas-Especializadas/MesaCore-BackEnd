namespace MesaCore.Dtos
{
    public class ProyectoCardCUDto
    {
        public int Id { get; set; }

        public string NombreDelProyecto { get; set; }

        public DateTime? FechaSolicitud { get; set; }

        public string SolicitanteNombre { get; set; }

        public string PlantaNombre { get; set; }

        public string EstatusNombre { get; set; }

        public List<ImpresoraCardCUDto> Impresiones { get; set; }
    }
}