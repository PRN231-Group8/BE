using FluentValidation;
using PRN231.ExploreNow.BusinessObject.Models.Request;

namespace ExploreNow.Validations.Location
{
	public class LocationRequestValidator : AbstractValidator<LocationsRequest>
	{
		public LocationRequestValidator()
		{
			RuleFor(x => x.Name)
				.NotEmpty().WithMessage("Name is required")
				.MaximumLength(100).WithMessage("Name length can't exceed 100 characters");

			RuleFor(x => x.Description)
				.NotEmpty().WithMessage("Description is required");

			RuleFor(x => x.Address)
				.NotEmpty().WithMessage("Address is required");

			RuleFor(x => x.Status)
				.IsInEnum().WithMessage("Invalid status");

			RuleFor(x => x.Temperature)
				.InclusiveBetween(-100, 100).WithMessage("Temperature must be between -100 and 100 degrees");
		}
	}
}