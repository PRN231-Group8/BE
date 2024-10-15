using PRN231.ExploreNow.BusinessObject.Enums;
using PRN231.ExploreNow.BusinessObject.OtherObjects;
using System.Text.Json.Serialization;

namespace PRN231.ExploreNow.BusinessObject.Models.Request
{
	public class PostsRequest
	{
		public string? Content { get; set; }

		[JsonConverter(typeof(PostsStatusConverter))]
		public PostsStatus? Status { get; set; }
		public bool? RemoveAllComments { get; set; }
		public List<string>? CommentsToRemove { get; set; }
		public bool? RemoveAllPhotos { get; set; }
		public List<string>? PhotosToRemove { get; set; }
	}
}
