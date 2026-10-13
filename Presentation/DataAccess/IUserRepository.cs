using Models;

namespace DataAccess
{
    public interface IUserRepository
    {
        UserAuthRecord GetUserAuthRecord(string username);
    }
}