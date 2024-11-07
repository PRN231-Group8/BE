using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Enums;

namespace PRN231.ExploreNow.Repositories.Context
{
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

			// ApplicationUser configurations
			modelBuilder.Entity<ApplicationUser>(entity =>
			{
				entity.HasMany(u => u.Posts)
					  .WithOne(p => p.User)
					  .HasForeignKey(p => p.UserId);

				entity.HasMany(u => u.Transactions)
					  .WithOne(tr => tr.User)
					  .HasForeignKey(tr => tr.UserId);

				entity.HasMany(u => u.Payments)
					  .WithOne(p => p.User)
					  .HasForeignKey(p => p.UserId);

				entity.HasMany(u => u.Comments)
					  .WithOne(c => c.User)
					  .HasForeignKey(c => c.UserId);
			});

			// Tour configurations
			modelBuilder.Entity<Tour>(entity =>
			{
				entity.Property(t => t.Status)
					  .HasConversion(new EnumToStringConverter<TourStatus>());
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
					  .HasForeignKey<Transaction>(t => t.PaymentId);

				entity.Property(t => t.Status)
					  .HasConversion(new EnumToStringConverter<PaymentTransactionStatus>());
			});

			// Transportation configurations
			modelBuilder.Entity<Transportation>(entity =>
			{
				entity.HasOne(tp => tp.Tour)
					  .WithMany(t => t.Transportations)
					  .HasForeignKey(tp => tp.TourId);

				entity.Property(tp => tp.Type)
					  .HasConversion(new EnumToStringConverter<TransportationType>());
			});

			// LocationInTour configurations
			modelBuilder.Entity<LocationInTour>(entity =>
			{
				entity.HasOne(lit => lit.Tour)
					  .WithMany(t => t.LocationInTours)
					  .HasForeignKey(lit => lit.TourId);
			});

			// TourTimestamp configurations
			modelBuilder.Entity<TourTimestamp>(entity =>
			{
				entity.OwnsOne(tt => tt.PreferredTimeSlot, ts =>
				{
					ts.Property(p => p.StartTime).HasColumnName("PreferredStartTime");
					ts.Property(p => p.EndTime).HasColumnName("PreferredEndTime");
				});

				entity.HasOne(tt => tt.Tour)
					  .WithMany(t => t.TourTimestamps)
					  .HasForeignKey(tt => tt.TourId);

				entity.HasOne(tt => tt.Location)
					  .WithMany(l => l.TourTimestamps)
					  .HasForeignKey(tt => tt.LocationId);
			});

			// TourMood configurations
			modelBuilder.Entity<TourMood>(entity =>
			{
				entity.HasKey(tm => new { tm.TourId, tm.MoodId });

				entity.HasOne(tm => tm.Tour)
					  .WithMany(t => t.TourMoods)
					  .HasForeignKey(tm => tm.TourId);

				entity.HasOne(tm => tm.Mood)
					  .WithMany(m => m.TourMoods)
					  .HasForeignKey(tm => tm.MoodId);
			});

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
					  .HasForeignKey(p => p.UserId);

				entity.Property(p => p.Status)
					  .HasConversion(new EnumToStringConverter<PaymentStatus>());
			});

			// Posts configurations
			modelBuilder.Entity<Posts>(entity =>
			{
				entity.HasMany(p => p.Comments)
					  .WithOne(c => c.Post)
					  .HasForeignKey(c => c.PostId);

				entity.HasMany(p => p.Photos)
					  .WithOne(ph => ph.Post)
					  .HasForeignKey(ph => ph.PostId);

				entity.HasOne(p => p.User)
					  .WithMany(u => u.Posts)
					  .HasForeignKey(p => p.UserId);

				entity.Property(p => p.Status)
					  .HasConversion(new EnumToStringConverter<PostsStatus>())
					  .IsRequired();
			});

			// Comments configurations
			modelBuilder.Entity<Comments>(entity =>
			{
				entity.HasOne(c => c.Post)
					  .WithMany(p => p.Comments)
					  .HasForeignKey(c => c.PostId);
			});

			// Location configurations
			modelBuilder.Entity<Location>(entity =>
			{
				entity.HasMany(l => l.Photos)
					  .WithOne(p => p.Location)
					  .HasForeignKey(p => p.LocationId);

				entity.HasMany(l => l.TourTimestamps)
					  .WithOne(tt => tt.Location)
					  .HasForeignKey(tt => tt.LocationId);

				entity.OwnsOne(l => l.Address, addressBuilder =>
				{
					addressBuilder.Property(a => a.FullAddress).HasColumnName("Address");
					addressBuilder.Property(a => a.Longitude).HasColumnName("AddressLongitude");
					addressBuilder.Property(a => a.Latitude).HasColumnName("AddressLatitude");
				});
			});

			// Photo configurations
			modelBuilder.Entity<Photo>(entity =>
			{
				entity.HasOne(p => p.Location)
					  .WithMany(l => l.Photos)
					  .HasForeignKey(p => p.LocationId);

				entity.HasOne(p => p.Post)
					  .WithMany(po => po.Photos)
					  .HasForeignKey(p => p.PostId)
					  .IsRequired(false);

			});

			// Global DateTime conversion to UTC
			var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
				v => v.ToUniversalTime(),
				v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

			var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
				v => v.HasValue ? v.Value.ToUniversalTime() : v,
				v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

			foreach (var entityType in modelBuilder.Model.GetEntityTypes())
			{
				var dateTimeProperties = entityType.ClrType.GetProperties()
					.Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));

				foreach (var property in dateTimeProperties)
				{
					if (property.PropertyType == typeof(DateTime))
						modelBuilder.Entity(entityType.Name).Property(property.Name).HasConversion(dateTimeConverter);
					else if (property.PropertyType == typeof(DateTime?))
						modelBuilder.Entity(entityType.Name).Property(property.Name).HasConversion(nullableDateTimeConverter);
				}
			}
		}
	}
}
