using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Models;

/// <summary>
/// Razred koji predstavlja model za dokumentaciju
/// </summary>
public partial class Dokumentacija
{
    /// <summary>
    /// Jedinstveni identifikator dokumentacije
    /// </summary>
    [Key]
    public int DokumentacijaId { get; set; }

    /// <summary>
    /// Naziv dokumentacije
    /// </summary>
    [Required(ErrorMessage = "Naziv dokumentacije je obavezan")]
    public string NazivDokumentacije { get; set; }

    /// <summary>
    /// Vrijeme kreacije dokumentacije
    /// </summary>
    public DateTime VrijemeKreacije { get; set; }

    /// <summary>
    /// Format dokumentacije
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// Url na kojem se dokumentacija nalazi
    /// </summary>
    [Required(ErrorMessage = "URL dokumentacije je obavezan")]
    public string URL { get; set; }

    /// <summary>
    /// Status dovršenosti u kojem se dokumentacija nalazi
    /// </summary>
    public string? StatusDovršenosti { get; set; }

    /// <summary>
    /// Identifikator za projekt sa kojim je dokumentacija povezana
    /// </summary>
    [Required(ErrorMessage = "Potrenbno je odabrati povezani projekt")]
    public int ProjektId { get; set; }

    /// <summary>
    /// Projekt sa kojim je dokumentacija povezana
    /// </summary>
    [Required(ErrorMessage = "Potrenbno je odabrati povezani projekt")]
    public virtual Projekt Projekt { get; set; }

    /// <summary>
    /// Indentifikator za vrstu dokumentacije projekta
    /// </summary>
    public int VrstaDokumentacijeId { get; set; }

    /// <summary>
    /// Vrsta dokumentacije projekta
    /// </summary>
    [Required(ErrorMessage = "Potrenbno je odabrati vrstu dokumentacije")]
    public virtual VrstaDokumentacije VrstaDokumentacije { get; set; }
}