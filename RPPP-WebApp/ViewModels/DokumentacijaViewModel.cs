using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels;

public class DokumentacijaViewModel
{
    public Dokumentacija dokumentacija { get; set; }

    public String SelectedVrstaDokumentacije { get; set; }

    public String SelectedStatusDokumentacije { get; set; }

    public Projekt SelectedProjekt {  get; set; }

}
