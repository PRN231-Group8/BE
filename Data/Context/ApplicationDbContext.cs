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
    public DbSet<ApplicationUser> Users { get; set; } 
    public DbSet<Comments> Comments { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<LocationInTour> LocationInTours { get; set; } 
    public DbSet<Moods> Moods { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<Posts> Posts { get; set; }
    public DbSet<Tour> Tours { get; set; }
    public DbSet<TourTimestamp> TourTimestamps { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Transportation> Transportations { get; set; }
    public DbSet<TourTrip> TourTrips { get; set; }
    public DbSet<TourMood> TourMoods { get; set; }
    public DbSet<Payment> Payments { get; set; }  

    // Configure relationships using Fluent API
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Explicitly configure relationships between ApplicationUser and Tour (formerly Booking)

        // ApplicationUser -> Tour (1-to-Many)
        modelBuilder.Entity<Tour>()
            .HasOne(t => t.User)
            .WithMany(u => u.Tours)  // ApplicationUser now has a collection of Tours
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);  // Define cascade delete behavior

        // ApplicationUser -> Transaction (1-to-Many)
        modelBuilder.Entity<Transaction>()
            .HasOne(tr => tr.User)
            .WithMany(u => u.Transactions)  // ApplicationUser has a collection of Transactions
            .HasForeignKey(tr => tr.UserId)
            .OnDelete(DeleteBehavior.Cascade);  // Define cascade delete behavior

        // Transportation -> Tour (Many-to-1)
        modelBuilder.Entity<Transportation>()
            .HasOne(tp => tp.Tour)
            .WithMany(t => t.Transportations)  // Tour has a collection of Transportations
            .HasForeignKey(tp => tp.TourId)
            .OnDelete(DeleteBehavior.Cascade);  // Define cascade delete behavior

        // Tour -> LocationInTour (1-to-Many)
        modelBuilder.Entity<LocationInTour>()
            .HasOne(lit => lit.Tour)
            .WithMany(t => t.LocationInTours)  // Tour has a collection of LocationInTours
            .HasForeignKey(lit => lit.TourId)
            .OnDelete(DeleteBehavior.Cascade);  // Define cascade delete behavior

        modelBuilder.Entity<TourMood>()
        .HasKey(tm => new { tm.TourId, tm.MoodId });

        modelBuilder.Entity<TourMood>()
            .HasOne(tm => tm.Tour)
            .WithMany(t => t.TourMoods)
            .HasForeignKey(tm => tm.TourId);

        modelBuilder.Entity<TourMood>()
            .HasOne(tm => tm.Mood)
            .WithMany(m => m.TourMoods)
            .HasForeignKey(tm => tm.MoodId);
    }
}
