using Microsoft.EntityFrameworkCore;

namespace CulinaryCraftWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define DbSets for your tables
        public DbSet<User> Users { get; set; } // Add DbSet for users table

        // Fluent API for additional constraints
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id); // Primary Key
                entity.HasIndex(u => u.Email).IsUnique(); // Unique constraint on Email

                // Add the check constraint on the entity level
                entity.HasCheckConstraint("CK_User_Role", "[Role] IN ('user', 'admin', 'superadmin')");
            });
        }
    }

    // Define the model for the users table
    public class User
    {
        public string Id { get; set; } // Matches VARCHAR(50), PRIMARY KEY
        public string Name { get; set; } // Matches VARCHAR(50), NOT NULL
        public string Email { get; set; } // Matches VARCHAR(100), NOT NULL, UNIQUE
        public string PasswordHash { get; set; } // Matches VARCHAR(255), NOT NULL
        public string? ProfileImage { get; set; } // Matches VARCHAR(255), nullable
        public DateTime RegisteredDate { get; set; } = DateTime.Now; // Matches DATE with default
        public string Status { get; set; } = "active"; // Matches VARCHAR(20) with default
        public string Role { get; set; } // Matches VARCHAR(20) with CHECK constraint
    }
}