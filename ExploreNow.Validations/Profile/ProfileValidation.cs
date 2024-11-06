using FluentValidation;
using PRN231.ExploreNow.BusinessObject.Models.Request;

namespace PRN231.ExploreNow.Validations.Profile
{
	public class ProfileValidation : AbstractValidator<UserProfileRequestModel>
	{
		public ProfileValidation()
		{
			RuleFor(x => x.FirstName)
				.MaximumLength(50).WithMessage("FirstName not exceed 50 characters")
				.NotEmpty().WithMessage("FirstName is required");

			RuleFor(x => x.LastName)
				.MaximumLength(50).WithMessage("LastName not exceed 50 characters")
				.NotEmpty().WithMessage("LastName is required");

			RuleFor(x => x.Gender)
				.NotEmpty().WithMessage("Gender is required");

			RuleFor(x => x.Dob)
				.LessThan(DateTime.Now).WithMessage("Not a valid date")
				.NotEmpty().WithMessage("Date of birth is required");

			//RuleFor(x => x.AvatarPath)
			//	.NotEmpty().WithMessage("AvatarPath is required");
		}
	}
}
