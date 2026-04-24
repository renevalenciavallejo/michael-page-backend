namespace MichaelPage.Application.Users.Dto;

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}