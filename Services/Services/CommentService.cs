using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Contracts.Repositories.Interfaces;
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
				User = new UserPostResponse
				{
					UserId = Guid.Parse(user.Id),
					FirstName = user.FirstName,
					LastName = user.LastName,
					AvatarPath = user.AvatarPath,
					CreatedDate = user.CreatedDate,
				}
			};
		}

		public async Task<List<CommentResponse>> GetCommentsByPostIdAsync(Guid id)
		{
			var comments = await _unitOfWork.CommentRepository
				.GetQueryable() // Use GetQueryable without lambda here
				.Where(c => c.PostId == id && !c.IsDeleted)
				.Include(c => c.User) // Assuming there’s a navigation property to User
				.ToListAsync();

			return comments.Select(comment => new CommentResponse
			{
				Id = comment.Id,
				Content = comment.Content,
				PostId = (Guid)comment.PostId,
				CreatedDate = comment.CreatedDate,
				User = new UserPostResponse
				{
					FirstName = comment.User.FirstName,
					LastName = comment.User.LastName,
					AvatarPath = comment.User.AvatarPath,
					CreatedDate = comment.User.CreatedDate,
				}
			}).ToList();
		}

		private string GenerateUniqueCode() => Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
	}
}
