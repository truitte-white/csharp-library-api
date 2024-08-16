using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LibraryAPI.Data;
using LibraryAPI.Models;

namespace LibraryAPI.Helpers
{
    public class DbHelper : IDbHelper
    {
        private readonly LibraryDbContext _dbContext;

        public DbHelper(LibraryDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<T?> FindOne<T>(string tableName, Dictionary<string, object> filter, Dictionary<string, object>? options = null) where T : class
        {
            try
            {
                IQueryable<T> query = _dbContext.Set<T>();

                foreach (var (key, value) in filter)
                {
                    query = ApplyFilter(query, key, value);
                }

                return await query.FirstOrDefaultAsync(); // Returns null if no record is found
            }
            catch (Exception ex)
            {
                throw new Exception($"Error finding record in {tableName}: {ex.Message}", ex);
            }
        }

        public async Task<List<T>> FindAll<T>(string tableName, Dictionary<string, object>? filter = null, Dictionary<string, object>? options = null) where T : class
        {
            try
            {
                IQueryable<T> query = _dbContext.Set<T>();

                // Apply filters
                if (filter != null)
                {
                    foreach (var (key, value) in filter)
                    {
                        query = ApplyFilter(query, key, value);
                    }
                }

                return await query.ToListAsync(); // Returns a list of all records matching the filter
            }
            catch (Exception ex)
            {
                throw new Exception($"Error finding records in {tableName}: {ex.Message}", ex);
            }
        }

        public async Task<int> Create<T>(string tableName, T entity) where T : class
        {
            try
            {
                _dbContext.Set<T>().Add(entity);
                await _dbContext.SaveChangesAsync();
                return GetEntityId(entity);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating record in {tableName}: {ex.Message}", ex);
            }
        }

        public async Task<int> Update<T>(string tableName, object updatedBody, Dictionary<string, object> filter) where T : class
        {
            try
            {
                IQueryable<T> query = _dbContext.Set<T>();

                // Apply filters
                foreach (var (key, value) in filter)
                {
                    query = ApplyFilter(query, key, value);
                }

                var entityToUpdate = await query.FirstOrDefaultAsync();
                if (entityToUpdate != null)
                {
                    // Update fields
                    var updatedProperties = updatedBody.GetType().GetProperties();
                    foreach (var prop in updatedProperties)
                    {
                        var entityProperty = typeof(T).GetProperty(prop.Name);
                        if (entityProperty != null)
                        {
                            var currentValue = prop.GetValue(updatedBody);
                            entityProperty.SetValue(entityToUpdate, currentValue);
                        }
                    }

                    // Mark entity as modified
                    _dbContext.Entry(entityToUpdate).State = EntityState.Modified;

                    var rowsAffected = await _dbContext.SaveChangesAsync();
                    return rowsAffected;
                }

                throw new Exception($"Record not found in {tableName}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating record in {tableName}: {ex.Message}", ex);
            }
        }

        public IQueryable<T> ApplyFilter<T>(IQueryable<T> query, string key, object value) where T : class
        {
            ArgumentNullException.ThrowIfNull(query);

            var entityType = typeof(T);
            var property = entityType.GetProperty(key);
            if (property == null)
            {
                throw new Exception($"Property '{key}' does not exist on entity '{entityType.Name}'.");
            }

            return query.Where(e => EF.Property<object>(e, key).Equals(value));
        }

        public int GetEntityId<T>(T entity) where T : class
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null.");
            }

            // Look for a property named "Id" or other common names
            var idProp = typeof(T).GetProperties()
                                  .FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) && p.PropertyType == typeof(int));

            if (idProp != null)
            {
                return (int)idProp.GetValue(entity)!;
            }

            // If "Id" is not found, look for other common ID property names
            var alternativeIdProps = new[] { "Id", "UserId", "BookId" };
            foreach (var idPropName in alternativeIdProps)
            {
                idProp = typeof(T).GetProperty(idPropName, BindingFlags.Public | BindingFlags.Instance);
                if (idProp != null && idProp.PropertyType == typeof(int))
                {
                    return (int)idProp.GetValue(entity)!;
                }
            }

            // If no suitable property is found, throw an exception
            throw new InvalidOperationException($"Entity of type {typeof(T).Name} does not have an integer ID property.");
        }

        public async Task<List<Books>> GetLongestCheckedOutBooks()
        {
            try
            {
                var longestCheckedOutBooks = await _dbContext.BooksBorrowed
                    .GroupBy(bb => bb.BookId)
                    .Select(g => new
                    {
                        BookId = g.Key,
                        TotalDays = g.Sum(bb => (DateTime.UtcNow - bb.BorrowDate).TotalDays)
                    })
                    .OrderByDescending(x => x.TotalDays)
                    .Take(5)
                    .Join(_dbContext.Books,
                        bb => bb.BookId,
                        b => b.BookId,
                        (bb, b) => new Books
                        {
                            BookId = b.BookId,
                            BookName = b.BookName,
                            AuthorName = b.AuthorName,
                            PublishYear = b.PublishYear,
                            Status = Books.BookStatus.Available // Assigning BookStatus here
                        })
                    .ToListAsync();

                return longestCheckedOutBooks;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve longest checked out books: {ex.Message}", ex);
            }
        }

        public async Task<List<BooksComments>> GetLatestComments()
        {
            try
            {
                var latestComments = await _dbContext.BooksComments
                    .OrderByDescending(c => c.CreatedDate)
                    .Take(5)
                    .ToListAsync();

                return latestComments;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve latest comments: {ex.Message}", ex);
            }
        }
    }
}
