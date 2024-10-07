namespace RPPP_WebApp
{
    /// <summary>
    /// Razred za općenite postavke aplikacije
    /// </summary>
    public class AppSettings
    {
        public int PageSize { get; set; } = 10;
        public int PageOffset { get; set; } = 10;

        public int AutoCompleteCount { get; set; } = 50;

    }
}
