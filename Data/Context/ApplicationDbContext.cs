using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Entities;

namespace PRN231.ExploreNow.Repositories.Context;

public class ApplicationDbContext : BaseDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Define DbSets for all your entities
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Comments> Comments { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<LocationRequest> LocationRequests { get; set; }
    public DbSet<Moods> Moods { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<Posts> Posts { get; set; }
    public DbSet<Tour> Tours { get; set; }
    public DbSet<TourTimestamp> TourTimestamps { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Transportation> Transportations { get; set; }

    // Configure relationships using Fluent API
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Explicitly configure relationships between ApplicationUser, Booking, and Transaction

        // ApplicationUser -> Booking (1-to-Many)
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.User)
            .WithMany(u => u.Bookings) // ApplicationUser has a collection of Bookings
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Define cascade delete behavior

        // ApplicationUser -> Transaction (1-to-Many)
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.User)
            .WithMany(u => u.Transactions) // ApplicationUser has a collection of Transactions
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Define cascade delete behavior

        // Add configurations for other entities if necessary (optional)

        // Example: You can add specific configurations for other relationships here as needed.
        // modelBuilder.Entity<Tour>().HasMany(...); etc.
    }
}