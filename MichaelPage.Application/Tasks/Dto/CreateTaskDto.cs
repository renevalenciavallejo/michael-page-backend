using FluentValidation;

namespace MichaelPage.Application.Tasks.Dto;

public class CreateTaskDto
{
    public string Title { get; set; }
    public int UserId { get; set; }
    public TaskAdditionalInfoDto AdditionalInfo { get; set; }
    
    public void Normalize()
    {
        Title = Title.Trim();
    }

    public override string ToString()
    {
        return $"Title: {Title} - UserId: {UserId} - AdditionalInfo: {AdditionalInfo}";
    }
}

public class CreateTaskValidation : AbstractValidator<CreateTaskDto>
{
    public CreateTaskValidation()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200).WithName("Title");
        RuleFor(x => x.UserId).GreaterThan(0).WithName("UserId");
    }   
}