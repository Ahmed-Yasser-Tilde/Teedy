using System.Text.Json.Serialization;

namespace Teedy.ApiClient.Models.Tags
{
    public class Tag
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }  // ID of the tag
        [JsonPropertyName("name")]
        public string? Name { get; set; }  // Name of the tag
        [JsonPropertyName("color")]
        public string? Color { get; set; }  // Color of the tag
        [JsonPropertyName("parent")]
        public string? Parent { get; set; }  // Optional parent tag ID
    }

}
