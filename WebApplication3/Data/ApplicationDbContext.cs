using Microsoft.EntityFrameworkCore;
using MyAspNetCoreApp.Models;

namespace MyAspNetCoreApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // Add DbSets for Students and Teachers
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Student properties
            modelBuilder.Entity<Student>()
                .Property(s => s.DateOfBirth)
                .HasColumnType("DATE") // Ensuring DateOfBirth uses the DATE type in the database
                .IsRequired(); // Marking it as required (if needed)

            // Configure Teacher properties
            modelBuilder.Entity<Teacher>()
                .Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(200); // Ensure Name is required and has a max length

            modelBuilder.Entity<Teacher>()
                .Property(t => t.Subject)
                .IsRequired()
                .HasMaxLength(100); // Ensure Subject is required and has a max length

            modelBuilder.Entity<Teacher>()
                .Property(t => t.Division)
                .IsRequired()
                .HasMaxLength(10); // Ensure Division is required and has a max length

            // Add any other configurations if necessary
        }
    }
}
