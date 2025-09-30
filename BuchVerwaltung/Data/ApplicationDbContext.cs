// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using BuchVerwaltung.Models;

namespace BuchVerwaltung.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>()
                .HasIndex(b => b.Isbn)
                .IsUnique(); // ← Das macht ISBN eindeutig
        }
    }
}