using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryAPI.Models
{
    public class BooksBorrowed
    {
        [Key]
        public int BorrowId { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime BorrowDate { get; set; }

        public DateTime? ReturnDate { get; set; }  // Nullable return date

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public Users? User { get; set; } 

        [ForeignKey(nameof(BookId))]
        public Books? Book { get; set; }  
    }

}
