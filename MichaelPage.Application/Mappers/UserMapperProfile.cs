using AutoMapper;
using MichaelPage.Application.Users.Dto;
using MichaelPage.Core.Entities;

namespace MichaelPage.Application.Mappers;

public class UserMapperProfile : Profile
{
    public UserMapperProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<CreateUserDto, User>();
    }
}