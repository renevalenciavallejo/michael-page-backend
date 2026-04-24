using Task = MichaelPage.Core.Entities.Task;
using TaskStatus = MichaelPage.Core.Enum.TaskStatus;

namespace MichaelPage.Core.Repositories;

public interface ITaskRepository
{
    Task<int> Create(Task input);
    Task<IEnumerable<Task>> GetAll(TaskStatus? status = null);
    Task<IEnumerable<Task>> GetByUserId(int userId, TaskStatus? status);
    Task<Task> GetById(int id);
    Task<bool> UpdateStatus(int id, TaskStatus newStatus);
}
