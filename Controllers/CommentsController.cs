using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryAPI.Models;
using LibraryAPI.Services;

namespace LibraryAPI.Controllers
{
    [Route("rfs-library/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentsController> _logger;

        public CommentsController(ICommentService commentService, ILogger<CommentsController> logger)
        {
            _commentService = commentService ?? throw new ArgumentNullException(nameof(commentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("add-comment")]
        public async Task<ActionResult<int>> AddComment([FromBody] BooksComments comment)
        {
            _logger.LogInformation("Received request to AddComment with data: {@Comment}", comment);

            if (comment == null)
            {
                _logger.LogWarning("Received null data for AddComment.");
                return BadRequest("Comment data cannot be null.");
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _commentService.CreateCommentAsync(comment);
                _logger.LogInformation("Comment added successfully with ID: {CommentId}", result);
                return CreatedAtAction(nameof(GetCommentById), new { commentId = result }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding comment.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{commentId}")]
        public async Task<IActionResult> UpdateComment(int commentId, [FromBody] BooksComments comment)
        {
            if (commentId != comment.CommentId || !ModelState.IsValid)
                return BadRequest("Invalid data.");

            _logger.LogInformation("Updating comment with ID {CommentId}", commentId);

            try
            {
                bool success = await _commentService.UpdateCommentAsync(commentId, comment);
                if (success)
                    return NoContent();
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{commentId}/{userId}")]
        public async Task<IActionResult> DeleteComment(int commentId, int userId)
        {
            _logger.LogInformation("Deleting comment with ID {CommentId} by user {UserId}", commentId, userId);

            try
            {
                bool success = await _commentService.DeleteCommentAsync(commentId, userId);
                if (success)
                    return NoContent();
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{commentId}")]
        public async Task<IActionResult> GetCommentById(int commentId)
        {
            _logger.LogInformation("Fetching comment with ID {CommentId}", commentId);

            try
            {
                var comment = await _commentService.GetCommentByIdAsync(commentId);
                if (comment == null)
                    return NotFound();
                return Ok(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comment");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("latest-comments")]
        public async Task<IActionResult> GetLatestComments()
        {
            _logger.LogInformation("Fetching latest comments");

            try
            {
                var latestComments = await _commentService.GetLatestCommentsAsync();
                _logger.LogInformation("Fetched {Count} comments", latestComments.Count);
                return Ok(latestComments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest comments");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetCommentsByUserId(int userId)
        {
            _logger.LogInformation("Fetching comments for user with ID {UserId}", userId);

            try
            {
                var userComments = await _commentService.GetCommentsByUserIdAsync(userId);
                return Ok(userComments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving comments for user");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
