using FluentValidation;

namespace MichaelPage.Application.Users.Dto;

public class CreateUserDto
{
    public string Name { get; set; }
    public string Email { get; set;}
    
    public void Normalize()
    {
        Name = Name.Trim();
        Email = Email.Trim().ToLower();
    }

    public override string ToString()
    {
        return $"Name: {Name} - Email: {Email}";
    }
}

public class CreateUserValidation : AbstractValidator<CreateUserDto>
{
    public CreateUserValidation()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100).WithName("Name");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(150).WithName("Email");
    }   
}