using FluentValidation;
using PRN231.ExploreNow.BusinessObject.Models.Request;

namespace PRN231.ExploreNow.Validations.TourTimeStamp
{
	public class TourTimeStampValidator : AbstractValidator<TourTimeStampRequest>
	{
		public TourTimeStampValidator()
		{
			RuleFor(x => x.Title)
				.NotEmpty().WithMessage("Title is required.")
				.MaximumLength(100).WithMessage("Title must not exceed 100 characters.");

			RuleFor(x => x.Description)
				.NotEmpty().WithMessage("Description is required.")
				.MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

			RuleFor(x => x.PreferredTimeSlot)
				.NotNull().WithMessage("Preferred time slot is required.");

			RuleFor(x => x.PreferredTimeSlot.StartTime)
				.NotEmpty().WithMessage("Start time is required.")
				.LessThan(x => x.PreferredTimeSlot.EndTime).WithMessage("Start time must be before end time.");

			RuleFor(x => x.PreferredTimeSlot.EndTime)
				.NotEmpty().WithMessage("End time is required.");

			RuleFor(x => x.TourId)
				.NotEmpty().WithMessage("Tour ID is required.");

			RuleFor(x => x.LocationId)
				.NotEmpty().WithMessage("Location ID is required");
		}
	}
}
