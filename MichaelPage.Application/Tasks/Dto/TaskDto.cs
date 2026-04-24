namespace MichaelPage.Application.Tasks.Dto;

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public AssignedUserDto AssignedUser { get; set; }
    public TaskAdditionalInfoDto AdditionalInfo { get; set; }
}