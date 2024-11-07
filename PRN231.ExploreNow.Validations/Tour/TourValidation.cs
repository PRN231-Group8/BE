using FluentValidation;
using PRN231.ExploreNow.BusinessObject.Models.Request;
namespace PRN231.ExploreNow.Validations.Tour
{
	public class TourValidation : AbstractValidator<TourRequestModel>
	{
		public TourValidation()
		{
			RuleFor(x => x.Title)
				.NotEmpty().WithMessage("Title is required")
				.MaximumLength(100).WithMessage("Title length can't exceed 100 characters");

			RuleFor(x => x.Description)
				.NotEmpty().WithMessage("Description is required");

			RuleFor(x => x.Status)
				.IsInEnum().WithMessage("Invalid status");

			RuleFor(x => x.StartDate)
				.LessThan(x => x.EndDate).WithMessage("EndDate must less than StartDate ");

			RuleFor(x => x.LocationInTours)
				.Must(x => x != null && x.Any()).WithMessage("At least one location in tour must be selected.");

			RuleFor(x => x.TourMoods)
				.Must(x => x != null && x.Any()).WithMessage("At least one tour mood must be selected.");
		}
	}
}
