using Dapper;
using MichaelPage.Common.Settings;
using MichaelPage.Core.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Task = MichaelPage.Core.Entities.Task;
using TaskStatus = MichaelPage.Core.Enum.TaskStatus;

namespace MichaelPage.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly SqlServerSettings _sqlServerSettings;
    private readonly ILogger<TaskRepository> _logger;

    public TaskRepository(SqlServerSettings sqlServerSettings, ILogger<TaskRepository> logger)
    {
        _sqlServerSettings = sqlServerSettings;
        _logger = logger;
    }
    
    public async Task<int> Create(Task input)
    {
        _logger.LogInformation("TaskRepository - Create. Input: {Input}", input.ToString()); 
        
        // Create a connection to the SQL Server database
        await using var connection = new SqlConnection(_sqlServerSettings.ConnectionString);
        
        // Define the SQL INSERT statement
        var parameters = new
        {
            input.Title,
            input.Status,
            input.AdditionalInfo,
            input.UserId,
            CreatedAt = DateTimeOffset.UtcNow
        };
        
        const string sql = @"INSERT INTO michael_page.Tasks (
                               Title,
                               Status,
                               AdditionalInfo,
                               UserId,
                               CreatedAt)
                            VALUES (@Title,
                                    @Status,
                                    @AdditionalInfo,
                                    @UserId,
                                    @CreatedAt)
                            SELECT CAST(SCOPE_IDENTITY() as int)";

        // Execute the SQL statement and return the newly created task's ID
        return await connection.ExecuteScalarAsync<int>(sql, parameters);
    }

    public async Task<IEnumerable<Task>> GetAll(TaskStatus? status = null)
    {
        _logger.LogInformation("TaskRepository - GetAll. Status: {Status}", status);

        // Create a connection to the SQL Server database
        await using var connection = new SqlConnection(_sqlServerSettings.ConnectionString);

        // Define the SQL SELECT statement
        var parameters = new
        {
            Status = status?.ToString()
        };

        const string sql = @"SELECT *
                            FROM michael_page.Tasks
                            WHERE (@Status IS NULL OR Status = @Status)
                            ORDER BY CreatedAt DESC, Id DESC";

        // Execute the SQL SELECT statement
        return (await connection.QueryAsync<Task>(sql, parameters)).ToList();
    }

    public async Task<IEnumerable<Task>> GetByUserId(int userId, TaskStatus? status)
    {
        _logger.LogInformation("TaskRepository - GetByUserId. UserId: {UserId} - Status: {Status}", userId, status);

        // Create a connection to the SQL Server database
        await using var connection = new SqlConnection(_sqlServerSettings.ConnectionString);

        // Define the SQL SELECT statement
        var parameters = new
        {
            UserId = userId,
            Status = status?.ToString()
        };

        const string sql = @"SELECT *
                            FROM michael_page.Tasks
                            WHERE UserId = @UserId
                              AND (@Status IS NULL OR Status = @Status)
                            ORDER BY CreatedAt DESC, Id DESC";

        // Execute the SQL SELECT statement
        return await connection.QueryAsync<Task>(sql, parameters);
    }

    public async Task<Task> GetById(int id)
    {
        _logger.LogInformation("TaskRepository - GetById. Id: {Id}", id);

        // Create a connection to the SQL Server database
        await using var connection = new SqlConnection(_sqlServerSettings.ConnectionString);

        // Define the SQL SELECT statement
        var parameters = new {TaskId = id};
        const string sql = @"SELECT *
                            FROM michael_page.Tasks
                            WHERE Id = @TaskId";

        // Execute the SQL SELECT statement
        return await connection.QueryFirstOrDefaultAsync<Task>(sql, parameters);
    }

    public async Task<bool> UpdateStatus(int id, TaskStatus newStatus)
    {
        _logger.LogInformation("TaskRepository - UpdateStatus. Id: {Id} - NewStatus: {NewStatus}", id, newStatus);

        // Create a connection to the SQL Server database
        await using var connection = new SqlConnection(_sqlServerSettings.ConnectionString);

        // Define the SQL UPDATE statement
        var parameters = new
        {
            TaskId = id,
            Status = newStatus.ToString()
        };

        const string sql = @"UPDATE michael_page.Tasks
                            SET Status = @Status
                            WHERE Id = @TaskId";

        // Execute the SQL UPDATE statement and return whether a row was affected
        var affected = await connection.ExecuteAsync(sql, parameters);
        return affected > 0;
    }
}