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
			return new CommentResponse
			{
				Id = comment.Id,
				Content = comment.Content,
				CreatedDate = comment.CreatedDate,
				UserId = comment.UserId,
				PostId = (Guid)comment.PostId,
			};
		}
		private string GenerateUniqueCode() => Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
	}
}
