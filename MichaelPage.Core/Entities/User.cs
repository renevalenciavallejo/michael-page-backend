using System.ComponentModel.DataAnnotations;

namespace MichaelPage.Core.Entities;

public class User
{
    [Key] public int Id { get; set; }
    
    public string Name { get; set; }
    public string Email { get; set; }
    public ICollection<Task> Tasks { get; set; } 
    
    public DateTimeOffset CreatedAt { get; set; }
}