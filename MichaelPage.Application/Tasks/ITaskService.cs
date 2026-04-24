using MichaelPage.Application.Tasks.Dto;
using MichaelPage.Common.Models;
using TaskStatus = MichaelPage.Core.Enum.TaskStatus;

namespace MichaelPage.Application.Tasks;

public interface ITaskService
{
    Task<Result<TaskDto>> CreateTask(CreateTaskDto input);
    Task<Result<List<TaskDto>>> GetAllTasks(TaskStatus? status = null);
    Task<Result<TaskDto>> GetTaskById(int taskId);
    Task<Result> UpdateTaskStatus(int taskId, TaskStatus newStatus);
}