using FluentValidation;
using PRN231.ExploreNow.BusinessObject.Models.Request;

namespace PRN231.ExploreNow.Validations.TourTrip
{
	public class TourTripValidator : AbstractValidator<TourTripRequest>
	{
		public TourTripValidator()
		{
			RuleFor(x => x.TourId)
				.NotEmpty().WithMessage("Tour ID is required")
				.NotEqual(Guid.Empty).WithMessage("Invalid Tour ID");

			RuleFor(x => x.TripDate)
				.NotEmpty().WithMessage("Trip date is required")
				.GreaterThan(DateTime.Now.Date)
				   .WithMessage("Trip date must be in the future");

			RuleFor(x => x.Price)
				.NotEmpty().WithMessage("Price is required")
				.GreaterThan(0).WithMessage("Price must be greater than 0")
				.PrecisionScale(18, 2, false)
					.WithMessage("Price cannot have more than 2 decimal places");

			RuleFor(x => x.TotalSeats)
				.NotEmpty().WithMessage("Total seats is required")
				.GreaterThan(0).WithMessage("Total seats must be greater than 0")
				.LessThanOrEqualTo(200)
					.WithMessage("Total seats cannot exceed 200");

			RuleFor(x => x.BookedSeats)
				.GreaterThanOrEqualTo(0)
					.WithMessage("Booked seats cannot be negative")
				.LessThanOrEqualTo(x => x.TotalSeats)
					.WithMessage("Booked seats cannot exceed total seats");

			RuleFor(x => x.TripStatus)
				.NotNull().WithMessage("Trip status is required");
		}
	}
}
