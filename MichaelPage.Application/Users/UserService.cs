using AutoMapper;
using MichaelPage.Application.Users.Dto;
using MichaelPage.Common.Models;
using MichaelPage.Core.Entities;
using MichaelPage.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace MichaelPage.Application.Users;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger, IMapper mapper)
    {
        _userRepository = userRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> CreateUser(CreateUserDto input)
    {
        input.Normalize();
        _logger.LogInformation("UserService - CreateUser. Attempting to create user. {@Input}", input.ToString());

        // Check if the email already exists
        var existing = await _userRepository.GetByEmail(input.Email);
        if (existing is not null)
        {
            _logger.LogWarning("UserService - CreateUser. Email already in use. {@Input}", input.ToString());
            return Result.Fail<UserDto>("Email already in use.");
        }

        // Map the input DTO to a User entity
        var user = _mapper.Map<User>(input);

        // Create the user using the repository
        var newId = await _userRepository.Create(user);

        _logger.LogInformation("UserService - CreateUser. User created successfully. UserId: {@UserId}", newId);
        return await GetUserById(newId);
    }
    
    public async Task<Result<List<UserDto>>> GetAllUsers()
    {
        _logger.LogInformation("UserService - GetAllUsers. Fetching users.");

        // Get all users from the repository
        var users = await _userRepository.GetAll();

        // Map the users to DTOs
        var usersDto = _mapper.Map<List<UserDto>>(users);

        _logger.LogInformation("UserService - GetAllUsers. Returned {Count} users.", usersDto.Count);
        return Result.Ok(usersDto);
    }

    public async Task<Result<UserDto>> GetUserById(int userId)
    {
        _logger.LogInformation("UserService - GetUserById. Fetching user. UserId: {@UserId}", userId);

        // Get the user by ID from the repository
        var user = await _userRepository.GetById(userId);

        // Check if the user was found
        if (user is not null)
            return Result.Ok(_mapper.Map<UserDto>(user));

        _logger.LogWarning("UserService - GetUserById. User not found. UserId: {@UserId}", userId);
        return Result.Fail<UserDto>("User not found.");
    }
}
