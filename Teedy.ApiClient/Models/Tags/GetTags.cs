using System.Text.Json.Serialization;

namespace Teedy.ApiClient.Models.Tags
{
    public class GetTags
    {
        [JsonPropertyName("tags")]
        public List<Tag> Tags { get; set; }  // List of tags
    }
}
