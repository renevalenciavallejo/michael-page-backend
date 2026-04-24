using System.Text.Json.Serialization;

namespace MichaelPage.Core.Entities;

public class TaskAdditionalInfo
{
    [JsonPropertyName("priority")]
    public string Priority { get; set; }  
 
    [JsonPropertyName("estimatedEndDate")]
    public DateTime? EstimatedEndDate { get; set; }
 
    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; }
 
    [JsonPropertyName("metadata")]
    public Dictionary<string, object> Metadata { get; set; }
}