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
                .MaximumLength(100).WithMessage("Title lenght can't exceed 100 characters");
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required");
            RuleFor(x => x.TotalPrice)
                .NotEmpty().WithMessage("Total price is required");
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid status");
            RuleFor(x => x.EndDate)
                .LessThan(x => x.StartDate).WithMessage("EndDate must less than StartDate ");
        }
    }
}
