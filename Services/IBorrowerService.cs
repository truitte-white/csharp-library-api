using LibraryAPI.Models;

namespace LibraryAPI.Services
{
    public interface IBorrowerService
    {
        Task<BooksBorrowed?> FindBorrowedBookById(int userId, int bookId);
        Task<List<object>> FindAllBorrowedBooks(int userId);
        Task<int> BorrowBook(BooksBorrowed body);
        Task<int> UpdateBookStatus(int bookId, Books.BookStatus newStatus);
        Task<int> UpdateBorrowerBook(int userId, int bookId, object updateFields);
    }
}
