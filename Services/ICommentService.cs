using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryAPI.Models;

namespace LibraryAPI.Services
{
    public interface ICommentService
    {
        Task<int> CreateCommentAsync(BooksComments comment);
        Task<bool> UpdateCommentAsync(int commentId, BooksComments comment);
        Task<bool> DeleteCommentAsync(int commentId, int userId);
        Task<BooksComments?> GetCommentByIdAsync(int commentId);
        Task<List<BooksComments>> GetLatestCommentsAsync();
        Task<List<BooksComments>> GetCommentsByUserIdAsync(int userId);
    }
}
