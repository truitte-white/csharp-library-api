using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryAPI.Data;
using LibraryAPI.Helpers;
using LibraryAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LibraryAPI.Services
{
    public class BookService : IBookService
    {
        private readonly ILogger<BookService> _logger;
        private readonly IDbHelper _dbHelper;
        private readonly LibraryDbContext _context;

        public BookService(ILogger<BookService> logger, IDbHelper dbHelper, LibraryDbContext context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Books> FindBookById(int bookId)
        {
            try
            {
                var filter = new Dictionary<string, object> { { "BookId", bookId } };
                return await _dbHelper.FindOne<Books>("books", filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while finding the book by ID: {BookId}", bookId);
                throw; // Re-throw the exception to propagate it
            }
        }

        public async Task<List<Books>> FindAllBooks()
        {
            try
            {
                return await _dbHelper.FindAll<Books>("books");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all books.");
                throw; // Re-throw the exception to propagate it
            }
        }

        public async Task<int> CreateBook(Books newBook)
        {
            try
            {
                // Check for existing book with the same name and author
                var existingBook = await _context.Books
                    .FirstOrDefaultAsync(b => b.BookName == newBook.BookName && b.AuthorName == newBook.AuthorName);

                if (existingBook != null)
                {
                    throw new Exception("A book with this name and author already exists.");
                }

                _context.Books.Add(newBook);
                await _context.SaveChangesAsync();

                return newBook.BookId; // Return the generated BookId
            }
            catch (DbUpdateException dbEx)
            {
                // Log database update exceptions
                _logger.LogError(dbEx, "Database update error occurred while creating a book.");
                throw; // Rethrow the exception to be handled by the caller
            }
            catch (Exception ex)
            {
                // Log general exceptions
                _logger.LogError(ex, "Error occurred while creating a book.");
                throw; // Rethrow the exception to be handled by the caller
            }
        }

        public async Task<int> UpdateBook(int bookId, object updateFields)
        {
            try
            {
                var filter = new Dictionary<string, object> { { "bookId", bookId } };
                return await _dbHelper.Update<Books>("books", updateFields, filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating the book with ID: {BookId}", bookId);
                throw; // Re-throw the exception to propagate it
            }
        }

        public async Task<List<Books>> GetAllBooks()
        {
            try
            {
                return await _dbHelper.FindAll<Books>("books");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all books.");
                throw; // Re-throw the exception to propagate it
            }
        }

        public async Task<bool> UpdateBookStatus(int bookId, Books.BookStatus newStatus)
        {
            var filter = new Dictionary<string, object> { { "BookId", bookId } };
            var updateFields = new { Status = newStatus };

            // Assuming you have a method in DbHelper to update an entity
            try
            {
                var rowsAffected = await _dbHelper.Update<Books>("books", updateFields, filter);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating the status of book with ID: {BookId}", bookId);
                return false;
            }
        }
    }
}
