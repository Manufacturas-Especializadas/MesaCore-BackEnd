﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace MesaCore.Models;

public partial class Estatusimpresorasfx
{
    public int Id { get; set; }

    public string Nombre { get; set; }

    public virtual ICollection<Impresorascufx> Impresorascufx { get; set; } = new List<Impresorascufx>();

    public virtual ICollection<Impresorasfx> Impresorasfx { get; set; } = new List<Impresorasfx>();
}