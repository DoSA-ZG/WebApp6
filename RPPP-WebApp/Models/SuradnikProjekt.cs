namespace RPPP_WebApp.Models;

public class SuradnikProjekt
{

    public int ProjektId { get; set; }

    public string SuradnikEmail { get; set; }

    public Projekt Projekt { get; set; }

    public Suradnik Suradnik { get; set; }

}
