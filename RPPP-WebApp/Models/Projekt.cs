using FluentValidation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Models;

/// <summary>
/// Razred koji predstavlja model za projekt
/// </summary>
public partial class Projekt
{
    /// <summary>
    /// Jedinstveni identifikator projekta
    /// </summary>
    [Key]
    public int ProjektId { get; set; }

    /// <summary>
    /// Ime projekta
    /// </summary>
    [Required(ErrorMessage = "Ime projekta je obavezno")]
    public string Ime { get; set; }

    /// <summary>
    /// Kratica projekta
    /// </summary>
    [Display(Name = "Kratica: ")]
    public string Kratica { get; set; }

    /// <summary>
    /// Opis projekta
    /// </summary>
    public string Opis { get; set; }

    /// <summary>
    /// Datum planiranog početka projekta
    /// </summary>
    [Required(ErrorMessage = "PlaniraniPočetak projekta je obavezan")]
    public DateTime PlaniraniPočetak { get; set; }

    /// <summary>
    /// Datum planiranog završetka projekta
    /// </summary>
    [Required(ErrorMessage = "PlaniraniKraj projekta je obavezan")]
    public DateTime PlaniraniKraj { get; set; }

    /// <summary>
    /// Datum stvarnog početka projekta
    /// </summary>
    public DateTime? StvarniPočetak { get; set; }

    /// <summary>
    /// Datum stvarnog završetka projekta
    /// </summary>
    public DateTime? StvarniKraj { get; set; }

    /// <summary>
    /// Jedinstveni identifikator povezane vrste projekta
    /// </summary>
    [Required(ErrorMessage = "Vrsta projekta je obavezna")]
    public int VrstaProjektaId { get; set; }

    /// <summary>
    /// Dokumentacija koja pripada projekt
    /// </summary>
    public virtual ICollection<Dokumentacija> Dokumentacija { get; set; }

    /// <summary>
    /// Plan projekta koji pripada projektu
    /// </summary>
    public virtual PlanProjekta PlanProjekta { get; set; }

    /// <summary>
    /// Projektne kartice koje pripadaju projektu
    /// </summary>
    public virtual ICollection<ProjektnaKartica> ProjektnaKartica { get; set; } = new List<ProjektnaKartica>();

    /// <summary>
    /// Vrsta projekta
    /// </summary>
    public virtual VrstaProjekta VrstaProjekta { get; set; }

    /// <summary>
    /// Suradnici koji pripadaju projektu
    /// </summary>
    public virtual ICollection<Suradnik> SuradnikEmail { get; set; } = new List<Suradnik>();

}