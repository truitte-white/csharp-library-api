using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryAPI.Models;
using LibraryAPI.Services;
using LibraryAPI.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using LibraryAPI.Data;
using Newtonsoft.Json.Linq;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("rfs-library/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly ILogger<BooksController> _logger;
        private readonly IBookService _bookService;
        private readonly LibraryDbContext _context;

        public BooksController(ILogger<BooksController> logger, IBookService bookService, LibraryDbContext context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpGet]
        public async Task<ActionResult<List<Books>>> GetAllBooks()
        {
            try
            {
                _logger.LogInformation("Fetching all books.");
                var books = await _bookService.GetAllBooks();
                _logger.LogInformation("Books fetched: {Count} items.", books.Count);
                return Ok(books);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all books.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{bookId}")]
        public async Task<ActionResult<Books>> GetBookById(int bookId)
        {
            _logger.LogInformation("Fetching book by ID: {BookId}", bookId);

            try
            {
                var book = await _bookService.FindBookById(bookId);
                if (book == null)
                {
                    _logger.LogWarning("Book with ID {BookId} not found.", bookId);
                    return NotFound();
                }
                _logger.LogInformation("Book found: {BookId} - Title: {BookName}", bookId, book.BookName);
                return Ok(book);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching book by ID: {BookId}", bookId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("add-book")]
        public async Task<ActionResult<int>> AddBook([FromBody] Books book)
        {
            _logger.LogInformation("Received request to AddBook with data: {Book}", book);

            if (book == null)
            {
                _logger.LogWarning("Received null data for AddBook.");
                return BadRequest("Book data cannot be null.");
            }

            try
            {
                var result = await _bookService.CreateBook(book);

                _logger.LogInformation("Book added successfully with result: {Result}", result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddBook method.");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPut("{bookId}")]
        public async Task<ActionResult<int>> UpdateBook(int bookId, [FromBody] Books updateFields)
        {
            _logger.LogInformation("Updating book with ID: {BookId}", bookId);
            _logger.LogInformation("Data Received: {UpdateFields}", updateFields);
            _logger.LogInformation("Data Type: {DataType}", updateFields.GetType().FullName);

            try
            {
                var result = await _bookService.UpdateBook(bookId, updateFields);
                _logger.LogInformation("Book updated successfully. ID: {BookId}", bookId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating book with ID: {BookId}", bookId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{bookId}/edit-status")]
        public async Task<ActionResult> UpdateBookStatus(int bookId, [FromBody] dynamic payload)
        {
            _logger.LogInformation("Received request to UpdateBookStatus: BookId={BookId}", bookId);

            try
            {
                // Convert dynamic payload to JObject
                var jsonPayload = (JObject)payload;

                if (jsonPayload == null || !jsonPayload.ContainsKey("NewStatus"))
                {
                    _logger.LogWarning("Invalid or missing NewStatus in request payload.");
                    return BadRequest("Invalid or missing NewStatus.");
                }

                var newStatusString = jsonPayload["NewStatus"]?.ToString();

                if (string.IsNullOrWhiteSpace(newStatusString))
                {
                    _logger.LogWarning("NewStatus is null or empty.");
                    return BadRequest("NewStatus cannot be null or empty.");
                }

                if (!Enum.TryParse(newStatusString, ignoreCase: true, out Books.BookStatus newStatus))
                {
                    _logger.LogWarning("Received invalid status: {NewStatus}", newStatusString);
                    return BadRequest("Invalid status value.");
                }

                var book = await _context.Books.FindAsync(bookId);
                if (book == null)
                {
                    _logger.LogWarning("Book not found with BookId: {BookId}", bookId);
                    return NotFound($"Book with ID {bookId} does not exist.");
                }

                book.Status = newStatus;
                _context.Books.Update(book);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Book status updated successfully: BookId={BookId}, NewStatus={NewStatus}", bookId, newStatus);
                return Ok("Book status updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating book status: BookId={BookId}", bookId);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }




    }
}
