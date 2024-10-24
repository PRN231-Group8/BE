using FluentValidation;
using PRN231.ExploreNow.BusinessObject.Models.Request;

namespace PRN231.ExploreNow.Validations.Posts
{
	public class PostsRequestValidator : AbstractValidator<PostsRequest>
	{
		public PostsRequestValidator()
		{
			RuleFor(x => x.Content)
				.NotEmpty().WithMessage("Content is required.")
				.MaximumLength(200).WithMessage("Content must not exceed 200 characters.");

			RuleFor(x => x.Status)
				.IsInEnum().When(x => x.Status.HasValue)
				.WithMessage("Status must be a valid value (Pending, Approved, or Rejected).");

			// If RemoveAllComments is true, CommentsToRemove must be null.
			When(x => x.RemoveAllComments == true, () =>
			{
				RuleFor(x => x.CommentsToRemove)
					.Empty().WithMessage("CommentsToRemove must be null when RemoveAllComments is true.");
			});

			When(x => x.CommentsToRemove != null && x.CommentsToRemove.Any(), () =>
			{
				RuleFor(x => x.CommentsToRemove)
					.ForEach(commentId =>
					{
						commentId.NotEmpty().WithMessage("Comment ID cannot be empty.")
							.Must(BeValidGuid).WithMessage("Invalid GUID format for Comment ID.");
					});
			});

			// If RemoveAllPhotos is true, PhotosToRemove must be null
			When(x => x.RemoveAllPhotos == true, () =>
			{
				RuleFor(x => x.PhotosToRemove)
					.Empty().WithMessage("PhotosToRemove must be null when RemoveAllPhotos is true.");
			});

			When(x => x.PhotosToRemove != null && x.PhotosToRemove.Any(), () =>
			{
				RuleFor(x => x.PhotosToRemove)
					.ForEach(photoId =>
					{
						photoId.NotEmpty().WithMessage("Photo ID cannot be empty.")
							.Must(BeValidGuid).WithMessage("Invalid GUID format for Photo ID.");
					});
			});
		}

		private bool BeValidGuid(string id)
		{
			return Guid.TryParse(id, out _);
		}
	}
}
