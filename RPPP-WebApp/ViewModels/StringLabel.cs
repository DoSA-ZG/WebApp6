using System.Text.Json.Serialization;

namespace RPPP_WebApp.ViewModels
{
    public class StringLabel
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }
        public StringLabel() { }
        public StringLabel(string id, string label)
        {
            Id = id;
            Label = label;
        }
    }
}
