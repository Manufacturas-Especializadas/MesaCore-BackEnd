﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MesaCore.Models;

public partial class Registrodeimpresorasfx
{
    public int Id { get; set; }

    public int? SolicitanteId { get; set; }

    public DateTime? FechaDeSolicitud { get; set; }

    public string Comentarios { get; set; }

    public string Archivo { get; set; }

    [NotMapped]
    public IFormFile FormFile { get; set; }

    public string NombreDelProyecto { get; set; }

    public virtual Solicitanteimpresorafx Solicitante { get; set; }
}