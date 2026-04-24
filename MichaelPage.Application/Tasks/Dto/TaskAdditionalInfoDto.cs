namespace MichaelPage.Application.Tasks.Dto;

public class TaskAdditionalInfoDto
{
    public string Priority { get; set; }
    public DateTime EstimatedEndDate { get; set; }
    public List<string> Tags { get; set; }
    public Dictionary<string, object> Metadata { get; set; }

    public override string ToString()
    {
        return $"Priority: {Priority} - EstimatedEndDate: {EstimatedEndDate} - Tags: {string.Join(", ", Tags)}";
    }
}