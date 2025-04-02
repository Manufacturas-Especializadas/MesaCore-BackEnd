﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace MesaCore.Models;

public partial class Impresorascufx
{
    public int Id { get; set; }

    public string Codigo { get; set; }

    public int? PlantaId { get; set; }

    public int? SolicitanteId { get; set; }

    public int? ClienteId { get; set; }

    public string NDibujo { get; set; }

    public string NParte { get; set; }

    public string Revision { get; set; }

    public DateTime? EntregaLaboratorio { get; set; }

    public int? Fai { get; set; }

    public DateTime? LiberacionLaboratorio { get; set; }

    public string Comentarios { get; set; }

    public int? EstatusId { get; set; }

    public string NombreDelProyecto { get; set; }

    public DateTime? FechaDeLaSolicitud { get; set; }

    public int? EstatusProyectoId { get; set; }

    public virtual Clienteimpresorasfx Cliente { get; set; }

    public virtual Estatusimpresorasfx Estatus { get; set; }

    public virtual Estatusproyectoimpresorasfx EstatusProyecto { get; set; }

    public virtual ICollection<Impresionesestadisticas> Impresionesestadisticas { get; set; } = new List<Impresionesestadisticas>();

    public virtual Plantaimpresorasfx Planta { get; set; }

    public virtual Solicitanteimpresorafx Solicitante { get; set; }
}