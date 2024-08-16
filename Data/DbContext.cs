using Microsoft.EntityFrameworkCore;
using LibraryAPI.Models;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace LibraryAPI.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
        {
        }

        public DbSet<Books> Books { get; set; }
        public DbSet<BooksBorrowed> BooksBorrowed { get; set; }
        public DbSet<BooksComments> BooksComments { get; set; }
        public DbSet<Users> Users { get; set; } // Corrected to match the User class name

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Books>()
                .Property(b => b.Status)
                .HasConversion<string>();

            modelBuilder.Entity<BooksBorrowed>()
                .HasKey(bb => bb.BorrowId);

            modelBuilder.Entity<BooksBorrowed>()
                .HasOne(bb => bb.Book)
                .WithMany(b => b.BooksBorrowed)
                .HasForeignKey(bb => bb.BookId);

            modelBuilder.Entity<BooksBorrowed>()
                .HasOne(bb => bb.User)
                .WithMany(u => u.BooksBorrowed)
                .HasForeignKey(bb => bb.UserId);

            modelBuilder.Entity<BooksComments>()
                .HasKey(bc => bc.CommentId);

            modelBuilder.Entity<BooksComments>()
                .HasOne(bc => bc.User)
                .WithMany(u => u.BooksComments)
                .HasForeignKey(bc => bc.UserId);

            modelBuilder.Entity<BooksComments>()
                .HasOne(bc => bc.Book)
                .WithMany(b => b.BooksComments)
                .HasForeignKey(bc => bc.BookId);

            modelBuilder.Entity<Users>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<Users>()
                .HasMany(u => u.BooksBorrowed)
                .WithOne(bb => bb.User)
                .HasForeignKey(bb => bb.UserId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
