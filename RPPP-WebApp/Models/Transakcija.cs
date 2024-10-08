﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;


/// <summary>
/// Razred koji predstavlja model za transakciju
/// </summary>
public partial class Transakcija
{
    /// <summary>
    /// Jedinstveni identifikator transakcije
    /// </summary>
    public int TransakcijaId { get; set; }

    /// <summary>
    /// Iban isporučitelja
    /// </summary>
    public int IbanIsporučitelja { get; set; }

    /// <summary>
    /// Iban primatelja
    /// </summary>
    public int IbanPrimatelja { get; set; }

    /// <summary>
    /// Iznos transakcije
    /// </summary>
    public double Iznos { get; set; }

    /// <summary>
    /// Ukoliko postoji, identifikator projektne kartice isporučitelja
    /// </summary>
    public int? ProjektnaKarticaIsporučiteljId { get; set; }

    /// <summary>
    /// Ukoliko postoji, identifikator projektne kartice primatelja
    /// </summary>
    public int? ProjektnaKarticaPrimateljId { get; set; }

    /// <summary>
    /// Identifikator vrste transakcije
    /// </summary>
    public int VrstaTransakcijeId { get; set; }

    /// <summary>
    /// Projektna kartica isporučitelja
    /// </summary>
    public virtual ProjektnaKartica ProjektnaKarticaIsporučitelj { get; set; }

    /// <summary>
    /// Projektna kartica primatelja
    /// </summary>
    public virtual ProjektnaKartica ProjektnaKarticaPrimatelj { get; set; }

    /// <summary>
    /// Vrsta transakcije
    /// </summary>
    public virtual VrstaTransakcije VrstaTransakcije { get; set; }
}