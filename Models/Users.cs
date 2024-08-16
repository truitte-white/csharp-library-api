using LibraryAPI.Models;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace LibraryAPI.Models
{
    public class Users
    {
        [Key]
        public int UserId { get; set; }  // Primary key

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty; // Initialize with default value

        [Required]
        [StringLength(45)]
        public string LastName { get; set; } = string.Empty; // Initialize with default value

        [Required]
        [StringLength(100)]
        [EmailAddress] // Optional: Validate email format
        public string Email { get; set; } = string.Empty; // Initialize with default value

        [Required]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty; // Initialize with default value

        [StringLength(255)]
        public string? Token { get; set; } = string.Empty; // Initialize with default value

        [Required]
        public DateTime JoinDate { get; set; } = DateTime.Now; // Initialize with default value

        // Navigation property
        [JsonIgnore]
        public ICollection<BooksBorrowed> BooksBorrowed { get; set; } = new List<BooksBorrowed>(); // Initialize with an empty list
        [JsonIgnore]
        public ICollection<BooksComments> BooksComments { get; set; } = new List<BooksComments>(); // Ensure this property exists
        // Constructor
        public Users()
        {
            // Optionally initialize other properties or perform setup here
        }
    }
}
