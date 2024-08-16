using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryAPI.Data;
using LibraryAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LibraryAPI.Services
{
    public class CommentService : ICommentService
    {
        private readonly LibraryDbContext _dbContext;
        private readonly ILogger<CommentService> _logger;

        public CommentService(LibraryDbContext dbContext, ILogger<CommentService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> CreateCommentAsync(BooksComments comment)
        {
            if (comment == null)
            {
                _logger.LogWarning("Received null comment data for creation.");
                throw new ArgumentNullException(nameof(comment), "Comment data cannot be null.");
            }

            try
            {
                // Set creation date
                comment.CreatedDate = DateTime.UtcNow;

                // Add the comment to the database
                _dbContext.BooksComments.Add(comment);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Comment created successfully with ID: {CommentId}", comment.CommentId);
                return comment.CommentId;
            }
            catch (DbUpdateException dbEx)
            {
                // Log database update exceptions
                _logger.LogError(dbEx, "Database update error occurred while creating a comment.");
                throw; // Rethrow the exception to be handled by the controller
            }
            catch (Exception ex)
            {
                // Log general exceptions
                _logger.LogError(ex, "Error occurred while creating a comment.");
                throw; // Rethrow the exception to be handled by the controller
            }
        }

        public async Task<bool> UpdateCommentAsync(int commentId, BooksComments updatedComment)
        {
            var existingComment = await _dbContext.BooksComments.FindAsync(commentId);
            if (existingComment == null)
                return false;

            existingComment.CommentTitle = updatedComment.CommentTitle;
            existingComment.CommentText = updatedComment.CommentText;
            _dbContext.BooksComments.Update(existingComment);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCommentAsync(int commentId, int userId)
        {
            var commentToDelete = await _dbContext.BooksComments
                .FirstOrDefaultAsync(c => c.CommentId == commentId && c.UserId == userId);

            if (commentToDelete == null)
                return false;

            _dbContext.BooksComments.Remove(commentToDelete);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<BooksComments?> GetCommentByIdAsync(int commentId)
        {
            return await _dbContext.BooksComments
                .Include(c => c.Book)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CommentId == commentId);
        }

        public async Task<List<BooksComments>> GetLatestCommentsAsync()
        {
            try
            {
                return await _dbContext.BooksComments
                    .OrderByDescending(c => c.CreatedDate)
                    .Take(5)
                    .Include(c => c.Book)
                    .Include(c => c.User)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching latest comments");
                throw; // Rethrow the exception to be handled by the controller
            }
        }

        public async Task<List<BooksComments>> GetCommentsByUserIdAsync(int userId)
        {
            return await _dbContext.BooksComments
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedDate)
                .Include(c => c.Book)
                .Include(c => c.User)
                .ToListAsync();
        }
    }
}
