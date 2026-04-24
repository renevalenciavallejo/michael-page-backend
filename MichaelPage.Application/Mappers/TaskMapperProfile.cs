using System.Text.Json;
using AutoMapper;
using MichaelPage.Application.Tasks.Dto;
using MichaelPage.Core.Entities;
using Task = MichaelPage.Core.Entities.Task;

namespace MichaelPage.Application.Mappers;

public class TaskMapperProfile : Profile
{
    public TaskMapperProfile()
    {
        CreateMap<CreateTaskDto, Task>()
            .ForMember(d => d.AdditionalInfo, o => o.Ignore());
        CreateMap<Task, TaskDto>()
            .ForMember(d => d.AdditionalInfo, o => o.MapFrom((src, _) =>
                src.AdditionalInfo != null
                    ? JsonSerializer.Deserialize<TaskAdditionalInfoDto>(src.AdditionalInfo)
                    : null));
        CreateMap<User, AssignedUserDto>();
    }
}