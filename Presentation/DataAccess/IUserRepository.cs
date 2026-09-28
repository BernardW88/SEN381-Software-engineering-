using Models;

namespace DataAccess
{
    public interface IUserRepository
    {
        User GetUserByCredentials(string username, string password);
    }
}