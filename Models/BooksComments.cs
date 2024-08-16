using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json; // For Newtonsoft.Json

namespace LibraryAPI.Models
{
    public class BooksComments
    {
        [Key]
        public int CommentId { get; set; } // Primary Key

        [Required]
        public int UserId { get; set; } // Foreign key for Users

        [Required]
        public int BookId { get; set; } // Foreign key for Books

        [Required]
        [StringLength(45)]
        public string CommentTitle { get; set; } = string.Empty;  // Matches varchar(45) NOT NULL

        [Required]
        public string CommentText { get; set; } = string.Empty;  // Matches text NOT NULL

        [Column("CreatedDate")]
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;  // Matches timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP

        // Navigation properties
        [ForeignKey(nameof(UserId))]

        public Users? User { get; set; }

        [ForeignKey(nameof(BookId))]

        public Books? Book { get; set; }


    }
}
