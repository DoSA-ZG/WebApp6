using System.Text.Json.Serialization;

namespace RPPP_WebApp.ViewModels
{
    public class IntDoubleLabel
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("opis")]
        public string Opis { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        public IntDoubleLabel() { }
        public IntDoubleLabel(int id, string label, string opis)
        {
            Id = id;
            Label = label;
            Opis = opis;
        }
    }
}
