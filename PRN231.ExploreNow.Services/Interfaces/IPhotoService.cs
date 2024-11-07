using System.Threading.Tasks;
using PRN231.ExploreNow.BusinessObject.Models.Response;
using Microsoft.AspNetCore.Http;

namespace PRN231.ExploreNow.Services.Interfaces
{
	public interface IPhotoService
	{
		Task<PhotoResponse> GetPhotoByIdAsync(Guid id);
		Task<PhotoResponse> UpdatePhotoAsync(Guid photoId, Guid postId, IFormFile file);
	}
}
