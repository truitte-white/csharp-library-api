using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LibraryAPI.Models
{
    public class Books
    {
        [Key]
        public int BookId { get; set; }

        [Required]
        [MaxLength(255)]
        public string BookName { get; set; } = string.Empty; // Default to an empty string

        [MaxLength(255)]
        public string? AuthorName { get; set; } // Nullable to match database schema

        public int? PublishYear { get; set; } // Nullable to match database schema

        [MaxLength(50)]
        public string? Genre { get; set; } // Nullable to match database schema

        [Column("BookStatus")]
        public BookStatus Status { get; set; } = BookStatus.Available; // Default value

        // Navigation properties

        public ICollection<BooksBorrowed> BooksBorrowed { get; set; } = new List<BooksBorrowed>();
        [JsonIgnore]
        public ICollection<BooksComments> BooksComments { get; set; } = new List<BooksComments>();

        public enum BookStatus
        {
            Available,
            CheckedOut,
            Lost,
            Destroyed
        }
    }
}
