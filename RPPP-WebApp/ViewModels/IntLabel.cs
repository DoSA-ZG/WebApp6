using System.Text.Json.Serialization;

namespace RPPP_WebApp.ViewModels
{
    public class IntLabel
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }
        public IntLabel() { }
        public IntLabel(int id, string label)
        {
            Id = id;
            Label = label;
        }
    }
}
