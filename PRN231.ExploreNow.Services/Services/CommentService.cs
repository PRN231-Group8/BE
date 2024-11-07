using PRN231.ExploreNow.BusinessObject.Entities;
using PRN231.ExploreNow.BusinessObject.Models.Request;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using PRN231.ExploreNow.Repositories.UnitOfWorks.Interfaces;
using PRN231.ExploreNow.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace PRN231.ExploreNow.Services.Services
{
	public class CommentService : ICommentService
	{
		private readonly IUnitOfWork _unitOfWork;

		public CommentService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<CommentResponse> AddCommentAsync(string userId, CommentRequest model)
		{
			var comment = new Comments
			{
				Code = GenerateUniqueCode(),
				Content = model.Content,
				PostId = model.PostId,
				UserId = userId,
				CreatedDate = DateTime.UtcNow,
				CreatedBy = userId,
			};

			try
			{
				await _unitOfWork.CommentRepository.AddAsync(comment);
				await _unitOfWork.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				throw new Exception("Error adding comment: " + ex.InnerException?.Message, ex);
			}

			// Fetch user details for the response
			var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
			if (user == null)
			{
				throw new Exception("User not found.");
			}

			return new CommentResponse
			{
				Id = comment.Id,
				Content = comment.Content,
				CreatedDate = comment.CreatedDate,
				PostId = (Guid)comment.PostId,
				User = new UserResponse
				{
					UserId = Guid.Parse(user.Id),
					FirstName = user.FirstName,
					LastName = user.LastName,
					Dob = user.Dob,
					Gender = user.Gender,
					Address = user.Address,
					AvatarPath = user.AvatarPath,
					CreatedDate = user.CreatedDate,
				}
			};
		}

		private string GenerateUniqueCode() => Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
	}
}
