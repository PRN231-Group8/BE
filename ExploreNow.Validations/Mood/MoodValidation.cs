using FluentValidation;
using PRN231.ExploreNow.BusinessObject.Models.Request;

namespace PRN231.ExploreNow.Validations.Mood
{
	public class MoodValidation : AbstractValidator<MoodRequest>
	{
		public MoodValidation()
		{
			RuleFor(m => m.MoodTag)
				.NotEmpty().WithMessage("MoodTag is required");
			RuleFor(m => m.IconName)
				.NotEmpty().WithMessage("IconName is required");
		}
	}
}
