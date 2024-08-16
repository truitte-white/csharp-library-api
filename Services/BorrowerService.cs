using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryAPI.Helpers;
using LibraryAPI.Models;
using LibraryAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace LibraryAPI.Services
{

    public class BorrowerService : IBorrowerService
    {
        private readonly ILogger<BorrowerService> _logger;
        public readonly IDbHelper _dbHelper;
        public readonly LibraryDbContext _dbContext;

        public BorrowerService(ILogger<BorrowerService> logger, IDbHelper dbHelper, LibraryDbContext dbContext)
        {
            _logger = logger;
            _dbHelper = dbHelper;
            _dbContext = dbContext;
        }

        public async Task<BooksBorrowed?> FindBorrowedBookById(int userId, int bookId)
        {
            try
            {
                var filter = new Dictionary<string, object>
        {
            { "UserId", userId },
            { "BookId", bookId }
        };

                var result = await _dbHelper.FindOne<BooksBorrowed>("booksborrowed", filter);

                if (result == null)
                {
                    _logger.LogWarning("No borrowed book record found for UserId: {UserId}, BookId: {BookId}", userId, bookId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding borrowed book by UserId: {UserId}, BookId: {BookId}", userId, bookId);
                throw; // Optionally, rethrow or handle it as per your needs
            }
        }


        public async Task<List<object>> FindAllBorrowedBooks(int userId)
        {
            return await _dbContext.BooksBorrowed
                .Where(bb => bb.UserId == userId)
                .Include(bb => bb.Book) // Eager load the related Books entity
                .Select(bb => new
                {
                    bb.BookId,
                    BookName = bb.Book.BookName,
                    AuthorName = bb.Book.AuthorName,
                    BorrowDate = bb.BorrowDate,
                    ReturnDate = bb.ReturnDate,
                    BookStatus = bb.Book.Status
                })
                .ToListAsync<object>(); // Return as List<object>
        }
        public async Task<int> BorrowBook(BooksBorrowed body)
        {
            try
            {
                // Add the borrowed book record to the database
                _logger.LogInformation("Adding new borrowed book record: {@Body}", body);
                _dbContext.BooksBorrowed.Add(body);
                await _dbContext.SaveChangesAsync();
                return body.BorrowId; // Assuming BorrowId is the primary key
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error borrowing book: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<int> UpdateBookStatus(int bookId, Books.BookStatus newStatus) 
        {
            var filter = new Dictionary<string, object> { { "BookId", bookId } };
            var updatedFields = new { BookStatus = newStatus };
            return await _dbHelper.Update<Books>("books", updatedFields, filter); 
        }

        public async Task<int> UpdateBorrowerBook(int userId, int bookId, object updateFields)
        {
            var filter = new Dictionary<string, object> { { "UserId", userId }, { "BookId", bookId } };
            return await _dbHelper.Update<BooksBorrowed>("booksborrowed", updateFields, filter);
        }
    }
}
