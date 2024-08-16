using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;

namespace LibraryAPI.Controllers
{
    [Route("rfs-library/[controller]")]
    [ApiController]
    public class BorrowerController : ControllerBase
    {
        private readonly ILogger<BorrowerController> _logger;
        private readonly LibraryDbContext _context;

        public BorrowerController(ILogger<BorrowerController> logger, LibraryDbContext context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpGet("longest-checked-out")]
        public async Task<IActionResult> GetLongestCheckedOutBooks()
        {
            _logger.LogInformation("Received request to GetLongestCheckedOutBooks");

            try
            {
                var borrowedBooks = await _context.BooksBorrowed
                    .Include(bb => bb.Book)
                    .Include(bb => bb.User)
                    .Where(bb => bb.Book != null && bb.User != null && bb.Book.Status == Books.BookStatus.CheckedOut)
                    .ToListAsync();

                var result = borrowedBooks
                    .GroupBy(bb => bb.BookId)
                    .Select(g => new
                    {
                        Book = new
                        {
                            BookId = g.First().Book?.BookId ?? 0,
                            BookName = g.First().Book?.BookName ?? "Unknown",
                            AuthorName = g.First().Book?.AuthorName ?? "Unknown",
                        },
                        User = new
                        {
                            UserId = g.First().User?.UserId ?? 0,
                            FirstName = g.First().User?.FirstName ?? "Unknown",
                            LastName = g.First().User?.LastName ?? "Unknown",
                        },
                        TotalDays = g.Sum(e => (DateTime.UtcNow - e.BorrowDate).TotalDays)
                    })
                    .OrderByDescending(x => x.TotalDays)
                    .Take(5)
                    .ToList();

                _logger.LogInformation("Retrieved longest checked out books: {resultCount} items", result.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching longest checked out books.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("borrow-book")]
        public async Task<ActionResult<int>> BorrowBook([FromBody] BooksBorrowed payload)
        {
            _logger.LogInformation("Starting BorrowBook method.");

            if (payload == null)
            {
                _logger.LogWarning("Received payload is null.");
                return BadRequest("Invalid data format. Payload is null.");
            }

            try
            {
                _logger.LogInformation("Received payload: {Payload}", payload);

                // Check if the required properties are present
                if (payload.BookId <= 0 || payload.UserId <= 0)
                {
                    _logger.LogWarning("Invalid payload: {Payload}. Invalid 'BookId' or 'UserId'.", payload);
                    return BadRequest("Invalid data format. 'BookId' and 'UserId' must be positive integers.");
                }

                // Retrieve book and user
                var book = await _context.Books.FindAsync(payload.BookId);
                if (book == null)
                {
                    _logger.LogWarning("Book not found with BookId: {BookId}", payload.BookId);
                    return NotFound("Book not found.");
                }

                var existingUser = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == payload.UserId);

                if (existingUser == null)
                {
                    _logger.LogWarning("User not found with UserId: {UserId}", payload.UserId);
                    return NotFound("User not found.");
                }

                var existingBorrow = await _context.BooksBorrowed
                    .AnyAsync(bb => bb.BookId == payload.BookId && bb.UserId == payload.UserId && bb.ReturnDate == null);

                if (existingBorrow)
                {
                    _logger.LogWarning("This book is already borrowed by the user. BookId: {BookId}, UserId: {UserId}", payload.BookId, payload.UserId);
                    return Conflict("This book is already borrowed by the user.");
                }

                book.Status = Books.BookStatus.CheckedOut;
                _context.Books.Update(book);

                var newBorrow = new BooksBorrowed
                {
                    BookId = payload.BookId,
                    UserId = payload.UserId,
                    BorrowDate = DateTime.UtcNow
                };

                _logger.LogInformation("Creating new borrow record: {NewBorrow}", newBorrow);
                _context.BooksBorrowed.Add(newBorrow);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Book borrowed successfully with BorrowId: {BorrowId}", newBorrow.BorrowId);
                return Ok(newBorrow.BorrowId);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update error occurred.");
                if (dbEx.InnerException is MySqlException mySqlEx && mySqlEx.Number == 1062)
                {
                    _logger.LogError("Duplicate entry error: {Message}", mySqlEx.Message);
                    return Conflict("A record with the same entry already exists.");
                }
                return StatusCode(500, "Database update error. Please check the database constraints.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error borrowing book.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }







        [HttpGet("{userId}/borrowed-books")]
        public async Task<ActionResult<List<dynamic>>> GetBorrowedBooks(int userId)
        {
            _logger.LogInformation("Received request to GetBorrowedBooks for UserId: {UserId}", userId);

            try
            {
                var borrowedBooks = await _context.BooksBorrowed
                    .Include(bb => bb.Book)
                    .Where(bb => bb.UserId == userId && bb.ReturnDate == null)
                    .ToListAsync();

                var result = borrowedBooks.Select(bb => new
                {
                    BookId = bb.BookId,
                    BookName = bb.Book?.BookName ?? "Unknown",
                    AuthorName = bb.Book?.AuthorName ?? "Unknown",
                    BorrowDate = bb.BorrowDate
                }).ToList();

                _logger.LogInformation("Retrieved borrowed books for UserId {UserId}: {resultCount} items", userId, result.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching borrowed books for UserId: {UserId}", userId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{userId}/borrowed-books/{bookId}")]
        public async Task<ActionResult> ReturnBook(int userId, int bookId)
        {
            _logger.LogInformation("Received request to ReturnBook: UserId={UserId}, BookId={BookId}", userId, bookId);

            try
            {
                var borrowedBook = await _context.BooksBorrowed
                    .FirstOrDefaultAsync(bb => bb.UserId == userId && bb.BookId == bookId && bb.ReturnDate == null);

                if (borrowedBook == null)
                {
                    _logger.LogWarning("BorrowedBook not found or already returned: UserId={UserId}, BookId={BookId}", userId, bookId);
                    return NotFound($"Book with ID {bookId} is either not borrowed by user {userId} or already returned.");
                }

                var book = await _context.Books.FindAsync(bookId);
                if (book == null)
                {
                    _logger.LogWarning("Book not found with BookId: {BookId}", bookId);
                    return NotFound($"Book with ID {bookId} does not exist.");
                }

                book.Status = Books.BookStatus.Available;
                _context.Books.Update(book);

                borrowedBook.ReturnDate = DateTime.UtcNow;
                _context.BooksBorrowed.Update(borrowedBook);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Book returned successfully: UserId={UserId}, BookId={BookId}", userId, bookId);
                return Ok("Book returned successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error returning book: UserId={UserId}, BookId={BookId}", userId, bookId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
