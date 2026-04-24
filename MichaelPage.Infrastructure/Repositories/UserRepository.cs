using Dapper;
using MichaelPage.Common.Settings;
using MichaelPage.Core.Entities;
using MichaelPage.Core.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace MichaelPage.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly SqlServerSettings _sqlServerSettings;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(SqlServerSettings sqlServerSettings, ILogger<UserRepository> logger)
    {
        _sqlServerSettings = sqlServerSettings;
        _logger = logger;
    }

    public async Task<int> Create(User user)
    {
        _logger.LogInformation("UserRepository - Create. Input: {Input}", user.ToString());

        // Create a connection to the SQL Server database
        await using var connection = new SqlConnection(_sqlServerSettings.ConnectionString);

        // Define the SQL INSERT statement
        var parameters = new
        {
            user.Name,
            user.Email,
            CreatedAt = DateTimeOffset.UtcNow
        };

        const string sql = @"INSERT INTO michael_page.Users (
                               Name,
                               Email,
                               CreatedAt)
                            VALUES (@Name,
                                    @Email,
                                    @CreatedAt)
                            SELECT CAST(SCOPE_IDENTITY() as int)";

        // Execute the SQL statement and return the newly created user's ID
        return await connection.ExecuteScalarAsync<int>(sql, parameters);
    }

    public async Task<IEnumerable<User>> GetAll()
    {
        _logger.LogInformation("UserRepository - GetAll");

        // Create a connection to the SQL Server database
        await using var connection = new SqlConnection(_sqlServerSettings.ConnectionString);

        // Define the SQL SELECT statement
        const string sql = @"SELECT *
                            FROM michael_page.Users
                            ORDER BY Name, CreatedAt DESC";

        // Execute the SQL SELECT statement
        return await connection.QueryAsync<User>(sql);
    }

    public async Task<User> GetByEmail(string email)
    {
        _logger.LogInformation("UserRepository - GetByEmail. Email: {Email}", email);

        // Create a connection to the SQL Server database
        await using var connection = new SqlConnection(_sqlServerSettings.ConnectionString);

        // Define the SQL SELECT statement
        var parameters = new {Email = email};
        const string sql = @"SELECT *
                            FROM michael_page.Users
                            WHERE Email = @Email";

        // Execute the SQL SELECT statement
        return await connection.QueryFirstOrDefaultAsync<User>(sql, parameters);
    }

    public async Task<User> GetById(int id)
    {
        _logger.LogInformation("UserRepository - GetById. Id: {Id}", id);

        // Create a connection to the SQL Server database
        await using var connection = new SqlConnection(_sqlServerSettings.ConnectionString);

        // Define the SQL SELECT statement
        var parameters = new {UserId = id};
        const string sql = @"SELECT *
                            FROM michael_page.Users
                            WHERE Id = @UserId";

        // Execute the SQL SELECT statement
        return await connection.QueryFirstOrDefaultAsync<User>(sql, parameters);
    }
}
