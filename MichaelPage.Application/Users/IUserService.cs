using MichaelPage.Application.Users.Dto;
using MichaelPage.Common.Models;

namespace MichaelPage.Application.Users;

public interface IUserService
{
    Task<Result<UserDto>> CreateUser(CreateUserDto input);
    Task<Result<List<UserDto>>> GetAllUsers();
    Task<Result<UserDto>> GetUserById(int userId);
}