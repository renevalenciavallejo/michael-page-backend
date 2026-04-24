using MichaelPage.Core.Entities;

namespace MichaelPage.Core.Repositories;

public interface IUserRepository
{
    Task<int> Create(User user);
    Task<IEnumerable<User>> GetAll();
    Task<User> GetByEmail(string email);
    Task<User> GetById(int id);
}
