using FluentValidation;
using PRN231.ExploreNow.BusinessObject.Models.Request;

namespace ExploreNow.Validations.Photo;

public class PhotoRequestValidator : AbstractValidator<PhotoRequest>
{
    public PhotoRequestValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("Photo Url is required!")
            .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute)).WithMessage("Invalid URL format");
    }
}