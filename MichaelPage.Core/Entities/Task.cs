using System.ComponentModel.DataAnnotations;

namespace MichaelPage.Core.Entities;

public class Task
{
    [Key] public int Id { get; set; }
    
    public string Title { get; set; }
    public string Status { get; set; } 
    public string AdditionalInfo { get; set; }
    
    public int UserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public override string ToString()
    {
        return $"Id: {Id} - Title: {Title} - Status: {Status}";
    }
}