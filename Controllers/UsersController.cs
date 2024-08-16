using System;
using System.Security.Claims;
using System.Threading.Tasks;
using LibraryAPI.Helpers;
using LibraryAPI.Models;
using LibraryAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LibraryAPI.Controllers
{
    [ApiController]
    [Route("rfs-library/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IBorrowerService _borrowerService;
        private readonly ILogger<UsersController> _logger;
        private readonly ITokenHelper _tokenHelper;

        public UsersController(IUserService userService, IBorrowerService borrowerService, ITokenHelper tokenHelper, ILogger<UsersController> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _borrowerService = borrowerService ?? throw new ArgumentNullException(nameof(borrowerService));
            _tokenHelper = tokenHelper ?? throw new ArgumentNullException(nameof(tokenHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("email/{email}")]
        public async Task<ActionResult<Users>> GetUserByEmail(string email)
        {
            _logger.LogInformation("Received request to GetUserByEmail with email: {Email}", email);

            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Email parameter is null or empty.");
                return BadRequest("Email cannot be null or empty.");
            }

            try
            {
                _logger.LogInformation("Fetching user by email: {Email}", email);
                var user = await _userService.FindUserByEmail(email);
                if (user == null)
                {
                    _logger.LogInformation("User not found with email: {Email}", email);
                    return NotFound();
                }

                _logger.LogInformation("User found with email: {Email}", email);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserByEmail method for email: {Email}", email);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("id/{userId}")]
        public async Task<ActionResult<Users>> GetUserById(int userId)
        {
            _logger.LogInformation("Received request to GetUserById with userId: {UserId}", userId);

            try
            {
                _logger.LogInformation("Fetching user by userId: {UserId}", userId);
                var user = await _userService.FindUserById(userId);
                if (user == null)
                {
                    _logger.LogInformation("User not found with userId: {UserId}", userId);
                    return NotFound();
                }

                _logger.LogInformation("User found with userId: {UserId}", userId);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserById method for userId: {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateUser([FromBody] Users newUser)
        {
            _logger.LogInformation("Received request to CreateUser with data: {NewUser}", newUser);

            try
            {
                if (newUser == null)
                {
                    _logger.LogWarning("Received null data for CreateUser.");
                    return BadRequest("User data cannot be null.");
                }

                _logger.LogInformation("Creating user with data: {NewUser}", newUser);
                var result = await _userService.CreateUser(newUser);

                _logger.LogInformation("User created successfully with result: {Result}", result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateUser method.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{userId}")]
        public async Task<ActionResult<int>> UpdateUser(int userId, [FromBody] Users updatedUser)
        {
            _logger.LogInformation("Received request to UpdateUser with userId: {UserId} and data: {UpdatedUser}", userId, updatedUser);

            try
            {
                if (updatedUser == null)
                {
                    _logger.LogWarning("Received null data for UpdateUser.");
                    return BadRequest("User data cannot be null.");
                }

                if (userId != updatedUser.UserId)
                {
                    _logger.LogWarning("User ID mismatch: Provided ID {UserId} does not match updatedUser ID {UpdatedUserId}", userId, updatedUser.UserId);
                    return BadRequest("User ID mismatch.");
                }

                _logger.LogInformation("Updating user with ID: {UserId}", userId);
                var result = await _userService.UpdateUser(updatedUser);

                _logger.LogInformation("User updated successfully with result: {Result}", result);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateUser method for userId: {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult> GetProfile()
        {
            _logger.LogInformation("Received request to GetProfile");

            try
            {
                var userId = Convert.ToInt32(User.FindFirst(ClaimTypes.NameIdentifier)?.Value); // Extract userId from claims
                _logger.LogInformation("Extracted userId from claims: {UserId}", userId);

                _logger.LogInformation("Fetching all borrowed books for userId: {UserId}", userId);
                var allBorrowedBooks = await _borrowerService.FindAllBorrowedBooks(userId);

                _logger.LogInformation("Retrieved all borrowed books for userId: {UserId}", userId);
                return Ok(allBorrowedBooks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetProfile method for userId: {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] Users loginUser)
        {
            _logger.LogInformation("Received request to Login with data: {LoginUser}", loginUser);

            if (string.IsNullOrEmpty(loginUser?.Email) || string.IsNullOrEmpty(loginUser?.Password))
            {
                _logger.LogWarning("Email or Password is null or empty.");
                return BadRequest("Email and Password cannot be null or empty.");
            }

            try
            {
                var user = await _userService.FindUserByEmail(loginUser.Email);
                if (user == null)
                {
                    _logger.LogWarning("User not found with email: {Email}", loginUser.Email);
                    return Unauthorized("User does not exist with this email.");
                }

                var passwordMatch = BCrypt.Net.BCrypt.Verify(loginUser.Password, user.Password);
                if (!passwordMatch)
                {
                    _logger.LogWarning("Incorrect password for email: {Email}", loginUser.Email);
                    return Unauthorized("Incorrect password.");
                }

                var token = _tokenHelper.Sign(user.UserId); // Use TokenHelper's Sign method
                user.Token = token;
                await _userService.UpdateUser(user);

                // Set cookie
                Response.Cookies.Append("token", token, new CookieOptions { HttpOnly = true, MaxAge = TimeSpan.FromHours(1) });

                _logger.LogInformation("Login successful for email: {Email}", loginUser.Email);
                return Ok(new { Message = "Login successful", Token = token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Login method for email: {Email}", loginUser.Email);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("signup")]
        public async Task<ActionResult> Signup([FromBody] Users signupUser)
        {
            _logger.LogInformation("Received request to Signup with data: {SignupUser}", signupUser);

            if (string.IsNullOrEmpty(signupUser?.Email) || string.IsNullOrEmpty(signupUser?.Password) ||
                string.IsNullOrEmpty(signupUser?.FirstName) || string.IsNullOrEmpty(signupUser?.LastName))
            {
                _logger.LogWarning("Required fields are null or empty.");
                return BadRequest("All fields are required.");
            }

            try
            {
                var existingUser = await _userService.FindUserByEmail(signupUser.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("User already exists with email: {Email}", signupUser.Email);
                    return BadRequest("User already exists.");
                }

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(signupUser.Password);
                var newUser = new Users
                {
                    FirstName = signupUser.FirstName,
                    LastName = signupUser.LastName,
                    Email = signupUser.Email,
                    Password = hashedPassword
                };

                var userId = await _userService.CreateUser(newUser);
                _logger.LogInformation("User signed up successfully with userId: {UserId}", userId);

                return Ok(new { UserId = userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Signup method for email: {Email}", signupUser.Email);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("rehash-passwords")]
        public async Task<ActionResult> RehashPasswords()
        {
            _logger.LogInformation("Received request to RehashPasswords");

            try
            {
                await _userService.RehashPasswords();
                _logger.LogInformation("Passwords rehashed successfully.");
                return Ok("Passwords rehashed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RehashPasswords method.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
