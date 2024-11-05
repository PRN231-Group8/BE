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

			RuleFor(x => x.Longitude)
				.Must(lng => !lng.HasValue || (lng >= -180 && lng <= 180))
				.WithMessage("Longitude must be between -180 and 180 degrees");

			RuleFor(x => x.Latitude)
				.Must(lat => !lat.HasValue || (lat >= -90 && lat <= 90))
				.WithMessage("Latitude must be between -90 and 90 degrees");

			RuleFor(x => x)
				.Must(x => (!x.Latitude.HasValue && !x.Longitude.HasValue) || (x.Latitude.HasValue && x.Longitude.HasValue))
				.WithMessage("Both Latitude and Longitude must be provided together");
		}
	}
}