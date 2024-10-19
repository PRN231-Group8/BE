using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;

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

		modelBuilder.Entity<Tour>()
			.Property(d => d.Status)
			.HasConversion(new EnumToStringConverter<TourStatus>());

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

		// TourTimeStamp configurations
		modelBuilder.Entity<TourTimestamp>(entity =>
		{
			entity.OwnsOne(ar => ar.PreferredTimeSlot, ts =>
			{
				ts.Property(p => p.StartTime).HasColumnName("PreferredStartTime");
				ts.Property(p => p.EndTime).HasColumnName("PreferredEndTime");
			});

			entity.HasOne(tt => tt.Tour)
				.WithMany(t => t.TourTimestamps)
				.HasForeignKey(tt => tt.TourId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasOne(tt => tt.Location)
			   .WithMany(l => l.TourTimestamps)
			   .HasForeignKey(tt => tt.LocationId)
			   .OnDelete(DeleteBehavior.Cascade);
		});

		// TourMood configurations
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

		// TourTrip configurations
		modelBuilder.Entity<TourTrip>(entity =>
		{
			entity.HasOne(tt => tt.Tour)
				.WithMany(t => t.TourTrips)
				.HasForeignKey(tt => tt.TourId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.Property(tt => tt.TripStatus)
				.HasConversion(new EnumToStringConverter<TripStatus>());
		});

		// Payment configurations
		modelBuilder.Entity<Payment>(entity =>
		{
			entity.HasOne(p => p.TourTrip)
				.WithMany(tt => tt.Payments)
				.HasForeignKey(p => p.TourTripId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasOne(p => p.User)
				.WithMany(u => u.Payments)
				.HasForeignKey(p => p.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.Property(p => p.Status)
				.HasConversion(new EnumToStringConverter<PaymentStatus>());
		});

		// Transaction configurations
		modelBuilder.Entity<Transaction>(entity =>
		{
			entity.HasOne(t => t.User)
				.WithMany(u => u.Transactions)
				.HasForeignKey(t => t.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasOne(t => t.Payment)
				.WithOne(p => p.Transaction)
				.HasForeignKey<Transaction>(t => t.PaymentId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.Property(t => t.Status)
				.HasConversion(new EnumToStringConverter<PaymentTransactionStatus>());
		});
	}
}
