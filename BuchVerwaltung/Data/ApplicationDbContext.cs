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
    }
}