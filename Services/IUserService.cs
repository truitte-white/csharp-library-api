using System.Threading.Tasks;
using LibraryAPI.Models;

namespace LibraryAPI.Services
{
    public interface IUserService
    {
        Task<Users> FindUserByEmail(string email);
        Task<Users> FindUserById(int userId);
        Task<int> CreateUser(Users userData);
        Task<int> UpdateUser(Users updatedBody);
        Task RehashPasswords();
    }
}
