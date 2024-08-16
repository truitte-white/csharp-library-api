using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryAPI.Models;

namespace LibraryAPI.Services
{
    public interface IBookService
    {
        Task<Books> FindBookById(int bookId);
        Task<List<Books>> FindAllBooks();
        Task<int> CreateBook(Books newBook);
        Task<int> UpdateBook(int bookId, object updateFields);
        Task<List<Books>> GetAllBooks();
        Task<bool> UpdateBookStatus(int bookId, Books.BookStatus newStatus);
    }
}
