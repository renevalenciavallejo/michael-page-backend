using System.Text.Json;
using AutoMapper;
using MichaelPage.Application.Tasks.Dto;
using MichaelPage.Common.Models;
using MichaelPage.Core.Repositories;
using Microsoft.Extensions.Logging;
using Task = MichaelPage.Core.Entities.Task;
using TaskStatus = MichaelPage.Core.Enum.TaskStatus;

namespace MichaelPage.Application.Tasks;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<TaskService> _logger;
    private readonly IMapper _mapper;
    
    public TaskService(ITaskRepository taskRepository, IUserRepository userRepository, ILogger<TaskService> logger,
        IMapper mapper)
    {
        _taskRepository = taskRepository;
        _userRepository = userRepository;
        _logger = logger;
        _mapper = mapper;
    }
    
    public async Task<Result<TaskDto>> CreateTask(CreateTaskDto input)
    {
        input.Normalize();
        
        _logger.LogInformation("TaskService - CreateTask. Attempting to create task. {@Input}", input.ToString());
        
        // Check if the assigned user exists
        var user = await _userRepository.GetById(input.UserId);
        if (user is null)
        {
            _logger.LogWarning("TaskService - CreateTask. Assigned user not found. {@Input}", input.ToString());
            return Result.Fail<TaskDto>("Assigned user not found.");
        }
 
        // Map the input DTO to a Task entity
        var task = _mapper.Map<Task>(input);
        
        // Set the task status to pending
        task.Status = TaskStatus.Pending.ToString();
 
        // Serialize additional info if provided
        if (input.AdditionalInfo is not null)
            task.AdditionalInfo = JsonSerializer.Serialize(input.AdditionalInfo);
 
        // Create the task using the repository
        var newId = await _taskRepository.Create(task);
        
        _logger.LogInformation("TaskService - CreateTask. Task created successfully. TaskId: {@TaskId}", newId);
        return await GetTaskById(newId);
    }
    
    public async Task<Result<List<TaskDto>>> GetAllTasks(TaskStatus? status = null)
    {
        _logger.LogInformation("TaskService - GetAllTasks. Fetching tasks. Status: {@Status}", status);
        
        // Get all tasks from the repository
        var tasks = (await _taskRepository.GetAll(status)).ToList();
        
        // Map the tasks to DTOs
        var tasksDto = _mapper.Map<List<TaskDto>>(tasks);

        // Get unique user IDs from the tasks
        var userIds = tasks.Select(t => t.UserId).Distinct().ToList();
        
        // Get all users from the repository
        var users = (await _userRepository.GetAll())
            .Where(u => userIds.Contains(u.Id))
            .ToDictionary(u => u.Id);

        // For each task, get the assigned user and add it to the DTO
        for (var i = 0; i < tasks.Count; i++)
        {
            if (users.TryGetValue(tasks[i].UserId, out var user))
                tasksDto[i].AssignedUser = _mapper.Map<AssignedUserDto>(user);
        }

        _logger.LogInformation("TaskService - GetAllTasks. Returned {Count} tasks.", tasksDto.Count);
        return Result.Ok(tasksDto);
    }

    public async Task<Result<TaskDto>> GetTaskById(int taskId)
    {
        _logger.LogInformation("TaskService - GetTaskById. Fetching task. TaskId: {@TaskId}", taskId);
 
        // Get the task by ID from the repository
        var task = await _taskRepository.GetById(taskId);

        // Check if the task was found
        if (task is null)
        {
            _logger.LogWarning("TaskService - GetTaskById. Task not found. TaskId: {@TaskId}", taskId);
            return Result.Fail<TaskDto>("Task not found.");
        }

        // Map the task to a DTO
        var dto = _mapper.Map<TaskDto>(task);

        // Get the assigned user for the task
        var user = await _userRepository.GetById(task.UserId);
        if (user is not null)
            dto.AssignedUser = _mapper.Map<AssignedUserDto>(user);

        return Result.Ok(dto);
    }

    public async Task<Result> UpdateTaskStatus(int taskId, TaskStatus newStatus)
    {
        _logger.LogInformation(
            "TaskService - UpdateTaskStatus. Attempting status update. TaskId: {@TaskId} - NewStatus: {@NewStatus}",
            taskId, newStatus);
 
        // Get the task by ID from the repository
        var task = await _taskRepository.GetById(taskId);
        
        // Check if the task was found
        if (task is null)
        {
            _logger.LogWarning("TaskService - UpdateTaskStatus. Task not found. TaskId: {@TaskId}", taskId);
            return Result.Fail("Task not found.");
        }

        // Validate the status transition
        if (task.Status == TaskStatus.Pending.ToString() && newStatus == TaskStatus.Done)
        {
            _logger.LogWarning("TaskService - UpdateTaskStatus. Invalid transition. TaskId: {@TaskId} - " +
                               "CurrentStatus: {@CurrentStatus} - NewStatus: {@NewStatus}", taskId, task.Status,
                newStatus);

            return Result.Fail($"Cannot transition from '{task.Status}' to '{newStatus}'");
        }
 
        // Update the task status
        await _taskRepository.UpdateStatus(taskId, newStatus);

        _logger.LogInformation(
            "TaskService - UpdateTaskStatus. Status updated successfully. TaskId: {@TaskId} - NewStatus: {@NewStatus}",
            taskId, newStatus);
        return Result.Ok(message: "Task status updated successfully.");
    }
}