using FluentValidation;
using Microsoft.AspNetCore.Http;
using PRN231.ExploreNow.BusinessObject.Models.Request;

namespace PRN231.ExploreNow.Validations.Posts
{
    public class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
    {
        public CreatePostRequestValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Content is required.")
                .MaximumLength(1000).WithMessage("Content can't exceed 1000 characters.");

            RuleFor(x => x.Photos)
                .NotEmpty().WithMessage("Please upload at least one image.")
                .Must(photos => photos.Count <= 5).WithMessage("You can upload up to 5 images only.")
                .ForEach(photo =>
                {
                    photo.Must(file => file.Length <= 3 * 1024 * 1024)
                         .WithMessage("Each file must be smaller than 3MB.");
                });
        }
    }
}
