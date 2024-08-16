using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryAPI.Helpers;
using LibraryAPI.Models;
using Microsoft.Extensions.Logging;

namespace LibraryAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IDbHelper _dbHelper;
        private readonly ILogger<UserService> _logger;

        public UserService(IDbHelper dbHelper, ILogger<UserService> logger)
        {
            _dbHelper = dbHelper;
            _logger = logger;
        }

        public async Task<Users> FindUserByEmail(string email)
        {
            try
            {
                var filter = new Dictionary<string, object> { { "Email", email } };
                return await _dbHelper.FindOne<Users>("users", filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FindUserByEmail method.");
                throw;
            }
        }

        public async Task<Users> FindUserById(int userId)
        {
            try
            {
                var filter = new Dictionary<string, object> { { "UserId", userId } };
                return await _dbHelper.FindOne<Users>("users", filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in FindUserById method for userId: {userId}");
                throw;
            }
        }

        public async Task<int> CreateUser(Users userData)
        {
            try
            {
                return await _dbHelper.Create("users", userData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateUser method.");
                throw;
            }
        }

        public async Task<int> UpdateUser(Users updatedBody)
        {
            try
            {
                var filter = new Dictionary<string, object> { { "UserId", updatedBody.UserId } };
                return await _dbHelper.Update<Users>("users", updatedBody, filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in UpdateUser method for userId: {updatedBody.UserId}");
                throw;
            }
        }
        public async Task RehashPasswords()
        {
            try
            {
                var users = await _dbHelper.FindAll<Users>("users"); // Fetch all users from the database

                foreach (var user in users)
                {
                    // Check if the password needs rehashing
                    if (BCrypt.Net.BCrypt.Verify(user.Password, user.Password))
                    {
                        // Rehash the password
                        var newHashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);

                        // Update user with new hashed password
                        user.Password = newHashedPassword;
                        await UpdateUser(user); // Save changes to the database
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RehashPasswords method.");
                throw;
            }
        }


    }
}
