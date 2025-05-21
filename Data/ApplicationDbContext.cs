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
        public DbSet<Cuisine> Cuisines { get; set; } // Add DbSet for cuisines table
        public DbSet<Recipe> Recipes { get; set; } // Add DbSet for recipes table

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

            modelBuilder.Entity<Cuisine>(entity =>
            {
                entity.HasKey(c => c.Id); // Primary Key
            });

            modelBuilder.Entity<Recipe>(entity =>
            {
                entity.HasKey(r => r.Id); // Primary Key
                entity.HasIndex(r => r.Youtube_Link).IsUnique(); // Unique constraint on Youtube_Link

                entity.Property(r => r.Created_At)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Default value for Created_At

                // Add foreign key constraint
                entity.HasOne(r => r.Cuisine)
                    .WithMany(c => c.Recipes)
                    .HasForeignKey(r => r.Cuisine_Id)
                    .OnDelete(DeleteBehavior.Cascade); // Cascade delete
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

    public class Cuisine
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public ICollection<Recipe> Recipes { get; set; }
    }

    public class Recipe
    {
        public int Id { get; set; }
        public int Cuisine_Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string Content { get; set; }
        public string Youtube_Link { get; set; }
        public string Created_By { get; set; }
        public DateTime Created_At { get; set; } = DateTime.Now;
        public string Status { get; set; }

        public Cuisine Cuisine { get; set; }
    }
}